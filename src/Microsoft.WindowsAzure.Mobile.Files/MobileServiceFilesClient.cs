// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Files.Identity;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.StorageProviders;

namespace Microsoft.WindowsAzure.MobileServices.Files
{
    /// <summary>
    /// Provides functionality for managing remotely stored files
    /// </summary>
    public sealed class MobileServiceFilesClient : IMobileServiceFilesClient
    {
        private IMobileServiceClient client;
        private IStorageProvider storageProvider;

        /// <summary>
        /// Creates an instance of the MobileServiceFilesClient class
        /// </summary>
        /// <param name="client">An instance of the Azure Mobile Apps client</param>
        /// <param name="storageProvider">An instance of a storage provider</param>
        public MobileServiceFilesClient(IMobileServiceClient client, IStorageProvider storageProvider)
        {
            this.client = client;
            this.storageProvider = storageProvider;
        }

        /// <summary>
        /// Returns all files associated with a single record in the specified table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <param name="itemId">Identifier for the record</param>
        /// <returns>A list of <see cref="MobileServiceFile"/> for each file stored</returns>
        public async Task<IEnumerable<MobileServiceFile>> GetFilesAsync(string tableName, string itemId)
        {
            string route = string.Format("/tables/{0}/{1}/MobileServiceFiles", tableName, itemId);

            if (!this.client.SerializerSettings.Converters.Any(p => p is MobileServiceFileJsonConverter))
            {
                this.client.SerializerSettings.Converters.Add(new MobileServiceFileJsonConverter(this.client));
            }

            return await this.client.InvokeApiAsync<IEnumerable<MobileServiceFile>>(route, HttpMethod.Get, null);
        }

        /// <summary>
        /// Upload a file to the storage provider, obtaining the file contents from the provided <see cref="IMobileServiceFileDataSource"/> instance.
        /// </summary>
        /// <param name="metadata">Metadata for the file</param>
        /// <param name="dataSource">Source of hte file content</param>
        public async Task UploadFileAsync(MobileServiceFileMetadata metadata, IMobileServiceFileDataSource dataSource)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }

            if (dataSource == null)
            {
                throw new ArgumentNullException("dataSource");
            }

            StorageToken token = await GetStorageToken(this.client, MobileServiceFile.FromMetadata(metadata), StoragePermissions.Write);

            await this.storageProvider.UploadFileAsync(metadata, dataSource, token);

            var list = await GetFilesAsync(metadata.ParentDataItemType, metadata.ParentDataItemId);
        }

        /// <summary>
        /// Downloads a file from the storage provider and writes it to the provided stream.
        /// </summary>
        /// <param name="file">Details of the file to download</param>
        /// <param name="stream">Writeable stream to write the contents of the file to</param>
        public async Task DownloadToStreamAsync(MobileServiceFile file, Stream stream)
        {
            StorageToken token = await GetStorageToken(this.client, file, StoragePermissions.Read);

            await this.storageProvider.DownloadFileToStreamAsync(file, stream, token);
        }

        /// <summary>
        /// Retrieves a file from the storage provider and returns a readable stream for the file contents.
        /// </summary>
        /// <param name="file">Details of the file to retrieve</param>
        /// <returns>A readable stream for the contents of the file</returns>
        public async Task<Stream> GetFileAsync(MobileServiceFile file)
        {
            StorageToken token = await GetStorageToken(this.client, file, StoragePermissions.Read);

            return await this.storageProvider.GetFileAsync(file, token);
        }

        /// <summary>
        /// Deletes a file from the storage provider
        /// </summary>
        /// <param name="metadata">Details of the file to delete</param>
        public async Task DeleteFileAsync(MobileServiceFileMetadata metadata)
        {
            string route = string.Format("/tables/{0}/{1}/MobileServiceFiles/{2}/", metadata.ParentDataItemType, metadata.ParentDataItemId, metadata.FileName);

            var parameters = new Dictionary<string, string>();
            if (metadata.FileStoreUri != null)
            {
                parameters.Add("x-zumo-filestoreuri", metadata.FileStoreUri);
            }

            await this.client.InvokeApiAsync(route, HttpMethod.Delete, parameters);
        }

        /// <summary>
        /// Obtains the <see cref="Uri"/> for manipulating a file on the storage provider with specified permissions
        /// </summary>
        /// <param name="file">Details of the file to obtain a Uri for</param>
        /// <param name="permissions">Permissions to grant against the file</param>
        /// <returns>A <see cref="Uri"/> for the file containing an embedded shared access signature (SAS) token</returns>
        public async Task<Uri> GetFileUriAsync(MobileServiceFile file, StoragePermissions permissions)
        {
            StorageToken token = await GetStorageToken(this.client, file, permissions);

            return await this.storageProvider.GetFileUriAsync(token, file.Name);
        }

        private async Task<StorageToken> GetStorageToken(IMobileServiceClient client, MobileServiceFileMetadata metadata, StoragePermissions permissions)
        {
            return await GetStorageToken(client, MobileServiceFile.FromMetadata(metadata), permissions);
        }

        private async Task<StorageToken> GetStorageToken(IMobileServiceClient client, MobileServiceFile file, StoragePermissions permissions)
        {

            var tokenRequest = new StorageTokenRequest();
            tokenRequest.Permissions = permissions;
            tokenRequest.TargetFile = file;

            string route = string.Format("/tables/{0}/{1}/StorageToken", file.TableName, file.ParentId);

            return await this.client.InvokeApiAsync<StorageTokenRequest, StorageToken>(route, tokenRequest);
        }
    }
}
