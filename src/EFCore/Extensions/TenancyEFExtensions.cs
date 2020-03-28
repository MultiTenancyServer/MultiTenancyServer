// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using KodeAid;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using MultiTenancyServer;
using MultiTenancyServer.EntityFramework;
using MultiTenancyServer.Options;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Extensions methods for configuring and implementing multi-tenancy in Entity Framework Core.
    /// </summary>
    public static class TenancyEFExtensions
    {
        /// <summary>
        /// Configures a <see cref="ModelBuilder"/> for multi-tenancy.
        /// </summary>
        /// <typeparam name="TTenantKey">Type that represents the tenant key.</typeparam>
        /// <param name="builder">Builder describing the DbContext model.</param>
        /// <param name="options">Options describing how tenanted entities reference their owner tenant.</param>
        /// <param name="tenancyModelState">A static field on the DbContext that will store state about the multi-tenancy configuration for the context.</param>
        /// <param name="unsetTenantKey">An optional value that represents an unset tenant ID on a tenanted entity, by default this will be null for reference types and 0 for integers.</param>
        public static void HasTenancy<TTenantKey>(
            this ModelBuilder builder,
            TenantReferenceOptions options,
            out TenancyModelState<TTenantKey> tenancyModelState,
            TTenantKey unsetTenantKey = default)
            where TTenantKey : IEquatable<TTenantKey>
        {
            tenancyModelState = new TenancyModelState<TTenantKey>()
            {
                UnsetTenantKey = unsetTenantKey,
                DefaultOptions = options
            };
        }

        /// <summary>
        /// Configures an entity to be tenanted (owned by a tenant).
        /// </summary>
        /// <typeparam name="TEntity">Type that represents the entity.</typeparam>
        /// <typeparam name="TTenantKey">Type that represents the tenant key.</typeparam>
        /// <param name="builder">Builder describing the entity.</param>
        /// <param name="tenantId">Expression to read the currently scoped tenant ID.</param>
        /// <param name="tenancyModelState">The same state object that was passed out of <see cref="ModelBuilder"/>.HasTenancy().</param>
        /// <param name="propertyExpression">Expression to access the property on the entity that references the ID of the owning tenant.</param>
        /// <param name="hasIndex">True if the tenant ID reference column should be indexed in the database, this will override any previously configured value in <see cref="TenantReferenceOptions"/>.</param>
        /// <param name="indexNameFormat">Format or name of the index, only applicable if <paramref name="hasIndex"/> is true, this will override any previously configured value in <see cref="TenantReferenceOptions"/>.</param>
        /// <param name="nullTenantReferenceHandling">Determines if a null tenant reference is allowed and how querying for null tenant references is handled.</param>
        public static void HasTenancy<TEntity, TTenantKey>(
            this EntityTypeBuilder<TEntity> builder,
            Expression<Func<TTenantKey>> tenantId,
            TenancyModelState<TTenantKey> tenancyModelState,
            Expression<Func<TEntity, TTenantKey>> propertyExpression,
            bool? hasIndex = null,
            string indexNameFormat = null,
            NullTenantReferenceHandling? nullTenantReferenceHandling = null)
            where TEntity : class
        {
            ArgCheck.NotNull(nameof(builder), builder);
            ArgCheck.NotNull(nameof(tenancyModelState), tenancyModelState);
            ArgCheck.NotNull(nameof(propertyExpression), propertyExpression);

            var propertyName = builder.Property(propertyExpression).Metadata.Name;
            builder.HasTenancy(tenantId, tenancyModelState, propertyName, hasIndex, indexNameFormat, nullTenantReferenceHandling);
        }

        /// <summary>
        /// Configures an entity to be tenanted (owned by a tenant).
        /// </summary>
        /// <typeparam name="TEntity">Type that represents the entity.</typeparam>
        /// <typeparam name="TTenantKey">Type that represents the tenant key.</typeparam>
        /// <param name="builder">Builder describing the entity.</param>
        /// <param name="tenantId">Expression to read the currently scoped tenant ID.</param>
        /// <param name="tenancyModelState">The same state object that was passed out of <see cref="ModelBuilder"/>.HasTenancy().</param>
        /// <param name="propertyName">Name of the property on the entity that references the ID of the owning tenant, if it does not exist a shadow property will be added to the entity's model.</param>
        /// <param name="hasIndex">True if the tenant ID reference column should be indexed in the database, this will override any previously configured value in <see cref="TenantReferenceOptions"/>.</param>
        /// <param name="indexNameFormat">Format or name of the index, only applicable if <paramref name="hasIndex"/> is true, this will override any previously configured value in <see cref="TenantReferenceOptions"/>.</param>
        /// <param name="nullTenantReferenceHandling">Determines if a null tenant reference is allowed and how querying for null tenant references is handled.</param>
        public static void HasTenancy<TEntity, TTenantKey>(
            this EntityTypeBuilder<TEntity> builder,
            Expression<Func<TTenantKey>> tenantId,
            TenancyModelState<TTenantKey> tenancyModelState,
            string propertyName = null,
            bool? hasIndex = null,
            string indexNameFormat = null,
            NullTenantReferenceHandling? nullTenantReferenceHandling = null)
            where TEntity : class
        {
            ArgCheck.NotNull(nameof(builder), builder);
            ArgCheck.NotNull(nameof(tenancyModelState), tenancyModelState);

            // get overrides or defaults
            var tenantKeyType = typeof(TTenantKey);
            propertyName ??= tenancyModelState.DefaultOptions.ReferenceName ?? throw new ArgumentNullException(nameof(propertyName));
            hasIndex ??= tenancyModelState.DefaultOptions.IndexReferences;
            indexNameFormat ??= tenancyModelState.DefaultOptions.IndexNameFormat;
            nullTenantReferenceHandling ??= tenancyModelState.DefaultOptions.NullTenantReferenceHandling;
            tenancyModelState.Properties[typeof(TEntity)] = new TenantReferenceOptions()
            {
                ReferenceName = propertyName,
                IndexReferences = hasIndex.Value,
                IndexNameFormat = indexNameFormat,
                NullTenantReferenceHandling = nullTenantReferenceHandling.Value,
            };

            // add property
            var property = builder.Property(tenantKeyType, propertyName);

            // is required / not null
            if (nullTenantReferenceHandling == NullTenantReferenceHandling.NotNullDenyAccess ||
                nullTenantReferenceHandling == NullTenantReferenceHandling.NotNullGlobalAccess)
            {
                property.IsRequired();
            }

            // add index
            if (hasIndex.Value)
            {
                var index = builder.HasIndex(propertyName);
                if (!string.IsNullOrEmpty(indexNameFormat))
                {
                    index.HasName(string.Format(indexNameFormat, propertyName));
                }
            }

            // add tenant query filter

            // Entity e
            var entityParameter = Expression.Parameter(typeof(TEntity), "e");
            // "TenantId"
            var propertyNameConstant = Expression.Constant(propertyName, typeof(string));
            // EF.Property<long>
            var efPropertyMethod = ((MethodCallExpression)((Expression<Func<object, string, TTenantKey>>)((e, p) => EF.Property<TTenantKey>(e, p))).Body).Method;
            // EF.Property<long>(e, "TenantId")
            var efPropertyCall = Expression.Call(efPropertyMethod, entityParameter, propertyNameConstant);
            // _tenancyContext.Tenant.Id == EF.Property<long>(e, "TenantId")
            var tenantCondition = Expression.Equal(tenantId.Body, efPropertyCall);

            if (nullTenantReferenceHandling == NullTenantReferenceHandling.NotNullDenyAccess ||
                nullTenantReferenceHandling == NullTenantReferenceHandling.NotNullGlobalAccess)
            {
                var nullableTenantKeyType = tenantKeyType.IsValueType ? typeof(Nullable<>).MakeGenericType(tenantKeyType) : tenantKeyType;
                var nullableTenantKey = tenantKeyType.IsValueType ? Expression.Convert(tenantId.Body, nullableTenantKeyType) : tenantId.Body;
                var nullTenantKey = Expression.Constant(null, nullableTenantKeyType);
                if (nullTenantReferenceHandling == NullTenantReferenceHandling.NotNullDenyAccess)
                {
                    // (long?)_tenancyContext.Tenant.Id != (long?)null && _tenancyContext.Tenant.Id == EF.Property<long>(e, "TenantId")
                    tenantCondition = Expression.AndAlso(Expression.NotEqual(nullableTenantKey, nullTenantKey), tenantCondition);
                }
                else if (nullTenantReferenceHandling == NullTenantReferenceHandling.NotNullGlobalAccess)
                {
                    // (long?)_tenancyContext.Tenant.Id == (long?)null || _tenancyContext.Tenant.Id == EF.Property<long>(e, "TenantId")
                    tenantCondition = Expression.OrElse(Expression.Equal(nullableTenantKey, nullTenantKey), tenantCondition);
                }
            }

            var lambda = Expression.Lambda(tenantCondition, entityParameter);
            builder.HasQueryFilter(lambda);
        }

        /// <summary>
        /// Ensures all changes on tenanted entities that are about to be saved to the underlying datastore only reference the currently scoped tenant.
        /// If a tenanted entity has their tenant ID set to that of the 'unset tenant ID' value it will then be set to the ID of the currently scoped tenant.
        /// </summary>
        /// <typeparam name="TTenantKey">Type that represents the tenant key.</typeparam>
        /// <param name="context">Context that has multi-tenancy configured and is calling SaveChanges() or SaveChangesAsync().</param>
        /// <param name="tenantId">ID of the currently scoped tenant.</param>
        /// <param name="tenancyModelState">The same state object that was passed out of <see cref="ModelBuilder"/>.HasTenancy().</param>
        /// <param name="logger">For logging tenancy access related traces.</param>
        public static void EnsureTenancy<TTenantKey>(this DbContext context, TTenantKey? tenantId, TenancyModelState<TTenantKey> tenancyModelState, ILogger logger = null)
            where TTenantKey : struct
        {
            context.EnsureTenancy((object)tenantId, tenancyModelState, logger);
        }

        /// <summary>
        /// Ensures all changes on tenanted entities that are about to be saved to the underlying datastore only reference the currently scoped tenant.
        /// If a tenanted entity has their tenant ID set to that of the 'unset tenant ID' value it will then be set to the ID of the currently scoped tenant.
        /// </summary>
        /// <typeparam name="TTenantKey">Type that represents the tenant key.</typeparam>
        /// <param name="context">Context that has multi-tenancy configured and is calling SaveChanges() or SaveChangesAsync().</param>
        /// <param name="tenantId">ID of the currently scoped tenant.</param>
        /// <param name="tenancyModelState">The same state object that was passed out of <see cref="ModelBuilder"/>.HasTenancy().</param>
        /// <param name="logger">For logging tenancy access related traces.</param>
        public static void EnsureTenancy<TTenantKey>(this DbContext context, TTenantKey tenantId, TenancyModelState<TTenantKey> tenancyModelState, ILogger logger = null)
            where TTenantKey : class
        {
            context.EnsureTenancy((object)tenantId, tenancyModelState, logger);
        }

        /// <summary>
        /// Ensures all changes on tenanted entities that are about to be saved to the underlying datastore only reference the currently scoped tenant.
        /// If a tenanted entity has their tenant ID set to that of the 'unset tenant ID' value it will then be set to the ID of the currently scoped tenant.
        /// </summary>
        /// <typeparam name="TTenantKey">Type that represents the tenant key.</typeparam>
        /// <param name="context">Context that has multi-tenancy configured and is calling SaveChanges() or SaveChangesAsync().</param>
        /// <param name="tenantId">ID of the currently scoped tenant.</param>
        /// <param name="tenancyModelState">The same state object that was passed out of <see cref="ModelBuilder"/>.HasTenancy().</param>
        /// <param name="logger">For logging tenancy access related traces.</param>
        public static void EnsureTenancy<TTenantKey>(this DbContext context, object tenantId, TenancyModelState<TTenantKey> tenancyModelState, ILogger logger)
        {
            ArgCheck.NotNull(nameof(context), context);
            ArgCheck.NotNull(nameof(tenancyModelState), tenancyModelState);

            var tenancyProperties = tenancyModelState.Properties;
            var hasTenancyLookup = new Dictionary<Type, TenantReferenceOptions>();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.State != EntityState.Added &&
                    entry.State != EntityState.Deleted &&
                    entry.State != EntityState.Modified)
                {
                    continue;
                }

                var type = entry.Entity.GetType();
                if (!hasTenancyLookup.TryGetValue(type, out var hasTenancyOptions))
                {
                    tenancyProperties.TryGetValue(type, out hasTenancyOptions);
                    hasTenancyLookup.Add(type, hasTenancyOptions);
                }

                if (hasTenancyOptions == null)
                {
                    continue;
                }

                if (Equals(tenantId, tenancyModelState.UnsetTenantKey))
                {
                    if (hasTenancyOptions.NullTenantReferenceHandling == NullTenantReferenceHandling.NotNullDenyAccess)
                    {
                        throw new InvalidOperationException($"Tenancy context is null - possibly because no scoped tenancy was found.");
                    }

                    if (hasTenancyOptions.NullTenantReferenceHandling == NullTenantReferenceHandling.NotNullGlobalAccess)
                    {
                        var referencedTenantId = entry.Property(hasTenancyOptions.ReferenceName).CurrentValue;

                        if (Equals(referencedTenantId, tenancyModelState.UnsetTenantKey))
                        {
                            throw new InvalidOperationException(
                                $"{hasTenancyOptions.ReferenceName} is required on entity of type {entry.Entity.GetType().Name} and the current tenant from the tenancy context is null - possibly because no scoped tenancy was found.");
                        }

                        continue;
                    }
                }

                // If tenantId is unset here, that's because the entity already has a tenant ID set or
                // the entity allows a null tenant reference (NullTenantReferenceHandling.NullableEntityAccess).

                var accessedTenantId = entry.Property(hasTenancyOptions.ReferenceName).CurrentValue;

                if (!Equals(accessedTenantId, tenancyModelState.UnsetTenantKey))
                {
                    TenancyAccessHelper.CheckTenancyAccess(tenantId, accessedTenantId, logger);
                }
                else
                {
                    entry.Property(hasTenancyOptions.ReferenceName).CurrentValue = tenantId;
                }
            }
        }
    }
}
