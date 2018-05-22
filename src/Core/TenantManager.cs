// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using KodeAid;
using KodeAid.Text.Normalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MultiTenancyServer.Options;
using MultiTenancyServer.Services;
using MultiTenancyServer.Stores;

namespace MultiTenancyServer
{
    /// <summary>
    /// Provides the APIs for managing tenants in a persistence store.
    /// </summary>
    /// <typeparam name="TTenant">The type encapsulating a tenant.</typeparam>
    public class TenantManager<TTenant> : IDisposable where TTenant : class
    {
        private bool _disposed;
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private readonly IServiceProvider _services;

        /// <summary>
        /// The cancellation token used to cancel operations.
        /// </summary>
        protected virtual CancellationToken CancellationToken => CancellationToken.None;

        /// <summary>
        /// Constructs a new instance of <see cref="TenantManager{TTenant}"/>.
        /// </summary>
        /// <param name="store">The persistence store the manager will operate over.</param>
        /// <param name="optionsAccessor">The accessor used to access the <see cref="TenancyOptions"/>.</param>
        /// <param name="tenantValidators">A collection of <see cref="ITenantValidator{TTenant}"/> to validate tenants against.</param>
        /// <param name="keyNormalizer">The <see cref="ILookupNormalizer"/> to use when generating index keys for tenants.</param>
        /// <param name="errors">The <see cref="TenancyErrorDescriber"/> used to provider error messages.</param>
        /// <param name="services">The <see cref="IServiceProvider"/> used to resolve services.</param>
        /// <param name="logger">The logger used to log messages, warnings and errors.</param>
        public TenantManager(ITenantStore<TTenant> store,
            IOptions<TenancyOptions> optionsAccessor,
            IEnumerable<ITenantValidator<TTenant>> tenantValidators,
            ILookupNormalizer keyNormalizer,
            TenancyErrorDescriber errors,
            IServiceProvider services,
            ILogger<TenantManager<TTenant>> logger)
        {
            ArgCheck.NotNull(nameof(store), store);
            Store = store;
            Options = optionsAccessor?.Value ?? new TenancyOptions();
            KeyNormalizer = keyNormalizer;
            ErrorDescriber = errors;
            Logger = logger;

            if (tenantValidators != null)
            {
                foreach (var v in tenantValidators)
                {
                    TenantValidators.Add(v);
                }
            }

            _services = services;
        }

        /// <summary>
        /// Gets or sets the persistence store the manager operates over.
        /// </summary>
        /// <value>The persistence store the manager operates over.</value>
        protected internal ITenantStore<TTenant> Store { get; set; }

        /// <summary>
        /// The <see cref="ILogger"/> used to log messages from the manager.
        /// </summary>
        /// <value>
        /// The <see cref="ILogger"/> used to log messages from the manager.
        /// </value>
        public virtual ILogger Logger { get; set; }

        /// <summary>
        /// The <see cref="ITenantValidator{TTenant}"/> used to validate tenants.
        /// </summary>
        public IList<ITenantValidator<TTenant>> TenantValidators { get; } = new List<ITenantValidator<TTenant>>();

        /// <summary>
        /// The <see cref="ILookupNormalizer"/> used to normalize things like tenant and role names.
        /// </summary>
        public ILookupNormalizer KeyNormalizer { get; set; }

        /// <summary>
        /// The <see cref="TenancyErrorDescriber"/> used to generate error messages.
        /// </summary>
        public TenancyErrorDescriber ErrorDescriber { get; set; }

        /// <summary>
        /// The <see cref="TenancyOptions"/> used to configure Tenancy.
        /// </summary>
        public TenancyOptions Options { get; set; }

        /// <summary>
        /// Gets a flag indicating whether the backing tenant store supports returning
        /// <see cref="IQueryable"/> collections of information.
        /// </summary>
        /// <value>
        /// True if the backing tenant store supports returning <see cref="IQueryable"/> 
        /// collections of information, otherwise false.</value>
        public virtual bool SupportsQueryableTenants
        {
            get
            {
                ThrowIfDisposed();
                return Store is IQueryableTenantStore<TTenant>;
            }
        }

        /// <summary>
        /// Gets an IQueryable of tenants if the store is an IQueryableTenantStore
        /// </summary>
        public virtual IQueryable<TTenant> Tenants
        {
            get
            {
                if (!(Store is IQueryableTenantStore<TTenant> queryableStore))
                {
                    throw new NotSupportedException(Resources.StoreNotIQueryableTenantStore);
                }
                return queryableStore.Tenants;
            }
        }

        /// <summary>
        /// Releases all resources used by the tenant manager.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Generates a value suitable for use in concurrency tracking.
        /// </summary>
        /// <param name="tenant">The tenant to generate the stamp for.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the security
        /// stamp for the specified <paramref name="tenant"/>.
        /// </returns>
        public virtual Task<string> GenerateConcurrencyStampAsync(TTenant tenant)
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Creates the specified <paramref name="tenant"/> in the backing store,
        /// as an asynchronous operation.
        /// </summary>
        /// <param name="tenant">The tenant to create.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="TenancyResult"/>
        /// of the operation.
        /// </returns>
        public virtual async Task<TenancyResult> CreateAsync(TTenant tenant)
        {
            ThrowIfDisposed();
            var result = await ValidateTenantAsync(tenant);
            if (!result.Succeeded)
            {
                return result;
            }
            await UpdateNormalizedCanonicalNameAsync(tenant);
            return await Store.CreateAsync(tenant, CancellationToken);
        }

        /// <summary>
        /// Updates the specified <paramref name="tenant"/> in the backing store.
        /// </summary>
        /// <param name="tenant">The tenant to update.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="TenancyResult"/>
        /// of the operation.
        /// </returns>
        public virtual Task<TenancyResult> UpdateAsync(TTenant tenant)
        {
            ArgCheck.NotNull(nameof(tenant), tenant);
            ThrowIfDisposed();
            return UpdateTenantAsync(tenant);
        }

        /// <summary>
        /// Deletes the specified <paramref name="tenant"/> from the backing store.
        /// </summary>
        /// <param name="tenant">The tenant to delete.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="TenancyResult"/>
        /// of the operation.
        /// </returns>
        public virtual Task<TenancyResult> DeleteAsync(TTenant tenant)
        {
            ArgCheck.NotNull(nameof(tenant), tenant);
            ThrowIfDisposed();
            return Store.DeleteAsync(tenant, CancellationToken);
        }

        /// <summary>
        /// Finds and returns a tenant, if any, who has the specified <paramref name="tenantId"/>.
        /// </summary>
        /// <param name="tenantId">The tenant ID to search for.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the tenant matching the specified <paramref name="tenantId"/> if it exists.
        /// </returns>
        public virtual Task<TTenant> FindByIdAsync(string tenantId)
        {
            ThrowIfDisposed();
            return Store.FindByIdAsync(tenantId, CancellationToken);
        }

        /// <summary>
        /// Finds and returns a tenant, if any, who has the specified canonical name.
        /// </summary>
        /// <param name="canonicalName">The canonical name to search for.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the tenant matching the specified <paramref name="canonicalName"/> if it exists.
        /// </returns>
        public virtual async Task<TTenant> FindByCanonicalNameAsync(string canonicalName)
        {
            ArgCheck.NotNull(nameof(canonicalName), canonicalName);
            ThrowIfDisposed();
            canonicalName = NormalizeKey(canonicalName);
            var tenant = await Store.FindByCanonicalNameAsync(canonicalName, CancellationToken);
            return tenant;
        }

        /// <summary>
        /// Normalize a key (canonical name) for consistent comparisons.
        /// </summary>
        /// <param name="key">The key to normalize.</param>
        /// <returns>A normalized value representing the specified <paramref name="key"/>.</returns>
        public virtual string NormalizeKey(string key)
        {
            return (KeyNormalizer == null) ? key : KeyNormalizer.Normalize(key);
        }

        /// <summary>
        /// Updates the normalized canonical name for the specified <paramref name="tenant"/>.
        /// </summary>
        /// <param name="tenant">The tenant whose canonical name should be normalized and updated.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public virtual async Task UpdateNormalizedCanonicalNameAsync(TTenant tenant)
        {
            var normalizedCanonicalName = NormalizeKey(await GetCanonicalNameAsync(tenant));
            await Store.SetNormalizedCanonicalNameAsync(tenant, normalizedCanonicalName, CancellationToken);
        }

        /// <summary>
        /// Gets the canonical name for the specified <paramref name="tenant"/>.
        /// </summary>
        /// <param name="tenant">The tenant whose canonical name should be retrieved.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the name for the specified <paramref name="tenant"/>.</returns>
        public virtual async Task<string> GetCanonicalNameAsync(TTenant tenant)
        {
            ArgCheck.NotNull(nameof(tenant), tenant);
            ThrowIfDisposed();
            return await Store.GetCanonicalNameAsync(tenant, CancellationToken);
        }

        /// <summary>
        /// Sets the given <paramref name="canonicalName" /> for the specified <paramref name="tenant"/>.
        /// </summary>
        /// <param name="tenant">The tenant whose canonical name should be set.</param>
        /// <param name="canonicalName">The canonical name to set.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public virtual async Task<TenancyResult> SetCanonicalNameAsync(TTenant tenant, string canonicalName)
        {
            ArgCheck.NotNull(nameof(tenant), tenant);
            ThrowIfDisposed();
            await Store.SetCanonicalNameAsync(tenant, canonicalName, CancellationToken);
            return await UpdateTenantAsync(tenant);
        }

        /// <summary>
        /// Gets the tenant identifier for the specified <paramref name="tenant"/>.
        /// </summary>
        /// <param name="tenant">The tenant whose identifier should be retrieved.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the identifier for the specified <paramref name="tenant"/>.</returns>
        public virtual async Task<string> GetTenantIdAsync(TTenant tenant)
        {
            ThrowIfDisposed();
            return await Store.GetTenantIdAsync(tenant, CancellationToken);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the tenant manager and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                Store.Dispose();
                _disposed = true;
            }
        }

        /// <summary>
        /// Should return <see cref="TenancyResult.Success"/> if validation is successful. This is
        /// called before saving the tenant via Create or Update.
        /// </summary>
        /// <param name="tenant">The tenant</param>
        /// <returns>A <see cref="TenancyResult"/> representing whether validation was successful.</returns>
        protected async Task<TenancyResult> ValidateTenantAsync(TTenant tenant)
        {
            var errors = new List<TenancyError>();
            foreach (var v in TenantValidators)
            {
                var result = await v.ValidateAsync(this, tenant);
                if (!result.Succeeded)
                {
                    errors.AddRange(result.Errors);
                }
            }
            if (errors.Count > 0)
            {
                Logger.LogWarning(13, "Tenant {tenantId} validation failed: {errors}.", await GetTenantIdAsync(tenant), string.Join(";", errors.Select(e => e.Code)));
                return TenancyResult.Failed(errors.ToArray());
            }
            return TenancyResult.Success;
        }

        /// <summary>
        /// Called to update the tenant after validating and updating the normalized canonical name.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <returns>Whether the operation was successful.</returns>
        protected virtual async Task<TenancyResult> UpdateTenantAsync(TTenant tenant)
        {
            var result = await ValidateTenantAsync(tenant);
            if (!result.Succeeded)
            {
                return result;
            }
            await UpdateNormalizedCanonicalNameAsync(tenant);
            return await Store.UpdateAsync(tenant, CancellationToken);
        }

        /// <summary>
        /// Throws if this class has been disposed.
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
