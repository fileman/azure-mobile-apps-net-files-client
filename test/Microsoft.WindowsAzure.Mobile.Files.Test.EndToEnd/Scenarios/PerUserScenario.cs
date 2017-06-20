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
using System;
using Microsoft.Azure.Mobile.Server.Login;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Configuration;

namespace Microsoft.WindowsAzure.Mobile.Files.Test.EndToEnd.Scenarios
{
    [Trait("End to end: Per user", "")]
    public class PerUserScenario
    {
        private static readonly PerUserDataEntity item1 = new PerUserDataEntity { Id = Guid.NewGuid().ToString() };
        private static readonly PerUserDataEntity item2 = new PerUserDataEntity { Id = Guid.NewGuid().ToString() };

        private static readonly MobileServiceFile file1 = new MobileServiceFile("test.txt", "PerUserDataEntity", item1.Id);
        private static readonly MobileServiceFile file2 = new MobileServiceFile("test.txt", "PerUserDataEntity", item2.Id);

        [Fact(DisplayName = "Files can only be accessed by specific users")]
        public async Task BasicScenario()
        {
            var table1 = GetTable("user1");
            var table2 = GetTable("user2");

            await table1.InsertAsync(item1);
            await table1.AddFileAsync(item1, "test.txt", GetStream("User 1"));
            await table2.InsertAsync(item2);
            await table2.AddFileAsync(item2, "test.txt", GetStream("User 2"));

            // make sure we get the right files when listing
            var files1 = await table1.GetFilesAsync(item1);
            Assert.Equal(1, files1.Count());
            Assert.Equal(item1.Id, files1.ElementAt(0).ParentId);

            var files2 = await table2.GetFilesAsync(item2);
            Assert.Equal(1, files2.Count());
            Assert.Equal(item2.Id, files2.ElementAt(0).ParentId);

            // getting files for correct users succeeds
            await table1.GetFileAsync(new MobileServiceFile("test.txt", "PerUserDataEntity", item1.Id));
            await table2.GetFileAsync(new MobileServiceFile("test.txt", "PerUserDataEntity", item2.Id));

            // getting files for records belonging to other users fails
            await Assert.ThrowsAsync<MobileServiceInvalidOperationException>(async () => await table1.GetFileAsync(file2));
            await Assert.ThrowsAsync<MobileServiceInvalidOperationException>(async () => await table2.GetFileAsync(file1));

            // listing files for records belonging to other users fails
            await Assert.ThrowsAsync<MobileServiceInvalidOperationException>(async () => await table1.GetFilesAsync(item2));
            await Assert.ThrowsAsync<MobileServiceInvalidOperationException>(async () => await table2.GetFilesAsync(item1));

            // listing files for records belonging to other users fails
            await Assert.ThrowsAsync<MobileServiceInvalidOperationException>(async () => await table1.DeleteFileAsync(file2));
            await Assert.ThrowsAsync<MobileServiceInvalidOperationException>(async () => await table2.DeleteFileAsync(file1));

            // deleting files for correct user succeeds
            await table1.DeleteFileAsync(item1, "test.txt");
            await table2.DeleteFileAsync(item2, "test.txt");
        }

        private IMobileServiceTable<PerUserDataEntity> GetTable(string username)
        {
            var client = new MobileServiceClient(ConfigurationManager.AppSettings["MobileAppUrl"]);
            client.CurrentUser = new MobileServiceUser(username) { MobileServiceAuthenticationToken = GetToken(username) };
            return client.GetTable<PerUserDataEntity>();
        }

        private string GetToken(string username)
        {
            return AppServiceLoginHandler.CreateToken(
                new Claim[] { new Claim(JwtRegisteredClaimNames.Sub, username) }, 
                ConfigurationManager.AppSettings["SigningKey"], 
                ConfigurationManager.AppSettings["Audience"], 
                ConfigurationManager.AppSettings["Issuer"], null).RawData;
        }

        private Stream GetStream(string source)
        {
            return new MemoryStream(source.Select(x => (byte)x).ToArray());
        }
    }
}
