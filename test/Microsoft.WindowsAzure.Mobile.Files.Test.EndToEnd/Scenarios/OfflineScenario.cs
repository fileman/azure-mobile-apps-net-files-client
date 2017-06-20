using Microsoft.WindowsAzure.Mobile.Files.Test.EndToEnd.Infrastructure;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Globalization;
#if __IOS__ || __UNIFIED__ || __ANDROID__ || DOTNET
using System.Configuration;
#else
using Windows.Storage;
#endif

namespace Microsoft.WindowsAzure.Mobile.Files.Test.EndToEnd.Scenarios
{
    [Trait("End to end: Basic offline scenarios", "")]
    public class OfflineScenario
    {
        private readonly DataEntity item = new DataEntity { Id = "1" };
        private readonly MobileServiceFile file = new MobileServiceFile("test.txt", "DataEntity", "1");
        private readonly Dictionary<string, string> fileContent = new Dictionary<string, string>
        {
            { "test.txt", "Basic scenario" }
        };
        private static string fileName = "test.txt";

        [Fact(DisplayName = "Files can be added, retrieved and deleted")]
        public async Task BlobCanBeUploadedListedRetrievedDeleted()
        {
            await ExecuteAndClearStore(async table =>
            {
                // add the file
                await table.AddFileAsync(file);

                // test our local store before syncing
                var files = await table.GetFilesAsync(item);
                Assert.Equal(1, files.Count());
                Assert.Equal("test.txt", files.ElementAt(0).Name);

                // sync
                await table.PushFileChangesAsync();
            });

            await ExecuteAndClearStore(async table =>
            {
                // test our local store after syncing
                await table.PullFilesAsync(item);
                var files = await table.GetFilesAsync(item);
                Assert.Equal(1, files.Count());
                Assert.Equal("test.txt", files.ElementAt(0).Name);
#if WIN_APPS
                fileName = Path.Combine(ApplicationData.Current.LocalFolder.Path, fileName);
#endif
                // download the file and test content
                await table.DownloadFileAsync(file, fileName);
                var content = File.ReadAllText(fileName);
                Assert.Equal(fileContent["test.txt"], content);
                File.Delete(fileName);

                // delete the file
                await table.DeleteFileAsync(file);
                await table.PushFileChangesAsync();
            });

            await ExecuteAndClearStore(async table =>
            {
                // make sure we really deleted the file
                await table.PullFilesAsync(item);
                var files = await table.GetFilesAsync(item);
                Assert.Equal(0, files.Count());
            });
        }

        [Fact(DisplayName = "Files can be added, retrieved and deleted in French TimeZone")]
        public async Task BlobCanBeUploadedListedRetrievedDeletedFrenchTimeZone()
        {
            var currentCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("co-Fr");
                await BlobCanBeUploadedListedRetrievedDeleted();
            }
            finally
            {
                CultureInfo.DefaultThreadCurrentCulture = currentCulture;
            }
        }

        private async Task ExecuteAndClearStore(Func<IMobileServiceSyncTable<DataEntity>, Task> test)
        {
            using (var store = new MobileServiceSQLiteStore("test.sqlite"))
            {
                var table = await GetTableAsync(store);
                await test(table);
            }
            File.Delete("test.sqlite");
        }

        private async Task<IMobileServiceSyncTable<DataEntity>> GetTableAsync(MobileServiceSQLiteStore store)
        {
            store.DefineTable<DataEntity>();
            MobileServiceClient client = null;
#if WIN_APPS
            client = new MobileServiceClient((string)ApplicationData.Current.LocalSettings.Values["MobileAppUrl"]);
#else
             client = new MobileServiceClient(ConfigurationManager.AppSettings["MobileAppUrl"]);
#endif

            client.InitializeFileSyncContext(new StringFileSyncHandler(fileContent), store);
            await client.SyncContext.InitializeAsync(store);
            return client.GetSyncTable<DataEntity>();
        }
    }
}
