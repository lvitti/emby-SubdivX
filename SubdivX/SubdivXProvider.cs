﻿using MediaBrowser.Controller.Library;
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

        private PluginConfiguration _configuration => !string.IsNullOrWhiteSpace(Plugin.Instance.ConfigurationFileName)
            ? Plugin.Instance.Configuration
            : null;

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
            const string url = "https://www.subdivx.com/inc/ajax.php";
            var data = GetJson<SearchResponse>(url, "POST", new Dictionary<string, string>
            {
                { "tabla", "resultados" },
                { "buscar", query },
            });

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
                if (_configuration?.ShowTitleInResult == true || _configuration?.ShowUploaderInResult == true)
                {
                    if (_configuration.ShowTitleInResult)
                        sub.Name = x.titulo;

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
                    var getSubtitleUrl = iteration == 0
                        ? $"https://www.subdivx.com/sub/{id}.rar"
                        : $"https://www.subdivx.com/sub{iteration}/{id}.rar";
                    
                    fileStream = GetFileStream(getSubtitleUrl);
                }
                catch (Exception ex)
                {
                    try
                    {
                        var getSubtitleUrl = iteration == 0
                            ? $"https://www.subdivx.com/sub/{id}.zip"
                            : $"https://www.subdivx.com/sub{iteration}/{id}.zip";
                        fileStream = GetFileStream(getSubtitleUrl);
                    }
                    catch (Exception ex2)
                    {
                        _logger.Debug($"Error al descargar subtitulo, ex: {ex.Message}");
                        iteration++;
                    }
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

        private T GetJson<T>(string urlAddress, string method = "POST", Dictionary<string, string> parameters = null)
        {
            _logger.Debug($"GetJson Url: {urlAddress}");

            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(parameters);

                client.DefaultRequestHeaders.Host = "www.subdivx.com";
                client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
                var response = client.PostAsync(urlAddress, content).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();

                var jsonResponseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                return _jsonSerializer.DeserializeFromString<T>(jsonResponseString);
            }
        }

        private Stream GetFileStream(string urlAddress)
        {
            _logger.Debug($"GetFileStream Url: {urlAddress}");

            Stream fileStream = null;
            var request = (HttpWebRequest)WebRequest.Create(urlAddress);
            request.Headers["Host"] = "www.subdivx.com";
            request.Headers["User-Agent"] = UserAgent;

            using (var response = (HttpWebResponse)request.GetResponse())
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