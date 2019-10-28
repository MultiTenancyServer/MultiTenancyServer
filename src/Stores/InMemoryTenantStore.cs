// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KodeAid;
using Microsoft.Extensions.Logging;

namespace MultiTenancyServer.Stores.InMemory
{
    /// <summary>
    /// Represents a new instance of a in-memory store for the specified tenant type.
    /// </summary>
    /// <typeparam name="TTenant">The type representing a tenant.</typeparam>
    /// <typeparam name="TKey">The type of the primary key for a tenant.</typeparam>
    public class InMemoryTenantStore<TTenant, TKey> : TenantStoreBase<TTenant, TKey>,
        IQueryableTenantStore<TTenant>
        where TTenant : TenancyTenant<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly List<TTenant> _tenants = new List<TTenant>();

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="describer">The <see cref="TenancyErrorDescriber"/> used to describe store errors.</param>
        public InMemoryTenantStore(IEnumerable<TTenant> tenants, TenancyErrorDescriber describer, ILogger<InMemoryTenantStore<TTenant, TKey>> logger)
            : base(describer, logger)
        {
            _tenants.AddRange(tenants ?? throw new ArgumentNullException(nameof(tenants)));
        }

        /// <summary>
        /// A navigation property for the tenants the store contains.
        /// </summary>
        public override IQueryable<TTenant> Tenants
        {
            get { return _tenants.AsQueryable(); }
        }

        /// <summary>
        /// Creates the specified <paramref name="tenant"/> in the tenant store.
        /// </summary>
        /// <param name="tenant">The tenant to create.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="TenancyResult"/> of the creation operation.</returns>
        public override Task<TenancyResult> CreateAsync(TTenant tenant, CancellationToken cancellationToken = default)
        {
            ArgCheck.NotNull(nameof(tenant), tenant);
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            _tenants.Add(tenant);

            Logger.LogDebug($"Tenant ID '{{{nameof(TenancyTenant.Id)}}}' created.", tenant.Id);

            return Task.FromResult(TenancyResult.Success);
        }

        /// <summary>
        /// Updates the specified <paramref name="tenant"/> in the tenant store.
        /// </summary>
        /// <param name="tenant">The tenant to update.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="TenancyResult"/> of the update operation.</returns>
        public override Task<TenancyResult> UpdateAsync(TTenant tenant, CancellationToken cancellationToken = default)
        {
            ArgCheck.NotNull(nameof(tenant), tenant);
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            Logger.LogDebug($"Tenant ID '{{{nameof(TenancyTenant.Id)}}}' updated.", tenant.Id);

            return Task.FromResult(TenancyResult.Success);
        }

        /// <summary>
        /// Deletes the specified <paramref name="tenant"/> from the tenant store.
        /// </summary>
        /// <param name="tenant">The tenant to delete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="TenancyResult"/> of the update operation.</returns>
        public override Task<TenancyResult> DeleteAsync(TTenant tenant, CancellationToken cancellationToken = default)
        {
            ArgCheck.NotNull(nameof(tenant), tenant);
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            _tenants.Remove(tenant);

            Logger.LogDebug($"Tenant ID '{{{nameof(TenancyTenant.Id)}}}' deleted.", tenant.Id);

            return Task.FromResult(TenancyResult.Success);
        }

        /// <summary>
        /// Finds and returns a tenant, if any, who has the specified <paramref name="tenantId"/>.
        /// </summary>
        /// <param name="tenantId">The tenant ID to search for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the tenant matching the specified <paramref name="tenantId"/> if it exists.
        /// </returns>
        public override async ValueTask<TTenant> FindByIdAsync(string tenantId, CancellationToken cancellationToken = default)
        {
            ArgCheck.NotNull(nameof(tenantId), tenantId);
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var tenant = await FindTenantAsync(ConvertIdFromString(tenantId), cancellationToken);

            if (tenant == null)
            {
                Logger.LogDebug($"Tenant ID '{{{nameof(TenancyTenant.Id)}}}' not found.", tenantId);
            }
            else
            {
                Logger.LogDebug($"Tenant ID '{{{nameof(TenancyTenant.Id)}}}' found.", tenantId);
            }

            return tenant;
        }

        /// <summary>
        /// Finds and returns a tenant, if any, who has the specified normalized canonical name.
        /// </summary>
        /// <param name="normalizedCanonicalName">The normalized canonical name to search for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the tenant matching the specified <paramref name="normalizedCanonicalName"/> if it exists.
        /// </returns>
        public override ValueTask<TTenant> FindByCanonicalNameAsync(string normalizedCanonicalName, CancellationToken cancellationToken = default)
        {
            ArgCheck.NotNull(nameof(normalizedCanonicalName), normalizedCanonicalName);
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var tenant = _tenants.SingleOrDefault(t => t.NormalizedCanonicalName == normalizedCanonicalName);

            if (tenant == null)
            {
                Logger.LogDebug($"Tenant canonical name '{{{nameof(TenancyTenant.CanonicalName)}}}' not found.", normalizedCanonicalName);
            }
            else
            {
                Logger.LogDebug($"Tenant canonical name '{{{nameof(TenancyTenant.CanonicalName)}}}' found.", normalizedCanonicalName);
            }

            return new ValueTask<TTenant>(tenant);
        }

        /// <summary>
        /// Return a tenant with the matching tenantId if it exists.
        /// </summary>
        /// <param name="tenantId">The tenant's id.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The tenant if it exists.</returns>
        protected override ValueTask<TTenant> FindTenantAsync(TKey tenantId, CancellationToken cancellationToken)
        {
            if (tenantId == null)
            {
                return default;
            }

            return new ValueTask<TTenant>(_tenants.SingleOrDefault(t => t.Id.Equals(tenantId)));
        }
    }
}
