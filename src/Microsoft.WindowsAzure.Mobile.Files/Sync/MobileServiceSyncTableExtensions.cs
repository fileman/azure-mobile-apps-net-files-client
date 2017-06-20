// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace Microsoft.WindowsAzure.MobileServices.Files
{
    public static class MobileServiceSyncTableExtensions
    {
        private static IFileSyncHandler fileSyncHandler;

        internal static void InitializeFileSync(IFileSyncHandler handler)
        {
            fileSyncHandler = handler;
        }

        /// <summary>
        /// Push any changes made to files associated with the table to the server
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceSyncTable{T}"/> instance</param>
        public async static Task PushFileChangesAsync<T>(this IMobileServiceSyncTable<T> table)
        {
            IFileSyncContext context = table.MobileServiceClient.GetFileSyncContext();

            await context.PushChangesAsync(CancellationToken.None);
        }

        /// <summary>
        /// Pull any changes made by other clients to files associated with the provided data item.
        /// The <see cref="IFileSyncHandler"/> provided when initializing file sync determines the
        /// action to take for each type of change.
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceSyncTable{T}"/> instance</param>
        /// <param name="dataItem">The data item that files are associated with</param>
        public async static Task PullFilesAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem)
        {
            IFileSyncContext context = table.MobileServiceClient.GetFileSyncContext();

            await context.PullFilesAsync(table.TableName, Utilities.GetDataItemId(dataItem));
        }

        /// <summary>
        /// Returns a <see cref="MobileServiceFile"/> for each file stored for the provided data item
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceSyncTable{T}"/> instance</param>
        /// <param name="dataItem">The data item to query</param>
        /// <returns>An enumeration of <see cref="MobileServiceFile"/></returns>
        public async static Task<IEnumerable<MobileServiceFile>> GetFilesAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem)
        {
            IFileSyncContext context = table.MobileServiceClient.GetFileSyncContext();

            var fileMetadata = await context.MetadataStore.GetMetadataAsync(table.TableName, Utilities.GetDataItemId(dataItem));

            return fileMetadata.Where(m => !m.PendingDeletion).Select(m => MobileServiceFile.FromMetadata(m));
        }

        /// <summary>
        /// Adds a file with the specified file name to be associated with the provided data item.
        /// The <see cref="IFileSyncHandler"/> provided when initializing file sync determines the
        /// source of the file when changes are pushed to the server.
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceSyncTable{T}"/> instance</param>
        /// <param name="dataItem">The data item that the file is associated with</param>
        /// <param name="fileName">The name of the file</param>
        /// <returns>A <see cref="MobileServiceFile"/> representing the added file</returns>
        public async static Task<MobileServiceFile> AddFileAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem, string fileName)
        {
            MobileServiceFile file = CreateFile(table, dataItem, fileName);

            await AddFileAsync(table, file);

            return file;
        }

        /// <summary>
        /// Adds a file with the provided details. The <see cref="IFileSyncHandler"/> provided when 
        /// initializing file sync determines the source of the file when changes are pushed to the server.
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceSyncTable{T}"/> instance</param>
        /// <param name="file">Details for the file</param>
        public async static Task AddFileAsync<T>(this IMobileServiceSyncTable<T> table, MobileServiceFile file)
        {
            IFileSyncContext context = table.MobileServiceClient.GetFileSyncContext();

            await context.AddFileAsync(file);
        }

        /// <summary>
        /// Deletes the file with the spcified filename that is associated with the provided data item
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceSyncTable{T}"/> instance</param>
        /// <param name="dataItem">The data item that the file is associated with</param>
        /// <param name="fileName">The name of the file</param>
        public async static Task DeleteFileAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem, string fileName)
        {
            MobileServiceFile file = CreateFile(table, dataItem, fileName);

            await DeleteFileAsync(table, file);
        }

        /// <summary>
        /// Deletes the file with the provided details
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceSyncTable{T}"/> instance</param>
        /// <param name="file">Details for the file</param>
        public async static Task DeleteFileAsync<T>(this IMobileServiceSyncTable<T> table, MobileServiceFile file)
        {
            IFileSyncContext context = table.MobileServiceClient.GetFileSyncContext();

            await context.DeleteFileAsync(file);

            MobileServiceFileMetadata metadata = await context.MetadataStore.GetFileMetadataAsync(file.Id);
            metadata.PendingDeletion = true;

            await context.MetadataStore.CreateOrUpdateAsync(metadata);
        }

        /// <summary>
        /// Purges all files stored in the local store for the table
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceSyncTable{T}"/> instance</param>
        public async static Task PurgeFilesAsync<T>(this IMobileServiceSyncTable<T> table)
        {
            IFileSyncContext context = table.MobileServiceClient.GetFileSyncContext();

            await context.MetadataStore.PurgeAsync(table.TableName);
        }

        /// <summary>
        /// Purges all files stored in the local store for the provided data item
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceSyncTable{T}"/> instance</param>
        /// <param name="dataItem">The data item that the files are associated with</param>
        public async static Task PurgeFilesAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem)
        {
            IFileSyncContext context = table.MobileServiceClient.GetFileSyncContext();

            await context.MetadataStore.PurgeAsync(table.TableName, Utilities.GetDataItemId(dataItem));
        }

        /// <summary>
        /// Creates an instance of <see cref="MobileServiceFile"/> for the specified data item and file name
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceTable{T}"/> instance</param>
        /// <param name="dataItem">The data item that the file is associated with</param>
        /// <param name="fileName">The name of the file</param>
        /// <returns>An instance of <see cref="MobileServiceFile"/> representing the file</returns>
        public static MobileServiceFile CreateFile<T>(this IMobileServiceSyncTable<T> table, T dataItem, string fileName)
        {
            return new MobileServiceFile(fileName, table.TableName, Utilities.GetDataItemId(dataItem));
        }
    }
}
