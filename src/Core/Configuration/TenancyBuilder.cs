// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
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
        {
            Services = services;
        }

        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> services are attached to.
        /// </summary>
        /// <value>
        /// The <see cref="IServiceCollection"/> services are attached to.
        /// </value>
        public IServiceCollection Services { get; private set; }

        /// <summary>
        /// Adds an <see cref="ITenantValidator{TTenant}"/> for the <seealso cref="TenantType"/>.
        /// </summary>
        /// <typeparam name="TValidator">The tenant validator type.</typeparam>
        /// <returns>The current <see cref="TenancyBuilder"/> instance.</returns>
        public virtual TenancyBuilder<TTenant, TKey> AddTenantValidator<TValidator>()
            where TValidator : class, ITenantValidator<TTenant>
        {
            Services.AddScoped<ITenantValidator<TTenant>, TValidator>();
            return this;
        }

        /// <summary>
        /// Adds an <see cref="ITenantValidator{TTenant}"/> for the <seealso cref="TenantType"/>.
        /// </summary>
        /// <typeparam name="TValidator">The tenant validator type.</typeparam>
        /// <returns>The current <see cref="TenancyBuilder"/> instance.</returns>
        public virtual TenancyBuilder<TTenant, TKey> AddTenantValidator<TValidator>(Func<IServiceProvider, TValidator> validatorFactory)
            where TValidator : class, ITenantValidator<TTenant>
        {
            Services.AddScoped<ITenantValidator<TTenant>, TValidator>(validatorFactory);
            return this;
        }

        /// <summary>
        /// Adds an <see cref="TenancyErrorDescriber"/>.
        /// </summary>
        /// <typeparam name="TDescriber">The type of the error describer.</typeparam>
        /// <returns>The current <see cref="TenancyBuilder"/> instance.</returns>
        public virtual TenancyBuilder<TTenant, TKey> AddErrorDescriber<TDescriber>()
            where TDescriber : TenancyErrorDescriber
        {
            Services.AddScoped<TenancyErrorDescriber, TDescriber>();
            return this;
        }

        /// <summary>
        /// Adds an <see cref="TenancyErrorDescriber"/>.
        /// </summary>
        /// <typeparam name="TDescriber">The type of the error describer.</typeparam>
        /// <returns>The current <see cref="TenancyBuilder"/> instance.</returns>
        public virtual TenancyBuilder<TTenant, TKey> AddErrorDescriber<TDescriber>(Func<IServiceProvider, TDescriber> describerFactory)
            where TDescriber : TenancyErrorDescriber
        {
            Services.AddScoped<TenancyErrorDescriber, TDescriber>(describerFactory);
            return this;
        }

        /// <summary>
        /// Adds an <see cref="ITenantStore{TTenant}"/> for the <seealso cref="TenantType"/>.
        /// </summary>
        /// <typeparam name="TStore">The tenant store type.</typeparam>
        /// <returns>The current <see cref="TenancyBuilder"/> instance.</returns>
        public virtual TenancyBuilder<TTenant, TKey> AddTenantStore<TStore>()
            where TStore : class, ITenantStore<TTenant>
        {
            Services.AddScoped<ITenantStore<TTenant>, TStore>();
            return this;
        }

        /// <summary>
        /// Adds an <see cref="ITenantStore{TTenant}"/> for the <seealso cref="TenantType"/>.
        /// </summary>
        /// <typeparam name="TStore">The tenant store type.</typeparam>
        /// <returns>The current <see cref="TenancyBuilder"/> instance.</returns>
        public virtual TenancyBuilder<TTenant, TKey> AddTenantStore<TStore>(Func<IServiceProvider, TStore> storeFactory)
            where TStore : class, ITenantStore<TTenant>
        {
            Services.AddScoped<ITenantStore<TTenant>, TStore>(storeFactory);
            return this;
        }

        /// <summary>
        /// Adds a <see cref="TenantManager{TTenant}"/> for the <seealso cref="TenantType"/>.
        /// </summary>
        /// <typeparam name="TManager">The type of the tenant manager to add.</typeparam>
        /// <returns>The current <see cref="TenancyBuilder"/> instance.</returns>
        public virtual TenancyBuilder<TTenant, TKey> AddTenantManager<TManager>()
            where TManager : TenantManager<TTenant>
        {
            Services.AddScoped<TenantManager<TTenant>, TManager>();
            return this;
        }

        /// <summary>
        /// Adds a <see cref="TenantManager{TTenant}"/> for the <seealso cref="TenantType"/>.
        /// </summary>
        /// <typeparam name="TManager">The type of the tenant manager to add.</typeparam>
        /// <returns>The current <see cref="TenancyBuilder"/> instance.</returns>
        public virtual TenancyBuilder<TTenant, TKey> AddTenantManager<TManager>(Func<IServiceProvider, TManager> managerFactory)
            where TManager : TenantManager<TTenant>
        {
            Services.AddScoped<TenantManager<TTenant>, TManager>(managerFactory);
            return this;
        }

        /// <summary>
        /// Adds a <see cref="ITenancyProvider{TTenant}"/> for the <seealso cref="TenantType"/>.
        /// </summary>
        /// <typeparam name="TProvider">The type of the tenancy provider to add.</typeparam>
        /// <returns>The current <see cref="TenancyBuilder"/> instance.</returns>
        public virtual TenancyBuilder<TTenant, TKey> AddTenancyProvider<TProvider>()
            where TProvider : class, ITenancyProvider<TTenant>
        {
            Services.AddScoped<ITenancyProvider<TTenant>, TProvider>();
            return this;
        }

        /// <summary>
        /// Adds a <see cref="ITenancyProvider{TTenant}"/> for the <seealso cref="TenantType"/>.
        /// </summary>
        /// <typeparam name="TProvider">The type of the tenancy provider to add.</typeparam>
        /// <returns>The current <see cref="TenancyBuilder"/> instance.</returns>
        public virtual TenancyBuilder<TTenant, TKey> AddTenancyProvider<TProvider>(Func<IServiceProvider, TProvider> providerFactory)
            where TProvider : class, ITenancyProvider<TTenant>
        {
            Services.AddScoped<ITenancyProvider<TTenant>, TProvider>(providerFactory);
            return this;
        }

        /// <summary>
        /// Adds a <see cref="ITenancyContext{TTenant}"/> for the <seealso cref="TenantType"/>.
        /// </summary>
        /// <typeparam name="TContext">The type of the tenancy context to add.</typeparam>
        /// <returns>The current <see cref="TenancyBuilder"/> instance.</returns>
        public virtual TenancyBuilder<TTenant, TKey> AddTenancyContext<TContext>()
            where TContext : class, ITenancyContext<TTenant>
        {
            Services.AddScoped<ITenancyContext<TTenant>, TContext>();
            return this;
        }

        /// <summary>
        /// Adds a <see cref="ITenancyContext{TTenant}"/> for the <seealso cref="TenantType"/>.
        /// </summary>
        /// <typeparam name="TContext">The type of the tenancy context to add.</typeparam>
        /// <returns>The current <see cref="TenancyBuilder"/> instance.</returns>
        public virtual TenancyBuilder<TTenant, TKey> AddTenancyContext<TContext>(Func<IServiceProvider, TContext> contextFactory)
            where TContext : class, ITenancyContext<TTenant>
        {
            Services.AddScoped<ITenancyContext<TTenant>, TContext>(contextFactory);
            return this;
        }
    }
}
