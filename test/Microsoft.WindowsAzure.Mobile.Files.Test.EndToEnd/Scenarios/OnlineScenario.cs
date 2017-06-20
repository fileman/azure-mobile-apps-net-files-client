// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.Mobile.Files.Test.EndToEnd.Infrastructure;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Files;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
#if __IOS__ || __UNIFIED__ || __ANDROID__ || DOTNET
using System.Configuration;
#else
using Windows.Storage;
#endif

namespace Microsoft.WindowsAzure.Mobile.Files.Test.EndToEnd.Scenarios
{
    [Trait("End to end: Basic online", "")]
    public class OnlineScenario
    {
        private readonly DataEntity item = new DataEntity { Id = "1" };

        [Fact(DisplayName = "Files can be added, retrieved and deleted")]
        public async Task BasicScenario()
        {
            var table = GetTable();

            // add a file
            await table.AddFileAsync(item, "test.txt", GetStream("Basic online scenario"));

            // make sure it appears in a list
            var files = await table.GetFilesAsync(item);
            Assert.Equal(1, files.Count());

            // make sure the content can be retrieved
            var retrievedStream = await table.GetFileAsync(item, "test.txt");
            Assert.Equal("Basic online scenario", new StreamReader(retrievedStream).ReadToEnd());

            // delete and make sure it no longer appears in a list
            await table.DeleteFileAsync(item, "test.txt");
            files = await table.GetFilesAsync(item);
            Assert.Equal(0, files.Count());
        }

        private IMobileServiceTable<DataEntity> GetTable()
        {
            MobileServiceClient client = null;
#if WIN_APPS
            client = new MobileServiceClient((string)ApplicationData.Current.LocalSettings.Values["MobileAppUrl"]);
#else
             client = new MobileServiceClient(ConfigurationManager.AppSettings["MobileAppUrl"]);
#endif
            return client.GetTable<DataEntity>();
        }

        private Stream GetStream(string source)
        {
            return new MemoryStream(source.Select(x => (byte)x).ToArray());
        }
    }
}
