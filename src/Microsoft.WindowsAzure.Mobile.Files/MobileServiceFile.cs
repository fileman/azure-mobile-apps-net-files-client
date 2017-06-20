// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;

namespace Microsoft.WindowsAzure.MobileServices.Files
{
    /// <summary>
    /// Encapsulates information about a stored file
    /// </summary>
    public class MobileServiceFile
    {
        internal MobileServiceFile() { }

        public MobileServiceFile(string name, string tableName, string parentId)
            : this(name, name, tableName, parentId) { }

        public MobileServiceFile(string id, string name, string tableName, string parentId)
        {
            this.Id = id;
            this.Name = name;
            this.TableName = tableName;
            this.ParentId = parentId;
        }

        /// <summary>
        /// The identifier of the file
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of the file
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The name of the associated table
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// The identifier of the associated data item
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        /// The length of the file
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// The MD5 hash for the file
        /// </summary>
        public string ContentMD5 { get; set; }

        /// <summary>
        /// The last point in time the file was modified
        /// </summary>
        public DateTimeOffset? LastModified { get; set; }

        /// <summary>
        /// The URI for the file in the underlying storage provider
        /// </summary>
        public string StoreUri { get; set; }

        /// <summary>
        /// Additional metadata about the file
        /// </summary>
        public IDictionary<string, string> Metadata { get; set; }

        /// <summary>
        /// Convert a <see cref="MobileServiceFileMetadata"/> into a <see cref="MobileServiceFile"/>
        /// </summary>
        /// <param name="metadata">The <see cref="MobileServiceFileMetadata"/> instance</param>
        /// <returns>An equivalent <see cref="MobileServiceFile"/> instance</returns>
        internal static MobileServiceFile FromMetadata(MobileServiceFileMetadata metadata)
        {
            var file = new MobileServiceFile(metadata.FileId, metadata.ParentDataItemType, metadata.ParentDataItemId);

            file.ContentMD5 = metadata.ContentMD5;
            file.LastModified = metadata.LastModified;
            file.Length = metadata.Length;
            file.Metadata = metadata.ToDictionary();
            return file;
        }
    }
}
