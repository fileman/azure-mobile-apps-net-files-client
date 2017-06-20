using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Express.LocalStorage.FileSystem;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.WindowsAzure.Mobile.Files.Test.Scenarios.Express.LocalStorage
{
    [Trait("FileSystemStorageProvider: Basic scenario", "")]
    public class FileSystemStorageProviderScenario
    {
        private MobileServiceFile file = new MobileServiceFile
        {
            Name = "test.txt",
            ParentId = "1",
            TableName = "table"
        };

        [Fact(DisplayName = "Files can be added, retrieved and deleted")]
        public async Task BasicScenario()
        {
            var provider = new FileSystemStorageProvider(new FileSystemAccess());
            await provider.AddAsync(file, GetStream("this is a test"));
            Assert.True(File.Exists("table-1-test.txt"));

            using (var contentStream = await provider.GetAsync(file))
            {
                Assert.Equal("this is a test", new StreamReader(contentStream).ReadToEnd());
            }

            await provider.DeleteAsync(file);
            Assert.False(File.Exists("table-1-test.txt"));
        }

        [Fact(DisplayName = "Target folder is created")]
        public async Task TargetFolderIsCreated()
        {
            var provider = new FileSystemStorageProvider(new FileSystemAccess("files"));
            await provider.AddAsync(new MobileServiceFile("test.txt", "table", "1"), new MemoryStream());
            Assert.True(Directory.Exists("files"));
            Directory.Delete("files", true);
        }

        [Fact(DisplayName = "Target folder can be an absolute path")]
        public async Task AbsoluteTargetFolderIsCreated()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "files");
            var provider = new FileSystemStorageProvider(new FileSystemAccess(path));
            await provider.AddAsync(new MobileServiceFile("test.txt", "table", "1"), new MemoryStream());
            Assert.True(Directory.Exists(path));
            Directory.Delete(path, true);
        }

        private Stream GetStream(string source)
        {
            return new MemoryStream(source.Select(x => (byte)x).ToArray());
        }
    }
}
