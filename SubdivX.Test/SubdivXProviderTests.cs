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

        var fakeJsonSerializer = new JsonSerializer();

        _jsonSerializer.Setup(repo => repo.DeserializeFromString<SearchResponse>(It.IsAny<string>()))
            .Returns((string text) => fakeJsonSerializer.DeserializeFromString<SearchResponse>(text));

        _provider = new SubdivXProvider(_logger.Object, _jsonSerializer.Object, _libraryManager.Object);
    }

    [TestCase("The Batman", 4, 6, "64807")]
    [TestCase("Dexter: New Blood", 1, 1, "632538")]
    [TestCase("Resident Alien", 2, 5, "639354")]
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

        var subtitle = subtitles.OrderBy(p => long.Parse(p.Id)).ToList().ElementAt(0);
        Assert.AreEqual(id, subtitle.Id);
    }

    [TestCase("Bad Boys: Ride or Die", 2024, "681473")]
    public async Task SearchMovie(string movieName, int movieYear, string id)
    {
        var request = new SubtitleSearchRequest()
        {
            Name = movieName,
            ProductionYear = movieYear,
            ContentType = VideoContentType.Movie,
            Language = "ES",
        };

        var baseItem = _fixture.Create<Mock<BaseItem>>();
        baseItem.Object.OriginalTitle = movieName;

        _libraryManager
            .Setup(x => x.FindByPath(It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(baseItem.Object);

        var subtitles = await this._provider.Search(request, CancellationToken.None);

        Assert.IsNotNull(subtitles);
        Assert.GreaterOrEqual(subtitles.Count(), 1);

        var subtitle = subtitles.OrderBy(p => long.Parse(p.Id)).ToList().ElementAt(0);
        Assert.AreEqual(id, subtitle.Id);
    }

    [TestCase("Resident Alien S02E05", "639354", 59526)]
    [TestCase("Dexter: New Blood S01E01", "632538", 42670)]
    [TestCase("The Batman S04E06", "64807", 14902)]
    [TestCase("Bad Boys: Ride or Die 2024", "681473", 121267)]
    public async Task DownloadSubtitle(string testName, string id, int length)
    {
        var subtitleResponse = await this._provider.GetSubtitles(id, CancellationToken.None);
        string subtitle = Encoding.UTF8.GetString((subtitleResponse.Stream as MemoryStream).ToArray());

        Assert.AreEqual(length, subtitle.Length);
    }
}