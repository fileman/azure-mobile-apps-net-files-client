// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices.Files.Metadata
{
    /// <summary>
    /// Encapsulates metadata about a stored file
    /// </summary>
    public class MobileServiceFileMetadata
    {
        /// <summary>
        /// The identifier of the file
        /// </summary>
        public string Id
        {
            get { return this.FileId; }
            set { this.FileId = value; }
        }

        /// <summary>
        /// The identifier of the file
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// The name of the file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The length of the file
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// The MD5 hash for the file
        /// </summary>
        public string ContentMD5 { get; set; }

        public FileLocation Location { get; set; }

        /// <summary>
        /// The last point in time the file was modified
        /// </summary>
        public DateTimeOffset? LastModified { get; set; }

        /// <summary>
        /// The type of the associated data item
        /// </summary>
        public string ParentDataItemType { get; set; }

        /// <summary>
        /// The identifier of the associated data item
        /// </summary>
        public string ParentDataItemId { get; set; }

        /// <summary>
        /// Specifies that the file has been deleted and is awaiting changes to be pushed to the server
        /// </summary>
        public bool PendingDeletion { get; set; }

        /// <summary>
        /// The URI for the file in the underlying storage provider
        /// </summary>
        public string FileStoreUri { get; set; }

        /// <summary>
        /// Serialized additional metadata about the file
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// Convert a <see cref="MobileServiceFile"/> into a <see cref="MobileServiceFileMetadata"/>
        /// </summary>
        /// <param name="file">The <see cref="MobileServiceFile"/> instance</param>
        /// <returns>An equivalent <see cref="MobileServiceFileMetadata"/> instance</returns>
        public static MobileServiceFileMetadata FromFile(MobileServiceFile file)
        {
            return new MobileServiceFileMetadata
            {
                FileId = file.Id,
                FileName = file.Name,
                ContentMD5 = file.ContentMD5,
                LastModified = file.LastModified,
                Length = file.Length,
                ParentDataItemId = file.ParentId,
                ParentDataItemType = file.TableName,
                FileStoreUri = file.StoreUri,
                PendingDeletion = false,
                Metadata = file.Metadata != null ? JsonConvert.SerializeObject(file.Metadata) : null
            };
        }

        internal IDictionary<string, string> ToDictionary()
        {
            if (this.Metadata == null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(this.Metadata);
        }
    }
}
