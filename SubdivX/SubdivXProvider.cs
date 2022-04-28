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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SubdivX
{
    public class SubdivXProvider : ISubtitleProvider, IHasOrder
    {
        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILibraryManager _libraryManager;
        private PluginConfiguration _configuration => !string.IsNullOrWhiteSpace(Plugin.Instance.ConfigurationFileName) ? Plugin.Instance.Configuration : null;

        public string Name => "SubdivX";

        public IEnumerable<VideoContentType> SupportedMediaTypes => new List<VideoContentType> { VideoContentType.Episode, VideoContentType.Movie };

        public int Order => 1;

        public SubdivXProvider(ILogger logger
            , IJsonSerializer jsonSerializer
            , ILibraryManager libraryManager)
        {
            _logger = logger;
            _jsonSerializer = jsonSerializer;
            _libraryManager = libraryManager;
        }

        public async Task<IEnumerable<RemoteSubtitleInfo>> Search(SubtitleSearchRequest request, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                var json = _jsonSerializer.SerializeToString(request);
                _logger.Debug($"SubdivX Search | Request-> {json}");
            });

            _logger.Debug($"SubdivX Search | UseOriginalTitle-> {_configuration?.UseOriginalTitle}");

            if (!string.Equals(request.TwoLetterISOLanguageName, "ES", StringComparison.OrdinalIgnoreCase))
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
            await Task.Run(() =>
            {
                _logger.Debug($"SubdivX GetSubtitles id: {id}");
            });

            var subtitle = DownloadSubtitle(id);
            if (subtitle != null)
                return subtitle;

            return new SubtitleResponse();
        }

        private List<RemoteSubtitleInfo> SearchSubtitles(string query)
        {
            var page = 1;
            var subtitles = new List<RemoteSubtitleInfo>();
            do
            {
                var list = SearchSubtitles(query, page);
                if (list == null)
                    break;

                subtitles.AddRange(list);
                page++;
            } while (true);

            return subtitles;
        }

        private List<RemoteSubtitleInfo> SearchSubtitles(string query, int page)
        {
            var url = $"https://www.subdivx.com/index.php?buscar2={query}&accion=5&masdesc=&subtitulos=1&realiza_b=1&pg={page}";
            var html = GetHtml(url);
            if (string.IsNullOrWhiteSpace(html))
                return null;

            string reListSub = "<a\\s+class=\"titulo_menu_izq\"\\s+href=\"https://www.subdivx.com/(?<id>[a-zA-Z\\w -]*).html\">(?<title>.*)</a></div>";
            reListSub += "+.*<img\\s+src=\"img/calif(?<calif>\\d)\\.gif\"\\s+class=\"detalle_calif\"\\s+name=\"detalle_calif\">+.*";
            reListSub += "\\n<div\\s+id=\"buscador_detalle_sub\">(?<desc>.*?)</div>+.*<b>Downloads:</b>(?<download>.+?)<b>Cds:</b>+.*<b>Subido\\ por:</b>\\s*<a.+?>(?<uploader>.+?)</a>.+?</div></div>";
            Regex re = new Regex(reListSub);

            var mat = re.Matches(html);
            if (mat.Count == 0)
                return null;

            var subtitles = new List<RemoteSubtitleInfo>();
            foreach (Match x in mat)
            {
                var sub = new RemoteSubtitleInfo()
                {
                    Name = "",
                    ThreeLetterISOLanguageName = "ESP",
                    Id = x.Groups["id"].Value,
                    CommunityRating = float.Parse(x.Groups["calif"].Value.Trim()),
                    DownloadCount = int.Parse(x.Groups["download"].Value.Trim().Replace(",", "").Replace(".", "")),
                    Author = x.Groups["uploader"].Value,
                    ProviderName = Name,
                    Format = "srt"
                };
                if (_configuration?.ShowTitleInResult == true || _configuration?.ShowUploaderInResult == true)
                {
                    if (_configuration.ShowTitleInResult)
                        sub.Name = $"{x.Groups["title"].Value.Trim()}";

                    if (_configuration.ShowUploaderInResult)
                        sub.Name += (_configuration.ShowTitleInResult ? " | " : "") + $"Uploader: { x.Groups["uploader"].Value.Trim()}";

                    sub.Comment = x.Groups["desc"].Value;
                }
                else
                {
                    sub.Name = x.Groups["desc"].Value;
                }

                subtitles.Add(sub);
            }

            return subtitles;
        }

        private SubtitleResponse DownloadSubtitle(string id)
        {
            var html = GetHtml($"https://www.subdivx.com/{id}.html");
            if (string.IsNullOrWhiteSpace(html))
                return null;

            string reDownloadPage = "<a class=\"link1\" href=\".*=(?<id>.*?)&u=(?<u>.*)\">Bajar subtítulo ahora</a>";
            Regex re2 = new Regex(reDownloadPage);

            var mat2 = re2.Match(html);
            if (!mat2.Success)
                return null;

            var fileId = mat2.Groups["id"].Value;
            var u = mat2.Groups["u"].Value;

            Stream fileStream;
            try
            {
                var getSubtitleUrl = $"https://www.subdivx.com/sub{u}/{fileId}";
                fileStream = GetFileStream(getSubtitleUrl);
            }
            catch (Exception ex)
            {
                _logger.Debug($"Error al descargar subtitulo, ex: {ex.Message}");

                var getSubtitleUrl = $"https://www.subdivx.com/sub/{fileId}";
                fileStream = GetFileStream(getSubtitleUrl);
            }

            return new SubtitleResponse()
            {
                Format = "srt",
                IsForced = false,
                Language = "ES",
                Stream = fileStream
            };
        }

        private string GetHtml(string urlAddress)
        {
            _logger.Debug($"GetHtml Url: {urlAddress}");

            string data = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            request.Headers["Host"] = "www.subdivx.com";
            request.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                using (Stream receiveStream = response.GetResponseStream())
                {
                    StreamReader readStream = null;

                    if (string.IsNullOrWhiteSpace(response.CharacterSet))
                        readStream = new StreamReader(receiveStream);
                    else
                        readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                    data = readStream.ReadToEnd();

                    response.Close();
                    readStream.Close();
                }
            }
            return data;
        }

        private Stream GetFileStream(string urlAddress)
        {
            _logger.Debug($"GetFileStream Url: {urlAddress}");

            Stream fileStream = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            request.Headers["Host"] = "www.subdivx.com";
            request.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var fileStreamZip = response.GetResponseStream())
                    {
                        fileStream = Unzip(fileStreamZip);
                    }

                    response.Close();
                }
            }

            return fileStream;
        }

        private Stream Unzip(Stream zippedStream)
        {
            MemoryStream ms = new MemoryStream();
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
