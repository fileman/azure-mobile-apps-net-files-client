using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Files
{
    internal static class Utilities
    {
        internal static string GetDataItemId(object dataItem)
        {
            // TODO: This needs to use the same logic used by the client SDK
            var objectType = dataItem.GetType();
            var propertyInfoList = objectType.GetRuntimeProperties();
            var idProperty = propertyInfoList.Where(x => x.Name.ToLower() == "id").FirstOrDefault();

            if (idProperty != null && idProperty.CanRead)
            {
                return idProperty.GetValue(dataItem) as string;
            }

            return null;
        }
    }
}
