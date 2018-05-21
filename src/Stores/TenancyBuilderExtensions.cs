// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MultiTenancyServer;
using MultiTenancyServer.Configuration.DependencyInjection;
using MultiTenancyServer.Stores.InMemory;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TenancyBuilderExtensions
    {
        /// <summary>
        /// Adds the in-memory tenant store.
        /// </summary>
        /// <typeparam name="TTenant">The type representing a Tenant in the system.</typeparam>
        /// <typeparam name="TKey">The type of the primary key for a tenant.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="tenants">The tenants.</param>
        /// <returns></returns>
        public static TenancyBuilder<TTenant, TKey> AddInMemoryStore<TTenant, TKey>(this TenancyBuilder<TTenant, TKey> builder, IEnumerable<TTenant> tenants)
            where TTenant : TenancyTenant<TKey>
            where TKey : IEquatable<TKey>
        {
            builder.Services.AddSingleton(tenants);
            builder.AddTenantStore<InMemoryTenantStore<TTenant, TKey>>();
            return builder;
        }

        /// <summary>
        /// Adds the in-memory tenant store.
        /// </summary>
        /// <typeparam name="TTenant">The type representing a Tenant in the system.</typeparam>
        /// <typeparam name="TKey">The type of the primary key for a tenant.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="section">The configuration section containing the configuration data.</param>
        /// <returns></returns>
        public static TenancyBuilder<TTenant, TKey> AddInMemoryStore<TTenant, TKey>(this TenancyBuilder<TTenant, TKey> builder, IConfigurationSection section)
            where TTenant : TenancyTenant<TKey>
            where TKey : IEquatable<TKey>
        {
            var tenants = new List<TTenant>();
            section.Bind(tenants);
            return builder.AddInMemoryStore(tenants);
        }
    }
}
