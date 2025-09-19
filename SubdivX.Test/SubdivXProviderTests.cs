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
using Moq;
using NUnit.Framework;

namespace SubdivX.Test;

public class SubdivXProviderTests
{
    private SubdivXProvider _provider;
    private Mock<ILibraryManager> _libraryManager;
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());

    [SetUp]
    public void Setup()
    {
        var (appHost, logMgr, jsonSerializer, _, _) = TestHostFactory.BuildAppHost();
        var testConfig = ConfigurationHelper.LoadConfig(jsonSerializer);
        
        // Partial mock: llama al ctor base (setea Plugin.Instance)
        var pluginMock = new Moq.Mock<Plugin>(appHost, logMgr) { CallBase = true };
        pluginMock
            .Setup(p => p.GetConfiguration())
            .Returns(testConfig);

        _ = pluginMock.Object;
        
        _libraryManager = new Mock<ILibraryManager>();
        _provider = new SubdivXProvider(
            logMgr.GetLogger(nameof(SubdivXProvider)), 
            jsonSerializer, 
            _libraryManager.Object
        );
    }

    [TestCase("The Batman", 4, 6, "901212")]
    [TestCase("Dexter: New Blood", 1, 1, "694326")]
    [TestCase("Resident Alien", 2, 5, "801288")]
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
        Assert.IsNotNull(subtitles.FirstOrDefault(p => p.Id == id));
    }

    [TestCase("Bad Boys: Ride or Die", 2024, "752980")]
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

        Assert.IsNotNull(subtitles.FirstOrDefault(p => p.Id == id));
    }

    [TestCase("Resident Alien S02E05", "801288", 59526)]
    [TestCase("Dexter: New Blood S01E01", "694326", 42670)]
    [TestCase("The Batman S04E06", "901212", 14902)]
    [TestCase("Bad Boys: Ride or Die 2024", "752980", 121267)]
    public async Task DownloadSubtitle(string testName, string id, int length)
    {
        var subtitleResponse = await this._provider.GetSubtitles(id, CancellationToken.None);
        string subtitle = Encoding.UTF8.GetString((subtitleResponse.Stream as MemoryStream).ToArray());

        Assert.AreEqual(length, subtitle.Length);
    }
}