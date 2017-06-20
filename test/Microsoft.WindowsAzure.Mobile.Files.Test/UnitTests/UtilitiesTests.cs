using Microsoft.WindowsAzure.MobileServices.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.WindowsAzure.Mobile.Files.Test.UnitTests
{
    public class LowerCaseId
    {
        public string id
        {
            get; set;
        }
    }

    public class UpperCaseId
    {
        public string ID
        {
            get; set;
        }
    }

    public class DerivedId : LowerCaseId
    {
       
    }

    public class UtilitiesTests
    {
        [Fact]
        public void VerifyIdNotNull()
        {
            LowerCaseId testLowerCaseId = new LowerCaseId { id = "1" };
            UpperCaseId testUppderCaseId = new UpperCaseId { ID = "1" };
            DerivedId testId = new DerivedId { id = "1" };

            Assert.NotNull(Utilities.GetDataItemId(testLowerCaseId));
            Assert.NotNull(Utilities.GetDataItemId(testUppderCaseId));
            Assert.NotNull(Utilities.GetDataItemId(testId));
        }
    }
}
