// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices.Files.Express.LocalStorage;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Files.Express
{
    public class LocalStorageSyncHandler : IFileSyncHandler
    {
        private ILocalStorageProvider localStorage;
        private IMobileServiceFilesClient client;

        public LocalStorageSyncHandler(ILocalStorageProvider localStorage, IMobileServiceFilesClient client)
        {
            this.localStorage = localStorage;
            this.client = client;
        }

        public async Task<IMobileServiceFileDataSource> GetDataSource(MobileServiceFileMetadata metadata)
        {
            var stream = await localStorage.GetAsync(MobileServiceFile.FromMetadata(metadata));
            return new StreamMobileServiceFileDataSource(stream);
        }

        public async Task ProcessFileSynchronizationAction(MobileServiceFile file, FileSynchronizationAction action)
        {
            switch(action)
            {
                case FileSynchronizationAction.Create:
                case FileSynchronizationAction.Update:
                    var stream = await client.GetFileAsync(file);
                    await localStorage.AddAsync(file, stream);
                    break;
                case FileSynchronizationAction.Delete:
                    await localStorage.DeleteAsync(file);
                    break;
            }
        }
    }
}
