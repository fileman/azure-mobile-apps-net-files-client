// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Files.Express.LocalStorage.FileSystem
{
    public interface IFileSystemAccess
    {
        Task<Stream> CreateAsync(string targetPath);
        Task<Stream> OpenReadAsync(string targetPath);
        Task DeleteAsync(string targetPath);
        Task EnsureFolderExistsAsync(string targetPath);
        string GetFullFilePath(string targetPath);
    }
}
