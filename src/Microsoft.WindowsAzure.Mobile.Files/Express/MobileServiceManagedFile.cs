namespace Microsoft.WindowsAzure.MobileServices.Files.Express
{
    public class MobileServiceExpressFile : MobileServiceFile
    {
        public static MobileServiceExpressFile FromFile(MobileServiceFile file)
        {
            return new MobileServiceExpressFile
            {
                ContentMD5 = file.ContentMD5,
                Id = file.Id,
                LastModified = file.LastModified,
                Length = file.Length,
                Metadata = file.Metadata,
                Name = file.Name,
                ParentId = file.ParentId,
                StoreUri = file.StoreUri,
                TableName = file.TableName
            };
        }
    }
}
