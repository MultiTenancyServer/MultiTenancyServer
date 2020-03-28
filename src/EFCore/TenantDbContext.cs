// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;

namespace MultiTenancyServer.EntityFramework
{
    /// <summary>
    /// Base class for the Entity Framework database context used for tenancy.
    /// </summary>
    /// <typeparam name="TTenant">The type of the tenant objects.</typeparam>
    public class TenantDbContext<TTenant> : TenantDbContext<TTenant, string>
        where TTenant : TenancyTenant<string>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TenantDbContext{TTenant}"/>.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        public TenantDbContext(DbContextOptions options, TenantStoreOptions storeOptions) : base(options, storeOptions) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantDbContext{TTenant}" /> class.
        /// </summary>
        protected TenantDbContext(TenantStoreOptions storeOptions) : base(storeOptions) { }
    }

    /// <summary>
    /// Base class for the Entity Framework database context used for tenancy.
    /// </summary>
    /// <typeparam name="TTenant">The type of tenant objects.</typeparam>
    /// <typeparam name="TKey">The type of the primary key for tenants.</typeparam>
    public class TenantDbContext<TTenant, TKey> : DbContext, ITenantDbContext<TTenant, TKey>
        where TTenant : TenancyTenant<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly TenantStoreOptions _storeOptions;

        /// <summary>
        /// Initializes a new instance of the db context.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        public TenantDbContext(DbContextOptions options, TenantStoreOptions storeOptions) : base(options)
        {
            _storeOptions = storeOptions;
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        protected TenantDbContext(TenantStoreOptions storeOptions)
        {
            _storeOptions = storeOptions;
        }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> of Tenants.
        /// </summary>
        public DbSet<TTenant> Tenants { get; set; }

        /// <summary>
        /// Configures the schema needed for the tenancy framework.
        /// </summary>
        /// <param name="builder">
        /// The builder being used to construct the model for this context.
        /// </param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ConfigureTenantContext<TTenant, TKey>(_storeOptions);
        }
    }
}
