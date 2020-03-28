// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace MultiTenancyServer.EntityFramework
{
    /// <summary>
    /// Used for tenant store specific options.
    /// </summary>
    public class TenantStoreOptions : TableConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantStoreOptions"/> class.
        /// </summary>
        /// <param name="name">Table name.</param>
        public TenantStoreOptions()
            : base(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantStoreOptions"/> class.
        /// </summary>
        /// <param name="name">Table name.</param>
        public TenantStoreOptions(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantStoreOptions"/> class.
        /// </summary>
        /// <param name="name">Table name.</param>
        /// <param name="schema">Schema name.</param>
        public TenantStoreOptions(string name, string schema)
            : base(name, schema)
        {
        }
    }
}
