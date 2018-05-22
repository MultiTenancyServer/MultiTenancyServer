// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using KodeAid.Text.Normalization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MultiTenancyServer;
using MultiTenancyServer.Configuration.DependencyInjection;
using MultiTenancyServer.Options;
using MultiTenancyServer.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods to <see cref="IServiceCollection"/> for configuring multi-tenancy services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds and configures the multi-tenancy system for the specified Tenant type.
        /// </summary>
        /// <typeparam name="TTenant">The type representing a Tenant in the system.</typeparam>
        /// <typeparam name="TKey">The type of the primary key for a tenant.</typeparam>
        /// <param name="services">The services available in the application.</param>
        /// <returns>An <see cref="TenancyBuilder"/> for creating and configuring the multi-tenancy system.</returns>
        public static TenancyBuilder<TTenant, TKey> AddMultiTenancyCore<TTenant, TKey>(this IServiceCollection services)
            where TTenant : class
            where TKey : IEquatable<TKey>
        => services.AddMultiTenancyCore<TTenant, TKey>(o => { });

        /// <summary>
        /// Adds and configures the multi-tenancy system for the specified Tenant type.
        /// </summary>
        /// <typeparam name="TTenant">The type representing a Tenant in the system.</typeparam>
        /// <typeparam name="TKey">The type of the primary key for a tenant.</typeparam>
        /// <param name="services">The services available in the application.</param>
        /// <param name="setup">An action to configure the <see cref="TenancyOptions"/>.</param>
        /// <returns>An <see cref="TenancyBuilder"/> for creating and configuring the multi-tenancy system.</returns>
        public static TenancyBuilder<TTenant, TKey> AddMultiTenancyCore<TTenant, TKey>(this IServiceCollection services, Action<TenancyOptions> setup)
            where TTenant : class
            where TKey : IEquatable<TKey>
        {
            // Services tenancy depends on
            services.AddOptions().AddLogging();

            // Services used by tenancy
            services.TryAddScoped<ITenantValidator<TTenant>, TenantValidator<TTenant>>();
            services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
            // No interface for the error describer so we can add errors without rev'ing the interface
            services.TryAddScoped<TenancyErrorDescriber>();
            services.TryAddScoped<TenantManager<TTenant>, TenantManager<TTenant>>();

            if (setup != null)
            {
                services.Configure(setup);
            }

            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<TenancyOptions>>().Value);
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<TenancyOptions>>().Value?.Tenant);
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<TenancyOptions>>().Value?.Reference);

            return new TenancyBuilder<TTenant, TKey>(services);
        }
    }
}
