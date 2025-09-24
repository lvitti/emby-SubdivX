using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Subtitles;
using Moq;
using NUnit.Framework;

namespace SubdivX.Test;

public class SubdivXProviderTests
{
    private SubdivXProvider _provider;
    private FakeLibraryManager _libraryManager;
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
        
        _libraryManager = new FakeLibraryManager();
        BaseItem.LibraryManager = _libraryManager;
        
        _provider = new SubdivXProvider(
            logMgr.GetLogger(nameof(SubdivXProvider)), 
            jsonSerializer, 
            _libraryManager
        );
    }

    [TestCase("The Batman", 4, 6, "901212")]
    [TestCase("Dexter: New Blood", 1, 1, "694326")]
    [TestCase("Resident Alien", 2, 5, "801288")]
    public async Task SearchSerie(string serieName, int seasonNumber, int episodeNumber, string id)
    {
        var serie = new Series
        {
            Id = Guid.NewGuid(),
            InternalId = 1,
            Path = $"/Shows/{serieName}",
            OriginalTitle = serieName,
            Name = serieName
        };
        // serie.SetProviderId(MetadataProvider.Imdb, "12345");
        _libraryManager.AddToLibrary(serie);
        
        var season = new Season()
        {
            Id = Guid.NewGuid(),
            InternalId = 2,
            SeriesId = 1,
            Path = $"/Shows/{serieName}/Season {seasonNumber}",
            IndexNumber = seasonNumber
        };
        _libraryManager.AddToLibrary(season);
        
        var episode = new Episode
        {
            Id = Guid.NewGuid(),
            InternalId = 3,
            AlbumId = 2,
            SeriesId = 1,
            Path = $"/Shows/{serieName}/Season {seasonNumber}/{serieName} S{seasonNumber:00}E{episodeNumber:00}.mkv",
            IndexNumber = episodeNumber,
            OriginalTitle = serieName,
        };
        _libraryManager.AddToLibrary(episode);
        
        var request = new SubtitleSearchRequest()
        {
            MediaPath = episode.Path,
            SeriesName = serieName,
            ParentIndexNumber = seasonNumber,
            IndexNumber = episodeNumber,
            ContentType = VideoContentType.Episode,
            Language = "ES",
        };

        var subtitles = await this._provider.Search(request, CancellationToken.None);

        Assert.IsNotNull(subtitles);
        Assert.IsNotNull(subtitles.FirstOrDefault(p => p.Id == id));
    }

    [TestCase("Bad Boys: Ride or Die", 2024, "752980")]
    public async Task SearchMovie(string movieName, int movieYear, string id)
    {
        var movie = new Movie()
        {
            Id = Guid.NewGuid(),
            Path = $"/Movies/{movieName} ({movieYear}).mkv",
            OriginalTitle = movieName,
            Name = movieName,
            ProductionYear = movieYear,
        };
        _libraryManager.AddToLibrary(movie);
        
        var request = new SubtitleSearchRequest()
        {
            MediaPath = movie.Path,
            Name = movieName,
            ProductionYear = movieYear,
            ContentType = VideoContentType.Movie,
            Language = "ES",
        };

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