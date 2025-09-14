using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Model.Serialization;
using SharpCompress.Readers;
using SubdivX.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SubdivX
{
    public class SubdivXProvider : ISubtitleProvider, IHasOrder
    {
        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILibraryManager _libraryManager;

        private PluginConfiguration _configuration => Plugin.Instance.GetConfiguration();

        public string Name => "SubdivX";

        public IEnumerable<VideoContentType> SupportedMediaTypes => new List<VideoContentType>
            { VideoContentType.Episode, VideoContentType.Movie };

        public int Order => 1;

        private const string UserAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36";

        public SubdivXProvider(ILogger logger
            , IJsonSerializer jsonSerializer
            , ILibraryManager libraryManager)
        {
            _logger = logger;
            _jsonSerializer = jsonSerializer;
            _libraryManager = libraryManager;
        }

        public async Task<IEnumerable<RemoteSubtitleInfo>> Search(SubtitleSearchRequest request,
            CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                var json = _jsonSerializer.SerializeToString(request);
                _logger.Debug($"SubdivX Search | Request-> {json}");
            }, cancellationToken);

            _logger.Debug($"SubdivX Search | UseOriginalTitle-> {_configuration?.UseOriginalTitle}");

            if (!string.Equals(request.Language, "ES", StringComparison.OrdinalIgnoreCase))
            {
                return Array.Empty<RemoteSubtitleInfo>();
            }

            var item = _libraryManager.FindByPath(request.MediaPath, false);

            if (request.ContentType == VideoContentType.Episode)
            {
                var name = request.SeriesName;
                if (_configuration?.UseOriginalTitle == true)
                    if (!string.IsNullOrWhiteSpace(item.OriginalTitle))
                        name = item.OriginalTitle;

                var query = $"{name} S{request.ParentIndexNumber:D2}E{request.IndexNumber:D2}";

                var subtitles = SearchSubtitles(query);
                if (subtitles?.Count > 0)
                    return subtitles;
            }
            else
            {
                var name = request.Name;
                if (_configuration?.UseOriginalTitle == true)
                    if (!string.IsNullOrWhiteSpace(item.OriginalTitle))
                        name = item.OriginalTitle;

                var query = $"{name} {request.ProductionYear}";

                var subtitles = SearchSubtitles(query);
                if (subtitles?.Count > 0)
                    return subtitles;
            }

            return Array.Empty<RemoteSubtitleInfo>();
        }

        public async Task<SubtitleResponse> GetSubtitles(string id, CancellationToken cancellationToken)
        {
            await Task.Run(() => { _logger.Debug($"SubdivX GetSubtitles id: {id}"); }, cancellationToken);

            var subtitle = DownloadSubtitle(id);
            return subtitle ?? new SubtitleResponse();
        }

        private List<RemoteSubtitleInfo> SearchSubtitles(string query)
        {
            SubdivxAjaxResponse data;
            using (var client = new HttpClient())
            {
                var fs = new FlareSolverrClientNetStd(_configuration.FlareSolverrUrl, "subdivx", _jsonSerializer, client);
                var svc = new SubdivxServiceNetStd(fs, _jsonSerializer);
                data = svc.AjaxWithRecoveryAsync(query, 1, CancellationToken.None).Result;
            }
            
            var subtitles = new List<RemoteSubtitleInfo>();
            foreach (var x in data.aaData)
            {
                var sub = new RemoteSubtitleInfo()
                {
                    Name = "",
                    Language = "ESP",
                    Id = x.id.ToString(),
                    CommunityRating = float.Parse(x.promedio.Trim()),
                    DownloadCount = x.descargas,
                    Author = x.nick,
                    ProviderName = Name,
                    Format = "srt"
                };
                if (_configuration?.ShowTitleInResult == true || _configuration?.ShowUploaderInResult == true || _configuration?.ShowDescriptionInResult == true)
                {
                    if (_configuration.ShowTitleInResult)
                        sub.Name = x.titulo;

                    if (_configuration.ShowDescriptionInResult)
                        sub.Name += (_configuration.ShowDescriptionInResult ? " | " : "") + $"Desc: {x.descargas}";
                    
                    if (_configuration.ShowUploaderInResult)
                        sub.Name += (_configuration.ShowTitleInResult ? " | " : "") + $"Uploader: {x.nick}";
                    
                    sub.Comment = x.descripcion;
                }
                else
                {
                    sub.Name = x.descripcion;
                }

                subtitles.Add(sub);
            }

            return subtitles;
        }

        private SubtitleResponse DownloadSubtitle(string id)
        {
            Stream fileStream = null;

            var iteration = 0;
            var maxIterations = 10;
            do
            {
                try
                {
                    var getSubtitleUrl = $"https://subdivx.com/descargar.php?f=1&id={id}";
                    _logger.Debug($"Download subtitle, {getSubtitleUrl}");
                    fileStream = GetFileStream(getSubtitleUrl);
                }
                catch (Exception ex)
                {
                    _logger.Debug($"Error downloading subtitle, ex: {ex.Message}");
                    iteration++;
                }
            } while (fileStream == null && iteration < maxIterations);

            return new SubtitleResponse()
            {
                Format = "srt",
                IsForced = false,
                Language = "ES",
                Stream = fileStream
            };
        }
        
        private Stream GetFileStream(string urlAddress)
        {
            _logger.Debug($"GetFileStream Url: {urlAddress}");

            // 1) Use FlareSolverr to obtain solved cookies and UA for the final URL
            FlareSolverrSolution solved;
            using (var client = new HttpClient())
            {
                var fs = new FlareSolverrClientNetStd(_configuration.FlareSolverrUrl, "subdivx", _jsonSerializer, client);
                // Ask FlareSolverr to GET the same URL; we only need cookies and UA
                solved = fs.RequestGetAsync(urlAddress, new { Accept = "*/*" }, 60000, CancellationToken.None).Result;
            }

            // 2) Reuse cookies and UA to download the binary directly
            var cookieContainer = new CookieContainer();
            if (solved?.cookies != null)
            {
                foreach (var ck in solved.cookies)
                {
                    if (string.IsNullOrEmpty(ck?.name)) continue;
                    try
                    {
                        var domain = (ck.domain ?? "www.subdivx.com").TrimStart('.');
                        var path = string.IsNullOrEmpty(ck.path) ? "/" : ck.path;
                        var cookie = new Cookie(ck.name, ck.value ?? string.Empty, path, domain)
                        {
                            HttpOnly = ck.httpOnly,
                            Secure = ck.secure
                        };
                        cookieContainer.Add(new Uri("https://" + domain), cookie);
                    }
                    catch
                    {
                        // Ignore malformed cookies
                    }
                }
            }

            using (var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            })
            using (var http = new HttpClient(handler))
            {
                var ua = string.IsNullOrWhiteSpace(solved?.userAgent) ? UserAgent : solved.userAgent;
                http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", ua);
                http.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
                http.Timeout = TimeSpan.FromSeconds(90);

                using (var resp = http.GetAsync(urlAddress, HttpCompletionOption.ResponseHeadersRead).Result)
                {
                    resp.EnsureSuccessStatusCode();
                    using (var fileStreamZip = resp.Content.ReadAsStreamAsync().Result)
                    {
                        return Unzip(fileStreamZip);
                    }
                }
            }
        }

        private Stream Unzip(Stream zippedStream)
        {
            var ms = new MemoryStream();
            using (var reader = ReaderFactory.Open(zippedStream))
            {
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory && !reader.Entry.Key.StartsWith("__MACOSX"))
                    {
                        using (var entryStream = reader.OpenEntryStream())
                        {
                            entryStream.CopyTo(ms);
                            ms.Position = 0;
                        }

                        break;
                    }
                }
            }

            return ms;
        }
    }
}
