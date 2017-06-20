// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices.Files.Express.LocalStorage;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Operations;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Files.Sync.Triggers;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Files.Express
{
    public class MobileServiceExpressFileSyncContext : MobileServiceFileSyncContext
    {
        public MobileServiceExpressFileSyncContext(IMobileServiceClient client, IFileMetadataStore metadataStore, IFileOperationQueue operationsQueue,
            IFileSyncTriggerFactory syncTriggerFactory, IFileSyncHandler syncHandler, IMobileServiceFilesClient filesClient, ILocalStorageProvider localStorage) 
            : base(client, metadataStore, operationsQueue, syncTriggerFactory, syncHandler, filesClient)
        {
            LocalStorage = localStorage;
        }

        public ILocalStorageProvider LocalStorage { get; set; }

        public async Task<Stream> GetFileAsync(MobileServiceFile file)
        {
            return await LocalStorage.GetAsync(file);
        }

        public async Task<MobileServiceExpressFile> AddFileAsync(MobileServiceFile file, Stream stream)
        {
            await base.AddFileAsync(file);
            await LocalStorage.AddAsync(file, stream);
            return LocalStorage.AttachMetadata(file);
        }

        public new async Task DeleteFileAsync(MobileServiceFile file)
        {
            await base.DeleteFileAsync(file);
            await LocalStorage.DeleteAsync(file);
        }
    }
}
