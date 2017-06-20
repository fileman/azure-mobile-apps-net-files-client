// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Files.Identity;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.StorageProviders;

namespace Microsoft.WindowsAzure.MobileServices.Files
{
    /// <summary>
    /// Provides extension methods to <see cref="IMobileServiceTable"/> for manipulating files in an online scenario.
    /// </summary>
    public static class MobileServiceTableExtensions
    {
        private readonly static Dictionary<IMobileServiceClient, IMobileServiceFilesClient> filesClients =
            new Dictionary<IMobileServiceClient, IMobileServiceFilesClient>();
        private readonly static object filesClientsSyncRoot = new object();

        /// <summary>
        /// Obtain the <see cref="IMobileServiceFilesClient"/> for the <see cref="IMobileServiceClient"/>
        /// </summary>
        /// <param name="client">The <see cref="IMobileServiceClient"/> instance</param>
        /// <returns>An instance of <see cref="IMobileServiceFilesClient"/></returns>
        public static IMobileServiceFilesClient GetFilesClient(this IMobileServiceClient client)
        {
            lock (filesClientsSyncRoot)
            {
                IMobileServiceFilesClient filesClient;
                
                if (!filesClients.TryGetValue(client, out filesClient))
                {
                    filesClient = new MobileServiceFilesClient(client, new AzureBlobStorageProvider(client));
                    filesClients.Add(client, filesClient);
                }

                return filesClient;
            }
        }

        /// <summary>
        /// Returns a <see cref="MobileServiceFile"/> for each file stored for the provided data item
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceTable{T}"/> instance</param>
        /// <param name="dataItem">The data item to query</param>
        /// <returns>An enumeration of <see cref="MobileServiceFile"/></returns>
        public async static Task<IEnumerable<MobileServiceFile>> GetFilesAsync<T>(this IMobileServiceTable<T> table, T dataItem)
        {
            IMobileServiceFilesClient filesClient = GetFilesClient(table.MobileServiceClient);

            return await filesClient.GetFilesAsync(table.TableName, Utilities.GetDataItemId(dataItem));
        }

        /// <summary>
        /// Adds a file from a <see cref="Stream"/> with the specified file name to be associated with the provided data item
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceTable{T}"/> instance</param>
        /// <param name="dataItem">The data item that the file is associated with</param>
        /// <param name="fileName">The name of the file</param>
        /// <param name="fileStream">A readable stream for the contents of the file</param>
        /// <returns>A <see cref="MobileServiceFile"/> representing the added file</returns>
        public async static Task<MobileServiceFile> AddFileAsync<T>(this IMobileServiceTable<T> table, T dataItem, string fileName, Stream fileStream)
        {
            MobileServiceFile file = CreateFile(table, dataItem, fileName);

            await AddFileAsync(table, file, fileStream);

            return file;
        }

        /// <summary>
        /// Adds a file from a <see cref="Stream"/> with the provided details
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceTable{T}"/> instance</param>
        /// <param name="file">Details for the file</param>
        /// <param name="fileStream">A readable stream for the contents of the file</param>
        public async static Task AddFileAsync<T>(this IMobileServiceTable<T> table, MobileServiceFile file, Stream fileStream)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            if (fileStream == null)
            {
                throw new ArgumentNullException("fileStream");
            }

            IMobileServiceFileDataSource dataSource = new StreamMobileServiceFileDataSource(fileStream);

            IMobileServiceFilesClient client = GetFilesClient(table.MobileServiceClient);
            await client.UploadFileAsync(MobileServiceFileMetadata.FromFile(file), dataSource);
        }

        /// <summary>
        /// Deletes the file with the spcified filename that is associated with the provided data item
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceTable{T}"/> instance</param>
        /// <param name="dataItem">The data item that the file is associated with</param>
        /// <param name="fileName">The name of the file</param>
        public async static Task DeleteFileAsync<T>(this IMobileServiceTable<T> table, T dataItem, string fileName)
        {
            MobileServiceFile file = CreateFile(table, dataItem, fileName);

            await DeleteFileAsync(table, file);
        }

        /// <summary>
        /// Deletes the file with the provided details
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceTable{T}"/> instance</param>
        /// <param name="file">Details for the file</param>
        public async static Task DeleteFileAsync<T>(this IMobileServiceTable<T> table, MobileServiceFile file)
        {
            MobileServiceFileMetadata metadata = MobileServiceFileMetadata.FromFile(file);

            IMobileServiceFilesClient client = GetFilesClient(table.MobileServiceClient);
            await client.DeleteFileAsync(metadata);
        }

        /// <summary>
        /// Obtains the <see cref="Uri"/> for manipulating a file on the storage provider with specified permissions
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceTable{T}"/> instance</param>
        /// <param name="file">Details of the file to obtain a URI for</param>
        /// <param name="permissions">Permissions to grant against the file</param>
        /// <returns>A <see cref="Uri"/> for the file containing an embedded shared access signature (SAS) token</returns>
        public async static Task<Uri> GetFileUriAsync<T>(this IMobileServiceTable<T> table, MobileServiceFile file, StoragePermissions permissions)
        {
            IMobileServiceFilesClient filesClient = GetFilesClient(table.MobileServiceClient);

            return await filesClient.GetFileUriAsync(file, permissions);
        }

        /// <summary>
        /// Downloads the file with the provided details and writes it to the provided <see cref="Stream"/>.
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceTable{T}"/> instance</param>
        /// <param name="file">Details for the file</param>
        /// <param name="fileStream">A writeable <see cref="Stream"/></param>
        public async static Task DownloadFileToStreamAsync<T>(this IMobileServiceTable<T> table, MobileServiceFile file, Stream fileStream)
        {
            IMobileServiceFilesClient filesClient = GetFilesClient(table.MobileServiceClient);
            await filesClient.DownloadToStreamAsync(file, fileStream);
        }

        /// <summary>
        /// Obtains a readable stream for the file with the specified filename associated with the provided data item
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceTable{T}"/> instance</param>
        /// <param name="dataItem">The data item that the file is associated with</param>
        /// <param name="fileName">The name of the file</param>
        /// <returns>A readable <see cref="Stream"/> for the file contents</returns>
        public async static Task<Stream> GetFileAsync<T>(this IMobileServiceTable<T> table, T dataItem, string fileName)
        {
            MobileServiceFile file = CreateFile(table, dataItem, fileName);

            return await GetFileAsync(table, file);
        }

        /// <summary>
        /// Obtains a readable stream for the file with the provided details
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceTable{T}"/> instance</param>
        /// <param name="file">Details for the file</param>
        /// <returns>A readable <see cref="Stream"/> for the file contents</returns>
        public async static Task<Stream> GetFileAsync<T>(this IMobileServiceTable<T> table, MobileServiceFile file)
        {
            IMobileServiceFilesClient filesClient = GetFilesClient(table.MobileServiceClient);
            return await filesClient.GetFileAsync(file);
        }

        public async static Task UploadFromStreamAsync<T>(this IMobileServiceTable<T> table, MobileServiceFile file, Stream fileStream)
        {
            IMobileServiceFileDataSource dataSource = new StreamMobileServiceFileDataSource(fileStream);
            await UploadAsync(table.MobileServiceClient, file, dataSource);
        }
        
        public async static Task UploadFileAsync<T>(this IMobileServiceTable<T> table, MobileServiceFile file, IMobileServiceFileDataSource dataSource)
        {
            await UploadAsync(table.MobileServiceClient, file, dataSource);
        }

        public async static Task UploadAsync(this IMobileServiceClient client, MobileServiceFile file, IMobileServiceFileDataSource dataSource)
        {
            MobileServiceFileMetadata metadata = MobileServiceFileMetadata.FromFile(file);

            IMobileServiceFilesClient filesClient = GetFilesClient(client);
            await filesClient.UploadFileAsync(metadata, dataSource);
        }

        /// <summary>
        /// Creates an instance of <see cref="MobileServiceFile"/> for the specified data item and file name
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceTable{T}"/> instance</param>
        /// <param name="dataItem">The data item that the file is associated with</param>
        /// <param name="fileName">The name of the file</param>
        /// <returns>An instance of <see cref="MobileServiceFile"/> representing the file</returns>
        public static MobileServiceFile CreateFile<T>(this IMobileServiceTable<T> table, T dataItem, string fileName)
        {
            return new MobileServiceFile(fileName, table.TableName, Utilities.GetDataItemId(dataItem));
        }
    }
}
