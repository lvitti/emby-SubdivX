using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using Moq;
using NUnit.Framework;

namespace SubdivX.Test;

public class SubdivXProviderTests
{
    private SubdivXProvider _provider;
    private Mock<ILogger> _logger;
    private Mock<IJsonSerializer> _jsonSerializer;
    private Mock<ILibraryManager> _libraryManager;
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());

    [SetUp]
    public void Setup()
    {
        _ = new Plugin(null, null);

        _logger = new Mock<ILogger>();
        _jsonSerializer = new Mock<IJsonSerializer>();
        _libraryManager = new Mock<ILibraryManager>();

        _provider = new SubdivXProvider(_logger.Object, _jsonSerializer.Object, _libraryManager.Object);
    }

    [TestCase("The Batman", 4, 6, "X666XNjQ4MDc8X-the-batman-s04e06")]
    [TestCase("Dexter: New Blood", 1, 1, "X666XNjMyNTM4X-dexter-new-blood-s01e01")]
    [TestCase("Resident Alien", 2, 5, "X666XNjM5MzU0X-resident-alien-s02e05")]
    public async Task SearchSerie(string serieName, int season, int episode, string id)
    {
        var request = new SubtitleSearchRequest()
        {
            SeriesName = serieName,
            ParentIndexNumber = season,
            IndexNumber = episode,
            ContentType = VideoContentType.Episode,
            Language = "ES",
        };

        var baseItem = _fixture.Create<Mock<BaseItem>>();
        baseItem.Object.OriginalTitle = serieName;

        _libraryManager
            .Setup(x => x.FindByPath(It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(baseItem.Object);

        var subtitles = await this._provider.Search(request, CancellationToken.None);

        Assert.IsNotNull(subtitles);
        Assert.GreaterOrEqual(subtitles.Count(), 1);

        var subtitle = subtitles.ElementAt(0);
        Assert.AreEqual(id, subtitle.Id);
    }

    [TestCase("X666XNjM5MzU0X-resident-alien-s02e05", 59526)]
    [TestCase("X666XNjMyNTM4X-dexter-new-blood-s01e01", 42670)]
    [TestCase("X666XNjQ4MDc8X-the-batman-s04e06", 14902)]
    public async Task DownloadSubtitle(string id, int length)
    {
        var subtitleResponse = await this._provider.GetSubtitles(id, CancellationToken.None);
        string subtitle = Encoding.UTF8.GetString((subtitleResponse.Stream as MemoryStream).ToArray());

        Assert.AreEqual(length, subtitle.Length);
    }
}
