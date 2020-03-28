// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MultiTenancyServer;
using MultiTenancyServer.Configuration.DependencyInjection;
using MultiTenancyServer.EntityFramework;
using MultiTenancyServer.Stores;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains extension methods to <see cref="ITenancyBuilder{TTenant, TKey}"/> for adding entity framework stores.
    /// </summary>
    public static class TenancyBuilderExtensions
    {
        /// <summary>
        /// Adds an Entity Framework implementation of multi-tenancy information stores.
        /// </summary>
        /// <typeparam name="TContext">The Entity Framework database context to use.</typeparam>
        /// <param name="builder">The <see cref="TenancyBuilder"/> instance this method extends.</param>
        /// <returns>The <see cref="TenancyBuilder"/> instance this method extends.</returns>
        public static TenancyBuilder<TTenant, TKey> AddEntityFrameworkStore<TContext, TTenant, TKey>(
            this TenancyBuilder<TTenant, TKey> builder, 
            Func<IServiceProvider, TContext> contextFactory = null)
            where TContext : DbContext, ITenantDbContext<TTenant, TKey>
            where TTenant : TenancyTenant<TKey>
            where TKey : IEquatable<TKey>
        {
            if (contextFactory != null)
            {
                builder.Services.TryAddScoped<ITenantStore<TTenant>>(sp =>
                    new TenantStore<TTenant, TContext, TKey>(
                        contextFactory(sp),
                        sp.GetRequiredService<ILogger<TenantStore<TTenant, TContext, TKey>>>(),
                        sp.GetService<TenancyErrorDescriber>()));
            }
            else
            {
                builder.Services.TryAddScoped<ITenantStore<TTenant>, TenantStore<TTenant, TContext, TKey>>();
            }

            return builder;
        }

        private static TypeInfo FindGenericBaseType(Type currentType, Type genericBaseType)
        {
            var type = currentType;

            while (type != null)
            {
                var typeInfo = type.GetTypeInfo();
                var genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;

                if (genericType != null && genericType == genericBaseType)
                {
                    return typeInfo;
                }

                type = type.BaseType;
            }

            return null;
        }
    }
}
