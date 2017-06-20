// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Express;
using Microsoft.WindowsAzure.MobileServices.Files.Express.LocalStorage;
using Microsoft.WindowsAzure.MobileServices.Files.Express.LocalStorage.FileSystem;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Operations;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Files.Sync.Triggers;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System;
using System.IO;

namespace Microsoft.WindowsAzure.Mobile.Files.Express
{
    public static class ExpressMobileServiceClientExtensions
    {
#if !WIN_APPS
        public static IFileSyncContext InitializeExpressFileSyncContext(this IMobileServiceClient client, IMobileServiceLocalStore store, string basePath = "")
        {
            basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), basePath);
            return client.InitializeExpressFileSyncContext(store, new FileSystemStorageProvider(new FileSystemAccess(basePath)));
        }
#endif

        public static IFileSyncContext InitializeExpressFileSyncContext(this IMobileServiceClient client, IMobileServiceLocalStore store, ILocalStorageProvider localStorage)
        {
            var metadataStore = new FileMetadataStore(store);
            var operationQueue = new FileOperationQueue(store);
            var triggerFactory = new DefaultFileSyncTriggerFactory(client, true);
            var filesClient = client.GetFilesClient();
            var syncHandler = new LocalStorageSyncHandler(localStorage, filesClient);

            return client.InitializeFileSyncContext(new MobileServiceExpressFileSyncContext(client, metadataStore, operationQueue, triggerFactory, syncHandler, filesClient, localStorage));
        }
    }
}
