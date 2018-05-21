// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MultiTenancyServer.Services;
using MultiTenancyServer.Stores;

namespace MultiTenancyServer.Configuration.DependencyInjection
{
    /// <summary>
    /// Helper functions for configuring multi-tenancy services.
    /// </summary>
    /// <typeparam name="TTenant">The type representing a tenant.</typeparam>
    /// <typeparam name="TKey">The type of the primary key for a tenant.</typeparam>
    public class TenancyBuilder<TTenant, TKey>
        where TTenant : class
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Creates a new instance of <see cref="TenancyBuilder"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to attach to.</param>
        public TenancyBuilder(IServiceCollection services)
            : this(typeof(TTenant), services)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="TenancyBuilder"/>.
        /// </summary>
        /// <param name="tenant">The <see cref="Type"/> to use for the tenants.</param>
        /// <param name="services">The <see cref="IServiceCollection"/> to attach to.</param>
        private TenancyBuilder(Type tenant, IServiceCollection services)
        {
            TenantType = tenant;
            Services = services;
        }

        /// <summary>
        /// Gets the <see cref="Type"/> used for tenants.
        /// </summary>
        /// <value>
        /// The <see cref="Type"/> used for tenants.
        /// </value>
        public Type TenantType { get; private set; }

        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> services are attached to.
        /// </summary>
        /// <value>
        /// The <see cref="IServiceCollection"/> services are attached to.
        /// </value>
        public IServiceCollection Services { get; private set; }

        private TenancyBuilder<TTenant, TKey> AddScoped(Type serviceType, Type concreteType)
        {
            Services.AddScoped(serviceType, concreteType);
            return this;
        }

        /// <summary>
        /// Adds an <see cref="ITenantValidator{TTenant}"/> for the <seealso cref="TenantType"/>.
        /// </summary>
        /// <typeparam name="TValidator">The tenant validator type.</typeparam>
        /// <returns>The current <see cref="TenancyBuilder"/> instance.</returns>
        public virtual TenancyBuilder<TTenant, TKey> AddTenantValidator<TValidator>() where TValidator : class
            => AddScoped(typeof(ITenantValidator<>).MakeGenericType(TenantType), typeof(TValidator));

        /// <summary>
        /// Adds an <see cref="TenancyErrorDescriber"/>.
        /// </summary>
        /// <typeparam name="TDescriber">The type of the error describer.</typeparam>
        /// <returns>The current <see cref="TenancyBuilder"/> instance.</returns>
        public virtual TenancyBuilder<TTenant, TKey> AddErrorDescriber<TDescriber>() where TDescriber : TenancyErrorDescriber
        {
            Services.AddScoped<TenancyErrorDescriber, TDescriber>();
            return this;
        }

        /// <summary>
        /// Adds an <see cref="ITenantStore{TTenant}"/> for the <seealso cref="TenantType"/>.
        /// </summary>
        /// <typeparam name="TStore">The tenant store type.</typeparam>
        /// <returns>The current <see cref="TenancyBuilder"/> instance.</returns>
        public virtual TenancyBuilder<TTenant, TKey> AddTenantStore<TStore>() where TStore : class
            => AddScoped(typeof(ITenantStore<>).MakeGenericType(TenantType), typeof(TStore));

        /// <summary>
        /// Adds a <see cref="TenancyManager{TTenant}"/> for the <seealso cref="TenantType"/>.
        /// </summary>
        /// <typeparam name="TTenantManager">The type of the tenant manager to add.</typeparam>
        /// <returns>The current <see cref="TenancyBuilder"/> instance.</returns>
        public virtual TenancyBuilder<TTenant, TKey> AddTenantManager<TTenantManager>() where TTenantManager : class
        {
            var tenantManagerType = typeof(TenancyManager<>).MakeGenericType(TenantType);
            var customType = typeof(TTenantManager);
            if (!tenantManagerType.GetTypeInfo().IsAssignableFrom(customType.GetTypeInfo()))
            {
                throw new InvalidOperationException(string.Format(Resources.InvalidManagerTypeFormat3, customType.Name, typeof(TenancyManager<>).Name.TrimEnd('`', '1'), TenantType.Name));
            }
            if (tenantManagerType != customType)
            {
                Services.AddScoped(customType, services => services.GetRequiredService(tenantManagerType));
            }
            return AddScoped(tenantManagerType, customType);
        }
    }
}
