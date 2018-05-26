// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace MultiTenancyServer.Options
{
    /// <summary>
    /// Determines if a null tenant reference is allowed for entities and how querying for null tenant references is handled.
    /// </summary>
    public enum NullTenantReferenceHandling
    {
        /// <summary>
        /// A null tenant reference is NOT allowed for the entity,
        /// where possible a NOT NULL or REQUIRED constraint should be set on the tenant reference,
        /// querying for entities with a null tenant reference will match NO entities.
        /// <para>
        /// This is the default option.
        /// </para>
        /// </summary>
        NotNullDenyAccess = 0,

        /// <summary>
        /// A null tenant reference is allowed for the entity,
        /// where possible an NULLABLE or OPTIONAL constraint should be set on the tenant reference,
        /// querying for entities with a null tenant reference will match those expected results.
        /// <para>
        /// This may be useful where globally defined system entities are set with a null tenant reference.
        /// </para>
        /// </summary>
        NullableEntityAccess = 1,

        /// <summary>
        /// A null tenant reference is NOT allowed for the entity,
        /// where possible a NOT NULL or REQUIRED constraint should be set on the tenant reference,
        /// querying for entities with a null tenant reference will match ALL entities across all tenants.
        /// <para>
        /// For obvious security reasons, this is typically not recommended;
        /// however, this can be useful for admin reporting across all tenants.
        /// </para>
        /// </summary>
        NotNullGlobalAccess = 2,
    }
}
