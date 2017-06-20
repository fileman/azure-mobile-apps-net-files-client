// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Files.Express;
using Microsoft.WindowsAzure.Mobile.Files.Test.EndToEnd.Infrastructure;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Files.Express;
using Microsoft.WindowsAzure.MobileServices.Files.Express.LocalStorage.FileSystem;
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
    [Trait("End to end: Express metadata", "")]
    public class MetadataScenario
    {
        private readonly DerivedDataEntity item = new DerivedDataEntity { Id = "1" };
        private readonly Stream fileStream = new MemoryStream("Express metadata scenario".Select(x => (byte)x).ToArray());

        [Fact(DisplayName = "Metadata is attached to files returned from GetFiles")]
        public async Task BasicScenario()
        {
            await ExecuteAndClearStore(async table =>
            {
                await table.AddFileAsync(item, "test.txt", fileStream);

                // make sure PhysicalPath is attached
                var files = await table.GetFilesAsync(item);
                Assert.Equal(1, files.Count());
                var file = (MobileServiceFileSystemFile) files.ElementAt(0);
                Assert.Equal(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DerivedDataEntity-1-test.txt"), file.PhysicalPath);
                Assert.Equal("test.txt", file.Name);

                // make sure PhysicalPath is correct!
                Assert.Equal("Express metadata scenario", File.ReadAllText(file.PhysicalPath));

                await table.DeleteFileAsync(item, "test.txt");
            });
        }

        private async Task ExecuteAndClearStore(Func<IMobileServiceSyncTable<DerivedDataEntity>, Task> test)
        {
            using (var store = new MobileServiceSQLiteStore("test.sqlite"))
            {
                var table = await GetTableAsync(store);
                await test(table);
            }
            File.Delete("test.sqlite");
        }

        private async Task<IMobileServiceSyncTable<DerivedDataEntity>> GetTableAsync(MobileServiceSQLiteStore store)
        {
            store.DefineTable<DerivedDataEntity>();
            MobileServiceClient client = null;
#if WIN_APPS
            client = new MobileServiceClient((string)ApplicationData.Current.LocalSettings.Values["MobileAppUrl"]);
#else
            client = new MobileServiceClient(ConfigurationManager.AppSettings["MobileAppUrl"]);
#endif
            client.InitializeExpressFileSyncContext(store);
            await client.SyncContext.InitializeAsync(store);
            return client.GetSyncTable<DerivedDataEntity>();
        }
    }
}
