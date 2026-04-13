using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using MediaBrowser.Common;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Model.Entities;
using Moq;
using NUnit.Framework;

namespace SubdivX.Test;

public class SubdivXProviderTests
{
    private SubdivXProvider _provider;
    private LibraryManagerHelper _libraryManagerHelper;
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
        
        _libraryManagerHelper = new LibraryManagerHelper();
        BaseItem.LibraryManager = _libraryManagerHelper.Object;

        var applicationHost = new Mock<IApplicationHost>();
        
        _provider = new SubdivXProvider(
            logMgr.GetLogger(nameof(SubdivXProvider)), 
            jsonSerializer, 
            _libraryManagerHelper.Object,
            applicationHost.Object
        );
    }

    [TestCase("The Batman", 4, 6, "56b1a743-de52-46da-91a1-2c91c9b1427a")]
    [TestCase("Dexter: New Blood", 1, 1, "098d1c77-508f-4cfb-913e-d34e541fb65c")]
    [TestCase("Resident Alien", 2, 5, "6b9f7cd4-f60d-4b1d-8f33-1fed1f3f999b")]
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
        serie.SetProviderId(MetadataProviders.Imdb, "ttShowImdbId");
        
        var season = new Season()
        {
            Id = Guid.NewGuid(),
            InternalId = 2,
            SeriesId = 1,
            Path = $"/Shows/{serieName}/Season {seasonNumber}",
            IndexNumber = seasonNumber
        };
        
        // Add Series and Season to library FIRST before creating Episode
        _libraryManagerHelper.AddToLibrary(serie);
        _libraryManagerHelper.AddToLibrary(season);
        
        var episode = new Episode
        {
            Id = Guid.NewGuid(),
            InternalId = 3,
            SeriesId = 1,
            ParentIndexNumber = seasonNumber,
            Path = $"/Shows/{serieName}/Season {seasonNumber}/{serieName} S{seasonNumber:00}E{episodeNumber:00}.mkv",
            IndexNumber = episodeNumber,
            OriginalTitle = serieName,
        };
        episode.SetProviderId(MetadataProviders.Imdb, "ttEpisodeImdbId");
        _libraryManagerHelper.AddToLibrary(episode);
        
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

    [TestCase("Bad Boys: Ride or Die", 2024, "f117211a-63f3-4485-bb1e-d347750891be")]
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
        _libraryManagerHelper.AddToLibrary(movie);
        
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

    [TestCase("Resident Alien S02E05", "6b9f7cd4-f60d-4b1d-8f33-1fed1f3f999b", 59526)]
    [TestCase("Dexter: New Blood S01E01", "06cbdb1f-e034-4537-a007-5cdc30d6c869", 42670)]
    [TestCase("The Batman S04E06", "56b1a743-de52-46da-91a1-2c91c9b1427a", 14902)]
    [TestCase("Bad Boys: Ride or Die 2024", "6373f0a7-5011-4b31-a6f8-59884cc7b4fd", 121267)]
    public async Task DownloadSubtitle(string testName, string id, int length)
    {
        var subtitleResponse = await this._provider.GetSubtitles(id, CancellationToken.None);
        string subtitle = Encoding.UTF8.GetString((subtitleResponse.Stream as MemoryStream).ToArray());

        Assert.AreEqual(length, subtitle.Length);
    }
}