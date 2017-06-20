// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices.Sync;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Files.Express
{
    public static class MobileServiceSyncTableExtensions
    {
        /// <summary>
        /// Adds a file from the provided stream to be associated with the provided data item with the specified file name
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceSyncTable{T}"/> instance</param>
        /// <param name="dataItem">The data item that the file is associated with</param>
        /// <param name="fileName">The name of the file</param>
        /// <param name="stream">A readable <see cref="Stream"/> for the file contents</param>
        public async static Task<MobileServiceExpressFile> AddFileAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem, string fileName, Stream stream)
        {
            var context = GetExpressSyncContext(table);
            var file = Files.MobileServiceSyncTableExtensions.CreateFile(table, dataItem, fileName);
            return await context.AddFileAsync(file, stream);
        }

        /// <summary>
        /// Returns a readable <see cref="Stream"/> for the contents of the file with the specified filename that is
        /// associated with the provided data item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="dataItem"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async static Task<Stream> GetFileAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem, string fileName)
        {
            var context = GetExpressSyncContext(table);
            var file = Files.MobileServiceSyncTableExtensions.CreateFile(table, dataItem, fileName);
            return await context.GetFileAsync(file);
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
            var context = GetExpressSyncContext(table);
            var file = Files.MobileServiceSyncTableExtensions.CreateFile(table, dataItem, fileName);
            await context.DeleteFileAsync(file);
        }

        /// <summary>
        /// Returns a <see cref="MobileServiceFile"/> for each file stored for the provided data item
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceSyncTable{T}"/> instance</param>
        /// <param name="dataItem">The data item to query</param>
        /// <returns>An enumeration of <see cref="MobileServiceFile"/></returns>
        public async static Task<IEnumerable<MobileServiceExpressFile>> GetFilesAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem)
        {
            var localStorage = GetExpressSyncContext(table).LocalStorage;
            return (await Files.MobileServiceSyncTableExtensions.GetFilesAsync(table, dataItem)).Select(localStorage.AttachMetadata);
        }

        /// <summary>
        /// Push any changes made to files associated with the table to the server
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceSyncTable{T}"/> instance</param>
        public static Task PushFileChangesAsync<T>(this IMobileServiceSyncTable<T> table)
        {
            return Files.MobileServiceSyncTableExtensions.PushFileChangesAsync(table);
        }

        /// <summary>
        /// Pull any changes made by other clients to files associated with the provided data item
        /// </summary>
        /// <typeparam name="T">The type of the data item</typeparam>
        /// <param name="table">The <see cref="IMobileServiceSyncTable{T}"/> instance</param>
        /// <param name="dataItem">The data item that files are associated with</param>
        public static Task PullFilesAsync<T>(this IMobileServiceSyncTable<T> table, T dataItem)
        {
            return Files.MobileServiceSyncTableExtensions.PullFilesAsync(table, dataItem);
        }

        private static MobileServiceExpressFileSyncContext GetExpressSyncContext<T>(IMobileServiceSyncTable<T> table)
        {
            var syncContext = table.MobileServiceClient.GetFileSyncContext() as MobileServiceExpressFileSyncContext;
            if (syncContext == null)
            {
                throw new InvalidOperationException("To use the Microsoft.WindowsAzure.MobileServices.Files.Express namespace, you must initialize file management by calling InitializeExpressFileSyncContext");
            }
            return syncContext;
        }
    }
}
