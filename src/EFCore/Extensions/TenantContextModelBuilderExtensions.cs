// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using KodeAid;
using Microsoft.EntityFrameworkCore;

namespace MultiTenancyServer.EntityFramework
{
    /// <summary>
    /// Extensions methods for configuring a <see cref="ModelBuilder"/> with the Tenants table.
    /// </summary>
    public static class TenantContextModelBuilderExtensions
    {
        /// <summary>
        /// Configures the Tenants table on a <see cref="ModelBuilder"/>.
        /// </summary>
        /// <typeparam name="TTenant">The type that represents a tenant.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="storeOptions">The options for configuring the table.</param>
        public static void ConfigureTenantContext<TTenant>(this ModelBuilder builder, TenantStoreOptions storeOptions)
          where TTenant : TenancyTenant
        {
            builder.ConfigureTenantContext<TTenant, string>(storeOptions);
        }

        /// <summary>
        /// Configures the Tenants table on a <see cref="ModelBuilder"/>.
        /// </summary>
        /// <typeparam name="TTenant">The type that represents a tenant.</typeparam>
        /// <typeparam name="TKey">The type that represents a primary key on the Tenants table.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="storeOptions">The options for configuring the table.</param>
        public static void ConfigureTenantContext<TTenant, TKey>(this ModelBuilder builder, TenantStoreOptions storeOptions)
          where TTenant : TenancyTenant<TKey>
          where TKey : IEquatable<TKey>
        {
            ArgCheck.NotNull(nameof(builder), builder);
            ArgCheck.NotNull(nameof(storeOptions), storeOptions);

            builder.Entity<TTenant>(b =>
            {
                b.HasKey(t => t.Id);
                b.Property(t => t.ConcurrencyStamp).IsConcurrencyToken();
                b.Property(t => t.CanonicalName).HasMaxLength(256);
                b.Property(t => t.NormalizedCanonicalName).HasMaxLength(256);
                b.HasIndex(t => t.NormalizedCanonicalName).HasName($"{nameof(TenancyTenant.CanonicalName)}Index").IsUnique();

                if (storeOptions?.Schema != null)
                {
                    b.ToTable(storeOptions?.Name ?? "Tenants", storeOptions.Schema);
                }
                else
                {
                    b.ToTable(storeOptions?.Name ?? "Tenants");
                }
            });
        }
    }
}
