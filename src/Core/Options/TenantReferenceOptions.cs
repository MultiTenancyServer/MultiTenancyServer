// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace MultiTenancyServer.Options
{
    /// <summary>
    /// Used for store specific options.
    /// </summary>
    public class TenantReferenceOptions
    {
        /// <summary>
        /// If set to a non-null value, the store will use this value as the name for the tenant's reference property.
        /// The default is "TenantId".
        /// </summary>
        public string ReferenceName { get; set; } = "TenantId";

        /// <summary>
        /// If set to a positive number, the store will use this value as the max length for any properties used as keys.
        /// The default is 256.
        /// </summary>
        public int MaxLengthForKeys { get; set; } = 256;

        /// <summary>
        /// True to enable indexing of tenant reference properties in the store, otherwise false.
        /// The default is true.
        /// </summary>
        public bool IndexReferences { get; set; } = true;

        /// <summary>
        /// If set to a non-null value, the store will use this value as the name of the index for any tenant references.
        /// The name is also a format pattern of {0:PropertyName}.
        /// The default is "{0}Index", eg. "TenantIdIndex".
        /// </summary>
        public string IndexNameFormat { get; set; } = "{0}Index";

        /// <summary>
        /// Determines if a null tenant reference is allowed for entities and how querying for null tenant references is handled.
        /// </summary>
        public NullTenantReferenceHandling NullTenantReferenceHandling { get; set; } = NullTenantReferenceHandling.NotNullDenyAccess;
    }
}
