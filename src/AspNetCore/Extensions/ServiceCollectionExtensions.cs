// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MultiTenancyServer;
using MultiTenancyServer.Configuration.DependencyInjection;
using MultiTenancyServer.Http;
using MultiTenancyServer.Options;

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
        public static TenancyBuilder<TTenant, TKey> AddMultiTenancy<TTenant, TKey>(this IServiceCollection services)
            where TTenant : class
            where TKey : IEquatable<TKey>
            => services.AddMultiTenancy<TTenant, TKey>(o => { });

        /// <summary>
        /// Adds and configures the multi-tenancy system for the specified Tenant type.
        /// </summary>
        /// <typeparam name="TTenant">The type representing a Tenant in the system.</typeparam>
        /// <typeparam name="TKey">The type of the primary key for a tenant.</typeparam>
        /// <param name="services">The services available in the application.</param>
        /// <param name="setup">An action to configure the <see cref="TenancyOptions"/>.</param>
        /// <returns>An <see cref="TenancyBuilder"/> for creating and configuring the multi-tenancy system.</returns>
        public static TenancyBuilder<TTenant, TKey> AddMultiTenancy<TTenant, TKey>(this IServiceCollection services, Action<TenancyOptions> setup)
            where TTenant : class
            where TKey : IEquatable<TKey>
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddScoped<ITenancyProvider<TTenant>, HttpTenancyProvider<TTenant>>();
            return services.AddMultiTenancyCore<TTenant, TKey>(setup);
        }
    }
}
