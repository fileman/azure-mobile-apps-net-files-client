// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Files.Express.LocalStorage.FileSystem
{
#if !WIN_APPS
    public class FileSystemAccess : IFileSystemAccess
    {
        private readonly string basePath;

        public FileSystemAccess(string basePath = "")
        {
            this.basePath = basePath;
        }

        public async Task<Stream> CreateAsync(string targetPath)
        {
            await EnsureFolderExistsAsync(targetPath);   
            return File.Create(GetFullFilePath(targetPath));
        }

        public Task<Stream> OpenReadAsync(string targetPath)
        {
            return Task.FromResult<Stream>(File.OpenRead(GetFullFilePath(targetPath)));
        }

        public Task DeleteAsync(string targetPath)
        {
            File.Delete(GetFullFilePath(targetPath));
            return Task.FromResult(true);
        }

        public Task EnsureFolderExistsAsync(string targetPath)
        {
            if (!string.IsNullOrEmpty(this.basePath) && !Directory.Exists(this.basePath))
                Directory.CreateDirectory(this.basePath);
            return Task.FromResult(true);
        }

        public string GetFullFilePath(string targetPath)
        {
            return Path.Combine(this.basePath, targetPath);
        }
    }
#endif
}
