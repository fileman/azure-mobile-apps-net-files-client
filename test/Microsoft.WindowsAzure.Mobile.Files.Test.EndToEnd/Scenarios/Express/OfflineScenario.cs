// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Files.Express;
using Microsoft.WindowsAzure.Mobile.Files.Test.EndToEnd.Infrastructure;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Files.Express;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.WindowsAzure.Mobile.Files.Test.EndToEnd.Scenarios.Express
{
    [Trait("End to end: Express offline", "")]
    public class OfflineScenario
    {
        private readonly DataEntity item = new DataEntity { Id = "1" };
        private readonly Stream fileStream = new MemoryStream("Express offline scenario".Select(x => (byte)x).ToArray());

        [Fact(DisplayName = "Files can be added, retrieved and deleted")]
        public async Task BasicScenario()
        {
            await ExecuteAndClearStore(async table =>
            {
                // add the file
                await table.AddFileAsync(item, "test.txt", fileStream);

                // test our local store before syncing
                await TestFiles(table, "test.txt", "Express offline scenario");

                // push our changes to the server
                await table.PushFileChangesAsync();
            });

            await ExecuteAndClearStore(async table =>
            {
                // populate our fresh store by pulling from the server
                await table.PullFilesAsync(item);

                // test that the file we added before has been synced
                await TestFiles(table, "test.txt", "Express offline scenario");

                // delete the file and push
                await table.DeleteFileAsync(item, "test.txt");
                Assert.False(File.Exists("DataEntity-1-test.txt"));
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

        // currently only tests that a single file has been added
        private async Task TestFiles(IMobileServiceSyncTable<DataEntity> table, string name, string content)
        {
            var files = await table.GetFilesAsync(item);
            Assert.Equal(1, files.Count());
            Assert.Equal(name, files.ElementAt(0).Name);
            using (var stream = await table.GetFileAsync(item, name))
            {
                Assert.Equal(content, new StreamReader(stream).ReadToEnd());
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
            client.InitializeExpressFileSyncContext(store);
            await client.SyncContext.InitializeAsync(store);
            return client.GetSyncTable<DataEntity>();
        }
    }
}
