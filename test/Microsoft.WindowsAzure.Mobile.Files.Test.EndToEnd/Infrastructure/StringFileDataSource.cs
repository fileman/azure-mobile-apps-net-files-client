using Microsoft.WindowsAzure.MobileServices.Files;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Microsoft.WindowsAzure.Mobile.Files.Test.EndToEnd.Infrastructure
{
    public class StringFileDataSource : IMobileServiceFileDataSource
    {
        private readonly string source;

        public StringFileDataSource(string source)
        {
            this.source = source;
        }

        public async Task<Stream> GetStream()
        {
            return new MemoryStream(this.source.Select(x => (byte)x).ToArray());
        }
    }
}
