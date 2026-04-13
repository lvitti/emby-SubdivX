using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using Moq;

namespace SubdivX.Test;

/// <summary>
/// Helper class to create a mocked ILibraryManager for tests
/// </summary>
public class LibraryManagerHelper
{
    private readonly Dictionary<string, BaseItem> _library = new();
    private readonly Dictionary<long, BaseItem> _libraryById = new();
    private readonly Dictionary<Guid, BaseItem> _libraryByGuid = new();
    private readonly Mock<ILibraryManager> _mock;

    public LibraryManagerHelper()
    {
        _mock = new Mock<ILibraryManager>(MockBehavior.Loose);
        
        // Setup FindByPath to return items from our in-memory library
        _mock.Setup(m => m.FindByPath(It.IsAny<string>(), It.IsAny<bool?>()))
            .Returns<string, bool?>((path, isFolder) => 
            {
                return _library.TryGetValue(path, out var item) ? item : null;
            });
        
        // Setup GetItemById for long IDs
        _mock.Setup(m => m.GetItemById(It.IsAny<long>()))
            .Returns<long>(id => 
            {
                return _libraryById.TryGetValue(id, out var item) ? item : null;
            });
        
        // Setup GetItemById for Guid IDs
        _mock.Setup(m => m.GetItemById(It.IsAny<Guid>()))
            .Returns<Guid>(id => 
            {
                return _libraryByGuid.TryGetValue(id, out var item) ? item : null;
            });
    }

    /// <summary>
    /// Add an item to the library for testing
    /// </summary>
    public void AddToLibrary(BaseItem item)
    {
        if (item == null) return;
        
        if (item.Path != null)
        {
            _library[item.Path] = item;
        }
        
        if (item.InternalId > 0)
        {
            _libraryById[item.InternalId] = item;
        }
        
        if (item.Id != Guid.Empty)
        {
            _libraryByGuid[item.Id] = item;
        }
    }

    /// <summary>
    /// Get the mocked ILibraryManager instance
    /// </summary>
    public ILibraryManager Object => _mock.Object;
}
