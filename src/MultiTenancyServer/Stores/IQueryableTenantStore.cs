// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Linq;

namespace MultiTenancyServer.Stores
{
    /// <summary>
    /// Provides an abstraction for querying tenants in a Tenant store.
    /// </summary>
    /// <typeparam name="TTenant">The type encapsulating a tenant.</typeparam>
    public interface IQueryableTenantStore<TTenant> : ITenantStore<TTenant> where TTenant : class
    {
        /// <summary>
        /// Returns an <see cref="IQueryable{T}"/> collection of tenants.
        /// </summary>
        /// <value>An <see cref="IQueryable{T}"/> collection of tenants.</value>
        IQueryable<TTenant> Tenants { get; }
    }
}
