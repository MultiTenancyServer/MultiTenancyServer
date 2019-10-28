// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace MultiTenancyServer
{
    /// <summary>
    /// Provides access to the current tenant of the scoped process.
    /// </summary>
    /// <typeparam name="TTenant">The type encapsulating a tenant.</typeparam>
    public class TenancyContext<TTenant> : ITenancyContext<TTenant>
        where TTenant : class
    {
        /// <summary>
        /// Gets the current tenant of the scoped process.
        /// </summary>
        /// <value>The current tenant of the scoped process.</value>
        public TTenant Tenant { get; set; }
    }
}
