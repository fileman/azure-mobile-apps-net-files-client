using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Files;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.Mobile.Files.Test.EndToEnd.Infrastructure
{
    public class StringFileSyncHandler : IFileSyncHandler
    {
        private readonly Dictionary<string, string> sources;

        public StringFileSyncHandler(Dictionary<string, string> sources)
        {
            this.sources = sources;
        }

        public async Task<IMobileServiceFileDataSource> GetDataSource(MobileServiceFileMetadata metadata)
        {
            return new StringFileDataSource(this.sources[metadata.FileName]);
        }

        public async Task ProcessFileSynchronizationAction(MobileServiceFile file, FileSynchronizationAction action)
        {

        }
    }
}
