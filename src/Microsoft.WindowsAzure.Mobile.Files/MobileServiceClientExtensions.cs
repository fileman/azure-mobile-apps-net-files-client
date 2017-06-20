// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices.Files.Metadata;
using Microsoft.WindowsAzure.MobileServices.Files.Operations;
using Microsoft.WindowsAzure.MobileServices.Files.Sync;
using Microsoft.WindowsAzure.MobileServices.Files.Sync.Triggers;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace Microsoft.WindowsAzure.MobileServices.Files
{
    public static class MobileServiceClientExtensions
    {
        private readonly static Dictionary<IMobileServiceClient, IFileSyncContext> contexts = new Dictionary<IMobileServiceClient, IFileSyncContext>();
        private readonly static object contextsSyncRoot = new object();

        /// <summary>
        /// Initializes the file synchronization capabilities of the <see cref="IMobileServiceClient"/>. Offline sync must also be initialized by calling
        /// IMobileServiceClient.SyncContext.InitializeAsync.
        /// </summary>
        /// <param name="client">IMobileServiceClient instance</param>
        /// <param name="syncHandler">An instance of <see cref="IFileSyncHandler"/> that specifies how to handle changes to stored files</param>
        /// <returns>An instance of <see cref="IFileSyncContext"/></returns>
        public static IFileSyncContext InitializeFileSyncContext(this IMobileServiceClient client, IFileSyncHandler syncHandler)
        {
            if (!client.SyncContext.IsInitialized)
            {
                throw new InvalidOperationException(@"The file sync context cannot be initialized without a MobileServiceLocalStore if offline sync has not been initialized. 
Please initialize offline sync by invoking InitializeAsync on the SyncContext or provide an IMobileServiceLocalStore instance to the InitializeFileSyncContext method.");
            }

            return InitializeFileSyncContext(client, syncHandler, client.SyncContext.Store);
        }

        /// <summary>
        /// Initializes the file synchronization capabilities of the <see cref="IMobileServiceClient"/>. Offline sync must also be initialized by calling
        /// IMobileServiceClient.SyncContext.InitializeAsync.
        /// </summary>
        /// <param name="client">IMobileServiceClient instance</param>
        /// <param name="syncHandler">An instance of <see cref="IFileSyncHandler"/> that specifies how to handle changes to stored files</param>
        /// <param name="store">The <see cref="IMobileServiceLocalStore"/> for storing file metadata locally</param>
        /// <returns>An instance of <see cref="IFileSyncContext"/></returns>
        public static IFileSyncContext InitializeFileSyncContext(this IMobileServiceClient client, IFileSyncHandler syncHandler, IMobileServiceLocalStore store)
        {
            return InitializeFileSyncContext(client, syncHandler, store, new DefaultFileSyncTriggerFactory(client, true));
        }

        /// <summary>
        /// Initializes the file synchronization capabilities of the <see cref="IMobileServiceClient"/>. Offline sync must also be initialized by calling
        /// IMobileServiceClient.SyncContext.InitializeAsync.
        /// </summary>
        /// <param name="client">IMobileServiceClient instance</param>
        /// <param name="syncHandler">An instance of <see cref="IFileSyncHandler"/> that specifies how to handle changes to stored files</param>
        /// <param name="store">The <see cref="IMobileServiceLocalStore"/> for storing file metadata locally</param>
        /// <param name="fileSyncTriggerFactory">An instance of <see cref="IFileSyncTriggerFactory"/> that generates <see cref="IFileSyncTrigger"/> objects for triggering synchronisation of file metadata and content</param>
        /// <returns>An instance of <see cref="IFileSyncContext"/></returns>
        public static IFileSyncContext InitializeFileSyncContext(this IMobileServiceClient client, IFileSyncHandler syncHandler, IMobileServiceLocalStore store, IFileSyncTriggerFactory fileSyncTriggerFactory)
        {
            return client.InitializeFileSyncContext(new MobileServiceFileSyncContext(client, new FileMetadataStore(store), new FileOperationQueue(store), fileSyncTriggerFactory, syncHandler));
        }

        /// <summary>
        /// Initializes the file synchronization capabilities of the <see cref="IMobileServiceClient"/>. Offline sync must also be initialized by calling
        /// IMobileServiceClient.SyncContext.InitializeAsync.
        /// </summary>
        /// <param name="client">IMobileServiceClient instance</param>
        /// <param name="context"></param>
        /// <returns>An instance of <see cref="IFileSyncContext"/></returns>
        public static IFileSyncContext InitializeFileSyncContext(this IMobileServiceClient client, IFileSyncContext context)
        {
            lock (contextsSyncRoot)
            {
                if (contexts.ContainsKey(client))
                {
                    throw new InvalidOperationException("The file sync context has already been initialized.");
                }

                contexts.Add(client, context);
                return context;
            }
        }

        /// <summary>
        /// Returns the current <see cref="IFileSyncContext"/> configured for the client. InitializeFileSyncContext must be called first.
        /// </summary>
        /// <param name="client">IMobileServiceClient instance</param>
        /// <returns>An instance of <see cref="IFileSyncContext"/></returns>
        public static IFileSyncContext GetFileSyncContext(this IMobileServiceClient client)
        {
            IFileSyncContext context;
            if (!contexts.TryGetValue(client, out context))
            {
                if (!client.SyncContext.IsInitialized)
                {
                    throw new InvalidOperationException("The file sync context has not been initialized. Pleae initialize the context by invoking InitializeFileSyncContext.");
                }
            }

            return context;
        }
    }
}
