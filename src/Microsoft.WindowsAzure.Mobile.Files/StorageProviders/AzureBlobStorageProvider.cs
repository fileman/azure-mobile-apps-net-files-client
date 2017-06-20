// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Files.Identity;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.WindowsAzure.MobileServices.Files.StorageProviders
{
    public class AzureBlobStorageProvider : IStorageProvider
    {
        private readonly IMobileServiceClient mobileServiceClient;

        public AzureBlobStorageProvider(IMobileServiceClient client)
        {
            this.mobileServiceClient = client;
        }

        public async Task UploadFileAsync(MobileServiceFileMetadata metadata, IMobileServiceFileDataSource dataSource, StorageToken storageToken)
        {
            using (var stream = await dataSource.GetStream())
            {
                CloudBlockBlob blob = await GetBlobReference(storageToken, metadata.FileName);
                await blob.UploadFromStreamAsync(stream);
                //await blob.FetchAttributesAsync();

                metadata.LastModified = blob.Properties.LastModified;
                metadata.FileStoreUri = blob.Uri.LocalPath;
                

                stream.Position = 0;
            }
        }

        public async Task DownloadFileToStreamAsync(MobileServiceFile file, Stream stream, StorageToken storageToken)
        {
            CloudBlockBlob blob = await GetBlobReference(storageToken, file.Name);

            await blob.DownloadToStreamAsync(stream);
        }

        public async Task<Stream> GetFileAsync(MobileServiceFile file, StorageToken token)
        {
            CloudBlockBlob blob = await GetBlobReference(token, file.Name);

            return await blob.OpenReadAsync();
        }

        public async Task<Uri> GetFileUriAsync(StorageToken storageToken, string fileName)
        {
            CloudBlockBlob blob = await GetBlobReference(storageToken, fileName);

            return new Uri(blob.Uri, storageToken.RawToken);
        }

        private async Task<CloudBlockBlob> GetBlobReference(StorageToken token, string fileName)
        {
            CloudBlockBlob blob = null;

            if (token.Scope == StorageTokenScope.File)
            {
                blob = new CloudBlockBlob(new Uri(token.ResourceUri, token.RawToken));
            }
            else if (token.Scope == StorageTokenScope.Record)
            {
                var container = new CloudBlobContainer(new Uri(token.ResourceUri, token.RawToken));

                blob = container.GetBlockBlobReference(fileName);
            }


            return blob;
        }
    }
}
