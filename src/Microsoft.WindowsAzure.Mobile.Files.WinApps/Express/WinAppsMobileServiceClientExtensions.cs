// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Files.Express.LocalStorage.FileSystem;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace Microsoft.WindowsAzure.Mobile.Files.Express
{
    public static class WinAppsMobileServiceClientExtensions
    {
        public static IFileSyncContext InitializeExpressFileSyncContext(this IMobileServiceClient client, IMobileServiceLocalStore store)
        {
            return client.InitializeExpressFileSyncContext(store, ApplicationData.Current.LocalFolder);
        }

        // not sure if this is useful. trying to make the signature consistent across platforms, but this needs to be async
        public async static Task<IFileSyncContext> InitializeExpressFileSyncContext(this IMobileServiceClient client, IMobileServiceLocalStore store, string basePath)
        {
            var folder = await StorageFolder.GetFolderFromPathAsync(basePath).AsTask();
            return client.InitializeExpressFileSyncContext(store, folder);
        }

        public static IFileSyncContext InitializeExpressFileSyncContext(this IMobileServiceClient client, IMobileServiceLocalStore store, StorageFolder folder)
        {
            return client.InitializeExpressFileSyncContext(store, new FileSystemStorageProvider(new WinAppsFileSystemAccess(folder)));
        }
    }
}
