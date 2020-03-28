// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;

namespace MultiTenancyServer.EntityFramework
{
    /// <summary>
    /// Abstraction interface for the Entity Framework database context used for tenancy.
    /// </summary>
    /// <typeparam name="TTenant">The type of tenant objects.</typeparam>
    public interface ITenantDbContext<TTenant> : ITenantDbContext<TTenant, string>
        where TTenant : TenancyTenant
    {
    }

    /// <summary>
    /// Abstraction interface for the Entity Framework database context used for tenancy.
    /// </summary>
    /// <typeparam name="TTenant">The type of tenant objects.</typeparam>
    /// <typeparam name="TKey">The type of the primary key for tenants.</typeparam>
    public interface ITenantDbContext<TTenant, TKey>
        where TTenant : TenancyTenant<TKey>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> of Tenants.
        /// </summary>
        DbSet<TTenant> Tenants { get; set; }
    }
}