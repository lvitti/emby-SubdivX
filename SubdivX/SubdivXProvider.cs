using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Model.Serialization;
using SharpCompress.Readers;
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

        public string Name => "SubdivX";

        public IEnumerable<VideoContentType> SupportedMediaTypes => new List<VideoContentType> { VideoContentType.Episode, VideoContentType.Movie };

        public int Order => 1;

        public SubdivXProvider(ILogger logger
            , IJsonSerializer jsonSerializer)
        {
            _logger = logger;
            _jsonSerializer = jsonSerializer;
        }

        public async Task<IEnumerable<RemoteSubtitleInfo>> Search(SubtitleSearchRequest request, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                var json = _jsonSerializer.SerializeToString(request);
                _logger.Debug($"SubdivX Search: {json}");
            });

            if (!string.Equals(request.TwoLetterISOLanguageName, "ES", StringComparison.OrdinalIgnoreCase))
            {
                return Array.Empty<RemoteSubtitleInfo>();
            }

            if (request.ContentType == VideoContentType.Episode)
            {
                var query = $"{request.SeriesName} S{request.ParentIndexNumber:D2}E{request.IndexNumber:D2}";

                var subtitles = SearchSubtitles(query);
                if (subtitles?.Count > 0)
                    return subtitles;
            }
            else
            {
                var query = $"{request.Name} {request.ProductionYear}";

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
            var html = GetHtml($"http://www.subdivx.com/index.php?accion=5&q={query}&pg={page}");
            if (string.IsNullOrWhiteSpace(html))
                return null;

            string reListSub = "<a\\s+class=\"titulo_menu_izq\"\\s+href=\"http://www.subdivx.com/(?<id>[a-zA-Z\\w -]*).html\">";
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
                    ThreeLetterISOLanguageName = "ESP",
                    Id = x.Groups["id"].Value,
                    Name = x.Groups["desc"].Value,
                    CommunityRating = float.Parse(x.Groups["calif"].Value.Trim()),
                    DownloadCount = int.Parse(x.Groups["download"].Value.Trim().Replace(",", "").Replace(".", "")),
                    Author = x.Groups["uploader"].Value,
                    ProviderName = Name,
                    Format = "srt"
                };
                subtitles.Add(sub);
            }

            return subtitles;
        }

        private SubtitleResponse DownloadSubtitle(string id)
        {
            var html = GetHtml($"http://www.subdivx.com/{id}.html");
            if (string.IsNullOrWhiteSpace(html))
                return null;

            string reDownloadPage = "<a class=\"link1\" href=\".*=(?<id>.*?)&u=(?<u>.*)\">Bajar subtítulo ahora</a>";
            Regex re2 = new Regex(reDownloadPage);

            var mat2 = re2.Match(html);
            if (!mat2.Success)
                return null;

            var fileId = mat2.Groups["id"].Value;
            var u = mat2.Groups["u"].Value;

            var getSubtitleUrl = $"http://www.subdivx.com/bajar.php?id={fileId}&u={u}";

            var fileStream = GetFileStream(getSubtitleUrl);

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
                    if (!reader.Entry.IsDirectory)
                    {
                        using (var entryStream = reader.OpenEntryStream())
                        {
                            entryStream.CopyTo(ms);
                            ms.Position = 0;
                        }
                    }
                }
            }

            return ms;
        }
    }
}
