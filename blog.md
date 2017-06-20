# Simplified File Management for Azure Mobile Apps

We have introduced some enhancements to the .NET file management client that
simplifies what you need to do to take advantage of file management with
offline sync.

## Introducing the "Express" Client API

The "Express" client API essentially provides you with some simple defaults
that mean you don't need to implement any of the usually required interfaces,
such as `IFileSyncHandler` and `IFileDataSource`. It saves files to a location
on the local device's file system and keeps them in sync as changes are
made. You save and access these files using `Stream` objects.

### Setting Up

First up, create yourself a simple Azure Mobile Apps server. If you want to use
the Node.js backend, follow the steps [here](https://github.com/Azure/azure-mobile-apps-node-files),
or for the .NET backend, follow the steps [here](https://azure.microsoft.com/en-us/blog/file-management-with-azure-mobile-apps/#creating-the-server).

Next, create yourself a Windows or Xamarin Azure Mobile Apps project. If you
want a completed application to follow along with, check out the sample app
[here](https://github.com/danderson00/app-service-mobile-dotnet-todo-list-files/tree/master/src/client/MobileAppsFilesSample). Most of the file management specific code is in the
`TodoItemManager` class [here](https://github.com/danderson00/app-service-mobile-dotnet-todo-list-files/blob/master/src/client/MobileAppsFilesSample/TodoItemManager.cs).

All of the functionality is exposed by extension methods on the Azure Mobile Apps
client and table classes. To make the extension methods available, add the
following using statement to the top of your class:

    using Microsoft.WindowsAzure.MobileServices.Files.Express;

### Initialization

Before you can use file management functions, you must first initialize the package:

``` cs
// Create an instance of the MobileServiceClient
var client = new MobileServiceClient("https://your-mobile-app.azurewebsites.net/");
// Create the SQLite store to store local changes
var store = new MobileServiceSQLiteStore("localstore.db");
// Define tables on the store - you will add all of your table types here
store.DefineTable<TodoItem>();
// Initialize file management - you can also pass a path here
client.InitializeManagedFileSyncContext(store);
// Initialize offline sync - add this StoreTrackingOptions value to have file changes synced automatically
await client.SyncContext.InitializeAsync(store, StoreTrackingOptions.AllNotificationsAndChangeDetection);
```

### The API

Now we have all the set up out of the way, we can start saving and retrieving files.
The following methods are available on the `MobileServiceSyncTable` class:

#### AddFileAsync

``` cs
Task<MobileServiceManagedFile> AddFileAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem, string fileName, Stream stream)
```

Adds a file to the file management system. The file is associated with
the provided data item and filename. The content is read from the provided stream.

``` cs
await client.AddFileAsync(todoitem, 'attachment.png', File.OpenRead("IMG0001.png"));
```

#### GetFilesAsync

``` cs
Task<IEnumerable<MobileServiceManagedFile>> GetFilesAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem)
```

Retrieves the list of files associated with the provided data item.

```cs
var filenames = (await client.GetFilesAsync(todoitem)).Select(x => x.Name);
```

#### GetFileAsync

``` cs
Task<Stream> GetFileAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem, string fileName)
```

Retrieves the content of the specified file. The file is associated with the
provided data item and has the specified file name. The content is returned
as a `Stream` object.

```cs
public ImageSource Image
{
    get { return ImageSource.FromStream(() => client.GetFileAsync(todoItem, "attachment.png").Result); }
}
```

#### DeleteFileAsync

``` cs
Task DeleteFileAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem, string fileName)
```

Deletes the file associated with the provided data item and file name from the 
file management system.

``` cs
await client.DeleteFileAsync(todoItem, "attachment.png");
```

#### PushFileChangesAsync

``` cs
Task PushFileChangesAsync<T>(this IMobileServiceSyncTable<T> table)
```

Pushes any changes made to files for the table to the server.

#### PullFilesAsync

``` cs
Task PullFilesAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem)
```

Pulls any changes made to files made on other devices for the table and specific
data item. If StoreTrackingOptions.AllNotificationsAndChangeDetection was
specified when initializing the Mobile Apps SyncContext, this method does not
need to be called - changes are pulled automatically when calling
`table.PullAsync`.
