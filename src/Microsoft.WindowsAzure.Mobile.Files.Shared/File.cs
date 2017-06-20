// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
#if WIN_APPS
using Windows.Storage;
#endif

namespace Microsoft.WindowsAzure.Mobile.Files.IO
{
    internal static class File
    {
#if __IOS__ || __UNIFIED__ || __ANDROID__ || DOTNET
        public static Task<Stream> CreateAsync(string targetPath)
        {
            return Task.FromResult<Stream>(System.IO.File.Create(targetPath));
        }

        public static Task<Stream> OpenReadAsync(string targetPath)
        {
            return Task.FromResult<Stream>(System.IO.File.OpenRead(targetPath));
        }
#elif WIN_APPS
        public async static Task<Stream> CreateAsync(string targetPath)
        {
            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(targetPath));
            StorageFile file = await storageFolder.CreateFileAsync(Path.GetFileName(targetPath), CreationCollisionOption.ReplaceExisting);
            var fStream = await file.OpenAsync(FileAccessMode.ReadWrite);
            return fStream.AsStreamForWrite();
        }

        public async static Task<Stream> OpenReadAsync(string targetPath)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(targetPath);
            return await file.OpenStreamForReadAsync();
        }
#endif
    }
}
