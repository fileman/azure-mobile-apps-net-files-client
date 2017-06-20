// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Microsoft.WindowsAzure.MobileServices.Files.Express.LocalStorage.FileSystem
{
    public class WinAppsFileSystemAccess : IFileSystemAccess
    {
        private readonly StorageFolder folder;

        public WinAppsFileSystemAccess(StorageFolder folder)
        {
            this.folder = folder;
        }

        public async Task<Stream> CreateAsync(string targetPath)
        {
            // ReplaceExisting is a partial work around for the fact that record create events are raised twice
            // IO locks can still occur, we need a better solution for this
            var file = await folder.CreateFileAsync(targetPath, CreationCollisionOption.ReplaceExisting);
            return await file.OpenStreamForWriteAsync();
        }

        public async Task<Stream> OpenReadAsync(string targetPath)
        {
            return await folder.OpenStreamForReadAsync(targetPath);
        }

        public async Task DeleteAsync(string targetPath)
        {
            var file = await folder.GetFileAsync(targetPath);
            await file.DeleteAsync();
        }

        public Task EnsureFolderExistsAsync(string targetPath)
        {
            // the folder we were passed the constructor must already exist
            return Task.FromResult(true);
        }

        public string GetFullFilePath(string targetPath)
        {
            return Path.Combine(this.folder.Path, targetPath);
        }
    }
}
