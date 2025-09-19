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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        public class SearchResponse
        {
            public List<ItemResponse> items { get; set; }
        }

        public class ItemResponse
        {
            public int id { get; set; }
            public string video_type{ get; set; }
            public string title{ get; set; }
            public int? season{ get; set; }
            public int? episode{ get; set; }
            public string? imdb_id{ get; set; }
            public string? description{ get; set; }
            public int downloads{ get; set; }
            public string uploader_name{ get; set; }
            public DateTime posted_at{ get; set; }
        }
        
        private List<RemoteSubtitleInfo> SearchSubtitles(string query)
        {
            string url = $"{this._configuration.SubXApiUrl}/subtitles/search";
            var data = GetJson<SearchResponse>(url, "GET",
                new Dictionary<string, string>
                {
                    { "query", query },
                },new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {this._configuration.Token}" },
                    { "accept", "application/json" },
                });
            
            
            var subtitles = new List<RemoteSubtitleInfo>();
            foreach (var x in data.items)
            {
                var sub = new RemoteSubtitleInfo()
                {
                    Name = "",
                    Language = "ESP",
                    Id = x.id.ToString(),
                    DownloadCount = x.downloads,
                    Author = x.uploader_name,
                    ProviderName = Name,
                    Format = "srt"
                };
                if (_configuration?.ShowTitleInResult == true || _configuration?.ShowUploaderInResult == true || _configuration?.ShowDescriptionInResult == true)
                {
                    if (_configuration.ShowTitleInResult)
                        sub.Name = x.title;

                    if (_configuration.ShowDescriptionInResult)
                        sub.Name += (_configuration.ShowDescriptionInResult ? " | " : "") + $"Desc: {x.description}";
                    
                    if (_configuration.ShowUploaderInResult)
                        sub.Name += (_configuration.ShowTitleInResult ? " | " : "") + $"Uploader: {x.uploader_name}";
                    
                    sub.Comment = x.description;
                }
                else
                {
                    sub.Name = x.description;
                }

                subtitles.Add(sub);
            }

            return subtitles;
        }

        private static bool TryParseAuthHeader(string value, out string scheme, out string parameter)
        {
            scheme = null;
            parameter = null;
            if (string.IsNullOrWhiteSpace(value)) return false;

            var idx = value.IndexOf(' ');
            if (idx <= 0 || idx >= value.Length - 1) return false;

            scheme = value.Substring(0, idx);
            parameter = value.Substring(idx + 1).Trim();
            return true;
        }
        
        private T GetJson<T>(
            string urlAddress,
            string method = "POST",
            Dictionary<string, string> parameters = null,
            Dictionary<string, string> headers = null)
        {
            _logger.Debug($"GetJson Url: {urlAddress} Method: {method}");

            var httpMethod = new HttpMethod((method ?? "POST").ToUpperInvariant());

            if ((httpMethod == HttpMethod.Get || httpMethod == HttpMethod.Delete) && parameters != null && parameters.Count > 0)
            {
                var qs = string.Join("&", parameters.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value ?? string.Empty)}"));
                urlAddress += (urlAddress.Contains("?") ? "&" : "?") + qs;
            }

            using var client = new HttpClient();
            using var request = new HttpRequestMessage(httpMethod, urlAddress);

            if ((httpMethod == HttpMethod.Post || httpMethod == HttpMethod.Put || httpMethod.Method == "PATCH") && parameters != null && parameters.Count > 0)
                request.Content = new FormUrlEncodedContent(parameters);

            if (headers != null)
            {
                foreach (var kv in headers)
                {
                    var key = kv.Key;
                    var value = kv.Value ?? string.Empty;

                    if (key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                    {
                        string scheme, parameter;
                        if (TryParseAuthHeader(value, out scheme, out parameter))
                            request.Headers.Authorization = new AuthenticationHeaderValue(scheme, parameter);
                        else
                            request.Headers.TryAddWithoutValidation(key, value);
                    }
                    else if (key.Equals("Accept", StringComparison.OrdinalIgnoreCase))
                    {
                        request.Headers.Accept.Clear();
                        var parts = value.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var v in parts)
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(v.Trim()));
                    }
                    else if (key.Equals("User-Agent", StringComparison.OrdinalIgnoreCase))
                    {
                        request.Headers.UserAgent.Clear();
                        request.Headers.UserAgent.ParseAdd(value);
                    }
                    else if (key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                    {
                        request.Headers.Host = value;
                    }
                    else if (!request.Headers.TryAddWithoutValidation(key, value))
                    {
                        if (request.Content == null)
                            request.Content = new ByteArrayContent(new byte[0]);
                        request.Content.Headers.TryAddWithoutValidation(key, value);
                    }
                }
            }

            var response = client.SendAsync(request).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var jsonResponseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return _jsonSerializer.DeserializeFromString<T>(jsonResponseString);
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
                    var getSubtitleUrl = $"{this._configuration.SubXApiUrl}/subtitles/{id}/download";
                    _logger.Debug($"Download subtitle, {getSubtitleUrl}");
                    fileStream = GetFileStream(getSubtitleUrl, bearerToken: this._configuration.Token);
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
        
        private Stream GetFileStream(string urlAddress, string bearerToken = null, Dictionary<string, string> headers = null)
        {
            _logger.Debug($"GetFileStream Url: {urlAddress}");

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            using (handler)
            using (var http = new HttpClient(handler))
            {
                http.Timeout = TimeSpan.FromSeconds(90);

                using (var req = new HttpRequestMessage(HttpMethod.Get, urlAddress))
                {
                    req.Headers.TryAddWithoutValidation("Accept", "*/*");

                    if (!string.IsNullOrEmpty(bearerToken))
                        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                    if (headers != null)
                    {
                        foreach (var kv in headers)
                        {
                            var key = kv.Key;
                            var value = kv.Value ?? string.Empty;

                            if (key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                            {
                                string scheme, parameter;
                                if (TryParseAuthHeader(value, out scheme, out parameter))
                                    req.Headers.Authorization = new AuthenticationHeaderValue(scheme, parameter);
                                else
                                    req.Headers.TryAddWithoutValidation(key, value);
                            }
                            else if (key.Equals("Accept", StringComparison.OrdinalIgnoreCase))
                            {
                                req.Headers.Accept.Clear();
                                var parts = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                for (int i = 0; i < parts.Length; i++)
                                    req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(parts[i].Trim()));
                            }
                            else if (key.Equals("User-Agent", StringComparison.OrdinalIgnoreCase))
                            {
                                req.Headers.UserAgent.Clear();
                                req.Headers.UserAgent.ParseAdd(value);
                            }
                            else if (key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                                req.Headers.Host = value;
                            else
                                req.Headers.TryAddWithoutValidation(key, value);
                        }
                    }

                    using (var resp = http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead).Result)
                    {
                        resp.EnsureSuccessStatusCode();

                        using (var archiveStream = resp.Content.ReadAsStreamAsync().Result)
                        {
                            return Unzip(archiveStream);
                        }
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
