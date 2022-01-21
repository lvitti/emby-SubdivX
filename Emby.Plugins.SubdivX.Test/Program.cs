using MediaBrowser.Controller.Data;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Controller.Sorting;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using SubdivX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Emby.Plugins.SubdivX.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            new Plugin(null, null);

            var provider = new SubdivXProvider(new Logger(), new JsonSerializer(), new MediaLibrary());

            TestDownload(provider, new SubtitleSearchRequest()
            {
                SeriesName = "Dexter: New Blood",
                ParentIndexNumber = 1,
                IndexNumber = 1,
                ContentType = VideoContentType.Episode,
                TwoLetterISOLanguageName = "ES"
            });

            TestDownload(provider, new SubtitleSearchRequest()
            {
                SeriesName = "The Batman",
                ParentIndexNumber = 4,
                IndexNumber = 6,
                ContentType = VideoContentType.Episode,
                TwoLetterISOLanguageName = "ES"
            });

            TestDownload(provider, new SubtitleSearchRequest()
            {
                Name = "The Avengers",
                ProductionYear = 2012,
                ContentType = VideoContentType.Movie,
                TwoLetterISOLanguageName = "ES",
            });

            Console.WriteLine("Done");
        }

        private static void TestDownload(SubdivXProvider provider, SubtitleSearchRequest request)
        {
            var subtitles = provider.Search(request, CancellationToken.None).GetAwaiter().GetResult();

            var item0 = (subtitles as List<MediaBrowser.Model.Providers.RemoteSubtitleInfo>)[0];

            var subtitle = provider.GetSubtitles(item0.Id, CancellationToken.None).GetAwaiter().GetResult();
        }
    }

    public class Logger : MediaBrowser.Model.Logging.ILogger
    {
        public void Debug(string message, params object[] paramList)
        {
            Console.WriteLine(message);
        }

        public void Debug(ReadOnlyMemory<char> message)
        {
            throw new NotImplementedException();
        }

        public void Error(string message, params object[] paramList)
        {
            throw new NotImplementedException();
        }

        public void Error(ReadOnlyMemory<char> message)
        {
            throw new NotImplementedException();
        }

        public void ErrorException(string message, Exception exception, params object[] paramList)
        {
            throw new NotImplementedException();
        }

        public void Fatal(string message, params object[] paramList)
        {
            throw new NotImplementedException();
        }

        public void FatalException(string message, Exception exception, params object[] paramList)
        {
            throw new NotImplementedException();
        }

        public void Info(string message, params object[] paramList)
        {
            throw new NotImplementedException();
        }

        public void Info(ReadOnlyMemory<char> message)
        {
            throw new NotImplementedException();
        }

        public void Log(LogSeverity severity, string message, params object[] paramList)
        {
            throw new NotImplementedException();
        }

        public void Log(LogSeverity severity, ReadOnlyMemory<char> message)
        {
            throw new NotImplementedException();
        }

        public void LogMultiline(string message, LogSeverity severity, StringBuilder additionalContent)
        {
            throw new NotImplementedException();
        }

        public void Warn(string message, params object[] paramList)
        {
            throw new NotImplementedException();
        }

        public void Warn(ReadOnlyMemory<char> message)
        {
            throw new NotImplementedException();
        }
    }

    public class JsonSerializer : MediaBrowser.Model.Serialization.IJsonSerializer
    {
        public object DeserializeFromBytes(ReadOnlySpan<byte> bytes, Type type)
        {
            throw new NotImplementedException();
        }

        public T DeserializeFromBytes<T>(ReadOnlySpan<byte> bytes)
        {
            throw new NotImplementedException();
        }

        public T DeserializeFromFile<T>(string file) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<object> DeserializeFromFileAsync(Type type, string file)
        {
            throw new NotImplementedException();
        }

        public Task<T> DeserializeFromFileAsync<T>(string file) where T : class
        {
            throw new NotImplementedException();
        }

        public T DeserializeFromSpan<T>(ReadOnlySpan<char> text)
        {
            throw new NotImplementedException();
        }

        public object DeserializeFromSpan(ReadOnlySpan<char> json, Type type)
        {
            throw new NotImplementedException();
        }

        public T DeserializeFromStream<T>(Stream stream)
        {
            throw new NotImplementedException();
        }

        public object DeserializeFromStream(Stream stream, Type type)
        {
            throw new NotImplementedException();
        }

        public Task<T> DeserializeFromStreamAsync<T>(Stream stream)
        {
            throw new NotImplementedException();
        }

        public Task<object> DeserializeFromStreamAsync(Stream stream, Type type)
        {
            throw new NotImplementedException();
        }

        public T DeserializeFromString<T>(string text)
        {
            throw new NotImplementedException();
        }

        public object DeserializeFromString(string json, Type type)
        {
            throw new NotImplementedException();
        }

        public void SerializeToFile(object obj, string file)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<char> SerializeToSpan(object obj)
        {
            throw new NotImplementedException();
        }

        public void SerializeToStream(object obj, Stream stream)
        {
            throw new NotImplementedException();
        }

        public string SerializeToString(object obj)
        {
            return String.Empty;
        }
    }

    public class MediaLibrary : MediaBrowser.Controller.Library.ILibraryManager
    {
        public AggregateFolder RootFolder => throw new NotImplementedException();

        public bool IsScanRunning => throw new NotImplementedException();

        public long RootFolderId => throw new NotImplementedException();

        public event EventHandler<ItemChangeEventArgs> ItemAdded;
        public event EventHandler<ItemChangeEventArgs> ItemUpdated;
        public event EventHandler<ItemChangeEventArgs> ItemRemoved;

        public void AddDatabase(ILibraryDatabase db)
        {
            throw new NotImplementedException();
        }

        public void AddExternalSubtitleStreams(List<MediaStream> streams, ReadOnlySpan<char> videoPath, string[] files)
        {
            throw new NotImplementedException();
        }

        public void AddListItems(BaseItem list, ListItem[] items, bool uniqueByItemId)
        {
            throw new NotImplementedException();
        }

        public void AddMediaPath(long virtualFolderId, MediaPathInfo path)
        {
            throw new NotImplementedException();
        }

        public void AddParts(IEnumerable<IItemResolver> resolvers, IEnumerable<IIntroProvider> introProviders, IEnumerable<IBaseItemComparer> itemComparers, IEnumerable<ILibraryPostScanTask> postscanTasks, IEnumerable<ILazyImageProvider> lazyImageProviders)
        {
            throw new NotImplementedException();
        }

        public Task AddVirtualFolder(string name, LibraryOptions options, bool refreshLibrary)
        {
            throw new NotImplementedException();
        }

        public Task<ItemImageInfo> ConvertImageToLocal(BaseItem item, ItemImageInfo image, int imageIndex, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void CreateItem(BaseItem item, BaseItem parent)
        {
            throw new NotImplementedException();
        }

        public void CreateItems(List<BaseItem> items, BaseItem parent, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void DeleteItem(BaseItem item, DeleteOptions options)
        {
            throw new NotImplementedException();
        }

        public void DeleteItem(BaseItem item, DeleteOptions options, bool notifyParentItem)
        {
            throw new NotImplementedException();
        }

        public void DeleteItem(BaseItem item, DeleteOptions options, BaseItem parent, bool notifyParentItem)
        {
            throw new NotImplementedException();
        }

        public void DeleteItems(long[] ids)
        {
            throw new NotImplementedException();
        }

        public bool FillMissingEpisodeNumbersFromPath(Episode episode, bool forceRefresh)
        {
            throw new NotImplementedException();
        }

        public BaseItem FindByPath(string path, bool? isFolder)
        {
            return new MockBaseItem();
        }

        public IEnumerable<BaseItem> FindExtras(BaseItem owner, List<FileSystemMetadata> fileSystemChildren, IDirectoryService directoryService)
        {
            throw new NotImplementedException();
        }

        public long FindIdByPath(string path, bool? isFolder)
        {
            throw new NotImplementedException();
        }

        public string[] GetAlbumArtistPrefixes(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public QueryResult<Tuple<BaseItem, ItemCounts>> GetAlbumArtists(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public string[] GetAllArtistPrefixes(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public List<BaseItem> GetAllArtists(BaseItem item)
        {
            throw new NotImplementedException();
        }

        public QueryResult<Tuple<BaseItem, ItemCounts>> GetAllArtists(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public List<string> GetAllIntroFiles()
        {
            throw new NotImplementedException();
        }

        public string[] GetArtistPrefixes(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public QueryResult<Tuple<BaseItem, ItemCounts>> GetArtists(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public string[] GetAudioCodecs(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public Folder[] GetCollectionFolders(BaseItem item)
        {
            throw new NotImplementedException();
        }

        public string[] GetContainers(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public long[] GetExcludedSubFolders(User user, CollectionFolder folder)
        {
            throw new NotImplementedException();
        }

        public List<string> GetExternalSubtitleFiles(long itemId)
        {
            throw new NotImplementedException();
        }

        public QueryResult<Tuple<BaseItem, ItemCounts>> GetGameGenres(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public QueryResult<Tuple<BaseItem, ItemCounts>> GetGenres(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(long id)
        {
            throw new NotImplementedException();
        }

        public Tuple<Guid, string> GetGuidAndPath(long itemId)
        {
            throw new NotImplementedException();
        }

        public (long, Guid)[] GetIdGuidPairs(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public ItemImageInfo GetImageInfo(long itemId, ImageType imageType, int index)
        {
            throw new NotImplementedException();
        }

        public long GetInternalId(Guid id)
        {
            throw new NotImplementedException();
        }

        public long GetInternalId(string id)
        {
            throw new NotImplementedException();
        }

        public long GetInternalId(ReadOnlySpan<char> id)
        {
            throw new NotImplementedException();
        }

        public long[] GetInternalItemIds(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Video>> GetIntros(BaseItem item, User user)
        {
            throw new NotImplementedException();
        }

        public BaseItem GetItemById(Guid id)
        {
            throw new NotImplementedException();
        }

        public BaseItem GetItemById(long id)
        {
            throw new NotImplementedException();
        }

        public Guid[] GetItemIds(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public List<(ItemLinkType, string, long)> GetItemLinks(long itemId, List<ItemLinkType> types)
        {
            throw new NotImplementedException();
        }

        public BaseItem[] GetItemList(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public BaseItem[] GetItemList(InternalItemsQuery query, bool allowExternalContent)
        {
            throw new NotImplementedException();
        }

        public BaseItem[] GetItemList(InternalItemsQuery query, BaseItem[] parents)
        {
            throw new NotImplementedException();
        }

        public List<PersonInfo> GetItemPeople(BaseItem item)
        {
            throw new NotImplementedException();
        }

        public List<PersonInfo> GetItemPeople(InternalPeopleQuery query)
        {
            throw new NotImplementedException();
        }

        public QueryResult<BaseItem> GetItemsResult(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public LibraryOptions GetLibraryOptions(BaseItem item)
        {
            throw new NotImplementedException();
        }

        public QueryResult<Tuple<BaseItem, ItemCounts>> GetMusicGenres(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public UserView GetNamedView(ReadOnlySpan<char> name, ReadOnlySpan<char> viewType)
        {
            throw new NotImplementedException();
        }

        public (string, long)[] GetNameIdPairs(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public Guid GetNewItemId(string key, Type type)
        {
            throw new NotImplementedException();
        }

        public string[] GetOfficialRatings(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public string GetPathAfterNetworkSubstitution(ReadOnlySpan<char> path, LibraryOptions libraryOptions)
        {
            throw new NotImplementedException();
        }

        public string[] GetPaths(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public QueryResult<Tuple<BaseItem, ItemCounts>> GetPeople(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public string[] GetPrefixes(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public int? GetSeasonNumberFromPath(ReadOnlySpan<char> path)
        {
            throw new NotImplementedException();
        }

        public QueryResult<Tuple<BaseItem, ItemCounts>> GetStudios(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public string[] GetSubtitleCodecs(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public QueryResult<Tuple<BaseItem, ItemCounts>> GetTags(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public long[] GetTopParentIdsForQuery(BaseItem item, User user)
        {
            throw new NotImplementedException();
        }

        public Folder GetUserRootFolder()
        {
            throw new NotImplementedException();
        }

        public string[] GetVideoCodecs(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public List<VirtualFolderInfo> GetVirtualFolders()
        {
            throw new NotImplementedException();
        }

        public List<VirtualFolderInfo> GetVirtualFolders(bool includeRefreshState)
        {
            throw new NotImplementedException();
        }

        public int[] GetYears(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public bool HasPeople(BaseItem item)
        {
            throw new NotImplementedException();
        }

        public bool IgnoreFile(FileSystemMetadata file, BaseItem parent, LibraryOptions libraryOptions)
        {
            throw new NotImplementedException();
        }

        public bool IsAudioFile(ReadOnlySpan<char> path)
        {
            throw new NotImplementedException();
        }

        public bool IsAudioFile(ReadOnlySpan<char> path, LibraryOptions libraryOptions)
        {
            throw new NotImplementedException();
        }

        public bool IsSubtitleFile(ReadOnlySpan<char> path)
        {
            throw new NotImplementedException();
        }

        public bool IsVideoFile(ReadOnlySpan<char> path)
        {
            throw new NotImplementedException();
        }

        public bool IsVideoFile(ReadOnlySpan<char> path, LibraryOptions libraryOptions)
        {
            throw new NotImplementedException();
        }

        public void MergeItems(BaseItem[] items)
        {
            throw new NotImplementedException();
        }

        public void MoveListItem(BaseItem list, long entryId, int newIndex)
        {
            throw new NotImplementedException();
        }

        public List<FileSystemMetadata> NormalizeRootPathList(FileSystemMetadata[] paths)
        {
            throw new NotImplementedException();
        }

        public ItemLookupInfo ParseName(ReadOnlySpan<char> name)
        {
            throw new NotImplementedException();
        }

        public QueryResult<BaseItem> QueryItems(InternalItemsQuery query)
        {
            throw new NotImplementedException();
        }

        public void QueueLibraryScan()
        {
            throw new NotImplementedException();
        }

        public void RemoveListItemsByItemIds(BaseItem list, long[] listItemIds)
        {
            throw new NotImplementedException();
        }

        public void RemoveListItemsByListItemEntryIds(BaseItem list, long[] listItemIds)
        {
            throw new NotImplementedException();
        }

        public void RemoveMediaPath(long virtualFolderId, string path)
        {
            throw new NotImplementedException();
        }

        public Task RemoveVirtualFolder(long id, bool refreshLibrary)
        {
            throw new NotImplementedException();
        }

        public BaseItem ResolvePath(FileSystemMetadata fileInfo, Folder parent = null)
        {
            throw new NotImplementedException();
        }

        public List<BaseItem> ResolvePaths(FileSystemMetadata[] files, IDirectoryService directoryService, Folder parent, LibraryOptions libraryOptions)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<BaseItem> Sort(IEnumerable<BaseItem> items, User user, IEnumerable<string> sortBy, SortOrder sortOrder)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<BaseItem> Sort(IEnumerable<BaseItem> items, User user, IEnumerable<(string, SortOrder)> orderBy)
        {
            throw new NotImplementedException();
        }

        public void SplitItems(BaseItem item)
        {
            throw new NotImplementedException();
        }

        public string SubstitutePath(ReadOnlySpan<char> path, ReadOnlySpan<char> from, ReadOnlySpan<char> to)
        {
            throw new NotImplementedException();
        }

        public void UpdateImages(BaseItem item)
        {
            throw new NotImplementedException();
        }

        public void UpdateItem(BaseItem item, BaseItem parent, ItemUpdateType updateReason)
        {
            throw new NotImplementedException();
        }

        public void UpdateItems(List<BaseItem> items, BaseItem parent, ItemUpdateType updateReason, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void UpdateListItems(BaseItem list, LinkedChild[] items)
        {
            throw new NotImplementedException();
        }

        public void UpdateMediaPath(long virtualFolderId, MediaPathInfo path)
        {
            throw new NotImplementedException();
        }

        public void UpdatePeople(BaseItem item, List<PersonInfo> people)
        {
            throw new NotImplementedException();
        }

        public Task ValidateMediaLibrary(IProgress<double> progress, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ValidatePeople(CancellationToken cancellationToken, IProgress<double> progress)
        {
            throw new NotImplementedException();
        }
    }

    public class MockBaseItem : BaseItem
    {
    }
}
