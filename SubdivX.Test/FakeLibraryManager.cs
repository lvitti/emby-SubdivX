using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emby.Naming.Common;
using MediaBrowser.Controller.Data;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Querying;


namespace SubdivX.Test;

public sealed class FakeLibraryManager : ILibraryManager
{
    private readonly Dictionary<Guid, BaseItem> _library = new();
    public void AddToLibrary(BaseItem item)
    {
        _library[item.Id] = item;
    }

    public BaseItem ResolvePath(FileSystemMetadata fileInfo, Folder parent = null)
    {
        throw new NotImplementedException();
    }

    public List<BaseItem> ResolvePaths(FileSystemMetadata[] files, IDirectoryService directoryService, Folder parent,
        LibraryOptions libraryOptions)
    {
        throw new NotImplementedException();
    }

    public BaseItem FindByPath(string path, bool? isFolder)
    {
        return _library.Values.FirstOrDefault(i => i.Path == path);
    }
    
    public BaseItem GetItemById(Guid id)
    {
        return _library.GetValueOrDefault(id);
    }
    
    public BaseItem GetItemById(long id)
    {
        return _library.Values.FirstOrDefault(i => i.InternalId == id);
    }

    public long FindIdByPath(string path, bool? isFolder)
    {
        throw new NotImplementedException();
    }

    public List<(ItemLinkType, string, long)> GetItemLinks(long itemId, List<ItemLinkType> types)
    {
        throw new NotImplementedException();
    }

    public Task ValidatePeople(CancellationToken cancellationToken, IProgress<double> progress)
    {
        throw new NotImplementedException();
    }

    public Task ValidateMediaLibrary(IProgress<double> progress, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void QueueLibraryScan()
    {
        throw new NotImplementedException();
    }

    public void UpdateImages(BaseItem item)
    {
        throw new NotImplementedException();
    }

    public ItemImageInfo GetImageInfo(long itemId, ImageType imageType, int index)
    {
        throw new NotImplementedException();
    }

    public Tuple<Type, ItemImageInfo> GetTypeAndImageInfo(long itemId, ImageType imageType, int index)
    {
        throw new NotImplementedException();
    }

    public List<VirtualFolderInfo> GetVirtualFolders()
    {
        throw new NotImplementedException();
    }

    public List<VirtualFolderInfo> GetVirtualFolders(User user, bool includeRefreshState)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Video>> GetIntros(BaseItem item, User user)
    {
        throw new NotImplementedException();
    }

    public List<string> GetAllIntroFiles()
    {
        throw new NotImplementedException();
    }

    public void AddParts(IEnumerable<IItemResolver> resolvers, IEnumerable<IIntroProvider> introProviders, IEnumerable<ILibraryPostScanTask> postscanTasks,
        IEnumerable<ILazyImageProvider> lazyImageProviders)
    {
        throw new NotImplementedException();
    }

    public void CreateItem(BaseItem item, BaseItem parent)
    {
        throw new NotImplementedException();
    }

    public Folder GetUserRootFolder()
    {
        throw new NotImplementedException();
    }

    public long[] FilterItemsToIdsForUser(BaseItem[] items, User user)
    {
        throw new NotImplementedException();
    }

    public void CreateItem(BaseItem item, BaseItem parent, BaseItem[] collectionFolders)
    {
        throw new NotImplementedException();
    }

    public void CreateItems(List<BaseItem> items, BaseItem parent, MetadataRefreshOptions metadataRefreshOptions,
        BaseItem[] collectionFolders, bool triggerItemAdded, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void TriggerItemAdded(BaseItem item, BaseItem parent, BaseItem[] collectionFolders)
    {
        throw new NotImplementedException();
    }

    public void UpdateItems(List<BaseItem> items, BaseItem parent, ItemUpdateType updateReason,
        MetadataRefreshOptions metadataRefreshOptions, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void UpdateItem(BaseItem item, BaseItem parent, ItemUpdateType updateReason,
        MetadataRefreshOptions metadataRefreshOptions)
    {
        throw new NotImplementedException();
    }

    public void UpdateItem(BaseItem item, BaseItem parent, ItemUpdateType updateReason)
    {
        throw new NotImplementedException();
    }

    public void UpdateItems(List<BaseItem> items, BaseItem parent, ItemUpdateType updateReason, bool setDateLastSaved, bool saveMetadata,
        MetadataRefreshOptions metadataRefreshOptions, CancellationToken cancellationToken)
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

    public Folder GetNamedView(string name, string viewType, bool createIfNotFound)
    {
        throw new NotImplementedException();
    }

    public bool IsVideoFile(ReadOnlySpan<char> path)
    {
        throw new NotImplementedException();
    }

    public bool IsSubtitleFile(string path)
    {
        throw new NotImplementedException();
    }

    public bool IsLyricsFile(FileSystemMetadata file)
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

    public bool IsAudioFile(FileSystemMetadata file, LibraryOptions libraryOptions)
    {
        throw new NotImplementedException();
    }

    public bool IsVideoFile(ReadOnlySpan<char> path, LibraryOptions libraryOptions)
    {
        throw new NotImplementedException();
    }

    public bool IsVideoFile(FileSystemMetadata file, LibraryOptions libraryOptions)
    {
        throw new NotImplementedException();
    }

    public int? GetSeasonNumberFromPath(ReadOnlySpan<char> path)
    {
        throw new NotImplementedException();
    }

    public bool IsTVSpecialsFolder(string path)
    {
        throw new NotImplementedException();
    }

    public bool FillMissingEpisodeNumbersFromPath(Episode episode, bool forceRefresh)
    {
        throw new NotImplementedException();
    }

    public Tuple<int?, int?, int?> ParseSxxExxEpisodeNumberSystem(string value)
    {
        throw new NotImplementedException();
    }

    public ItemLookupInfo ParseName(ReadOnlySpan<char> name)
    {
        throw new NotImplementedException();
    }

    public ItemLookupInfo ParseName(string name)
    {
        throw new NotImplementedException();
    }

    public Guid GetNewItemId(string key, Type type)
    {
        throw new NotImplementedException();
    }

    public Guid GetNewItemIdFromName(string name, Type type)
    {
        throw new NotImplementedException();
    }

    public Folder[] GetCollectionFolders(BaseItem item)
    {
        throw new NotImplementedException();
    }

    public LibraryOptions GetLibraryOptions(BaseItem item, BaseItem[] collectionFolders)
    {
        throw new NotImplementedException();
    }

    public LibraryOptions GetLibraryOptions(BaseItem item)
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

    public void UpdatePeople(BaseItem item, List<PersonInfo> people, bool isNewItem = false)
    {
        throw new NotImplementedException();
    }

    public Guid[] GetItemIds(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public long[] GetInternalItemIds(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public ExtraType[] GetExtraTypes(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public QueryResult<BaseItem> QueryItems(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public string GetPathAfterNetworkSubstitution(ReadOnlySpan<char> path, LibraryOptions libraryOptions)
    {
        throw new NotImplementedException();
    }

    public string SubstitutePath(ReadOnlySpan<char> path, ReadOnlySpan<char> from, ReadOnlySpan<char> to)
    {
        throw new NotImplementedException();
    }

    public Task<ItemImageInfo> ConvertImageToLocal(BaseItem item, ItemImageInfo image, int imageIndex, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public BaseItem[] GetItemList(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public bool IsAlphaNumericallyEquivalent(string str1, string str2)
    {
        throw new NotImplementedException();
    }

    public bool FileNameMatchesMetadataName(string filename, string metadataName)
    {
        throw new NotImplementedException();
    }

    public BaseItem[] GetItemList(InternalItemsQuery query, bool obsoleteParam)
    {
        throw new NotImplementedException();
    }

    public string[] GetPaths(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public (string, long)[] GetNameIdPairs(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public (long, Guid)[] GetIdGuidPairs(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public QueryResult<BaseItem> GetItemsResult(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public bool IgnoreFile(FileSystemMetadata file, BaseItem parent, LibraryOptions libraryOptions)
    {
        throw new NotImplementedException();
    }

    public CollectionFolder AddVirtualFolder(string name, LibraryOptions options, bool refreshLibrary)
    {
        throw new NotImplementedException();
    }

    public CollectionFolder AddVirtualFolder(string name, string sortName, LibraryOptions options, bool refreshLibrary)
    {
        throw new NotImplementedException();
    }

    public void RemoveVirtualFolder(long id, bool refreshLibrary)
    {
        throw new NotImplementedException();
    }

    public void RemoveVirtualFolder(long id)
    {
        throw new NotImplementedException();
    }

    public void AddMediaPaths(CollectionFolder item, MediaPathInfo[] pathInfos, bool refreshLibrary)
    {
        throw new NotImplementedException();
    }

    public void UpdateMediaPath(long virtualFolderId, MediaPathInfo path)
    {
        throw new NotImplementedException();
    }

    public void RemoveMediaPath(long virtualFolderId, string path)
    {
        throw new NotImplementedException();
    }

    public QueryResult<Tuple<BaseItem, ItemCounts>> GetGenres(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public QueryResult<Tuple<BaseItem, ItemCounts>> GetAllGenres(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public QueryResult<Tuple<BaseItem, ItemCounts>> GetMusicGenres(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public QueryResult<Tuple<BaseItem, ItemCounts>> GetGameGenres(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public QueryResult<Tuple<BaseItem, ItemCounts>> GetStudios(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public QueryResult<Tuple<BaseItem, ItemCounts>> GetMusicAlbums(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public QueryResult<Tuple<BaseItem, ItemCounts>> GetArtists(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public QueryResult<Tuple<BaseItem, ItemCounts>> GetArtists(InternalItemsQuery query, ItemLinkType[] artistTypes)
    {
        throw new NotImplementedException();
    }

    public QueryResult<Tuple<BaseItem, ItemCounts>> GetPeople(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public string[] GetItemTypes(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public QueryResult<UserItemShareLevel?> GetShareLevels(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public QueryResult<string> GetAudioCodecs(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public QueryResult<string> GetAudioLayouts(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public QueryResult<string> GetStreamLanguages(InternalItemsQuery query, MediaStreamType streamType)
    {
        throw new NotImplementedException();
    }

    public QueryResult<string> GetVideoCodecs(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public QueryResult<ExtendedVideoTypes> GetExtendedVideoTypes(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public QueryResult<string> GetSubtitleCodecs(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public QueryResult<Tuple<BaseItem, ItemCounts>> GetTags(InternalItemsQuery query)
    {
        throw new NotImplementedException();
    }

    public void AddExternalSubtitleStreams(List<MediaStream> streams, string mediaPath, bool isAudio, FileSystemMetadata[] files)
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

    public Task<ListItem[]> AddListItems(BaseItem list, ListItem[] items, bool skipDuplicates)
    {
        throw new NotImplementedException();
    }

    public void UpdateListItems(BaseItem list, LinkedChild[] items)
    {
        throw new NotImplementedException();
    }

    public Task RemoveListItemsByListItemEntryIds(BaseItem list, long[] listItemIds)
    {
        throw new NotImplementedException();
    }

    public Task MoveListItem(BaseItem list, long entryId, int newIndex)
    {
        throw new NotImplementedException();
    }

    public void AddDatabase(ILibraryDatabase db)
    {
        throw new NotImplementedException();
    }

    public long[] GetIdsForAncestorIdsQuery(BaseItem item, User user)
    {
        throw new NotImplementedException();
    }

    public void MergeItems(BaseItem[] items)
    {
        throw new NotImplementedException();
    }

    public void SplitItems(BaseItem item)
    {
        throw new NotImplementedException();
    }

    public void DeleteItems(long[] ids)
    {
        throw new NotImplementedException();
    }

    public List<string> GetExternalSubtitleFiles(long itemId)
    {
        throw new NotImplementedException();
    }

    public Tuple<Guid, string> GetGuidAndPath(long itemId)
    {
        throw new NotImplementedException();
    }

    public bool IsMultiDiscAlbumFolder(string path)
    {
        throw new NotImplementedException();
    }

    public List<string> GetSubviews(InternalItemsQuery query, string contentType)
    {
        throw new NotImplementedException();
    }

    public List<BaseItem> GetAllArtists(BaseItem item)
    {
        throw new NotImplementedException();
    }

    public ProviderIdDictionary GetProviderIds(long itemId)
    {
        throw new NotImplementedException();
    }

    public LinkedItemInfo[] GetImportedCollections(long itemId)
    {
        throw new NotImplementedException();
    }

    public void SetSortIndexNumbers(List<Tuple<long, int>> values)
    {
        throw new NotImplementedException();
    }

    public NamingOptions GetNamingOptions()
    {
        throw new NotImplementedException();
    }

    public Task<bool> RefreshThumbnailImages(Video item, MediaStream videoStream, LibraryOptions libraryOptions,
        MetadataRefreshOptions metadataRefreshOptions, List<ChapterInfo> chapters, bool extractImages, bool saveChapters,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void SaveUserItemShares(UserItemShare[] shares)
    {
        throw new NotImplementedException();
    }

    public void MakePublic(BaseItem item, User user)
    {
        throw new NotImplementedException();
    }

    public void MakePrivate(BaseItem item, User user)
    {
        throw new NotImplementedException();
    }

    public string[] GetPrefixes(BaseItem[] items)
    {
        throw new NotImplementedException();
    }

    public string GetCachedImage(BaseItem item, string originalImagePath)
    {
        throw new NotImplementedException();
    }

    public long GetSyncTargetId(string reportedDeviceId, bool createIfNotFound)
    {
        throw new NotImplementedException();
    }

    public AggregateFolder RootFolder { get; }
    public bool IsScanRunning { get; }
    public long RootFolderId { get; }
    public event EventHandler<ItemChangeEventArgs> ItemAdded;
    public event EventHandler<ItemChangeEventArgs> ItemAdding;
    public event EventHandler<ItemChangeEventArgs> ItemUpdated;
    public event EventHandler<ItemChangeEventArgs> ItemRemoved;
}
