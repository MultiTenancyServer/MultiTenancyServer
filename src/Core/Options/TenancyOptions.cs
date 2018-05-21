// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace MultiTenancyServer.Options
{
    /// <summary>
    /// Represents all the options you can use to configure the multi-tenancy system.
    /// </summary>
    public class TenancyOptions
    {
        /// <summary>
        /// Gets or sets the <see cref="TenantOptions"/> for the multi-tenancy system.
        /// </summary>
        /// <value>
        /// The <see cref="TenantOptions"/> for the multi-tenancy system.
        /// </value>
        public TenantOptions Tenant { get; set; } = new TenantOptions();

        /// <summary>
        /// Gets or sets the <see cref="TenantReferenceOptions"/> for the multi-tenancy system.
        /// </summary>
        /// <value>
        /// The <see cref="TenantReferenceOptions"/> for the multi-tenancy system.
        /// </value>
        public TenantReferenceOptions Reference { get; set; } = new TenantReferenceOptions();
    }
}
