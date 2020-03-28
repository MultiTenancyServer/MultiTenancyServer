// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KodeAid;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MultiTenancyServer.Stores;

namespace MultiTenancyServer.EntityFramework
{
    /// <summary>
    /// Creates a new instance of a persistence store for the specified tenant type.
    /// </summary>
    /// <typeparam name="TTenant">The type representing a tenant.</typeparam>
    public class TenantStore<TTenant> : TenantStore<TTenant, TenantDbContext<TTenant>, string>
        where TTenant : TenancyTenant<string>
    {
        /// <summary>
        /// Constructs a new instance of <see cref="TenantStore{TTenant}"/>.
        /// </summary>
        /// <param name="context">The <see cref="DbContext"/>.</param>
        /// <param name="describer">The <see cref="TenancyErrorDescriber"/>.</param>
        public TenantStore(TenantDbContext<TTenant> context, ILogger<TenantStore<TTenant>> logger, TenancyErrorDescriber describer = null)
            : base(context, logger, describer) { }
    }

    /// <summary>
    /// Represents a new instance of a persistence store for the specified tenant and context types.
    /// </summary>
    /// <typeparam name="TTenant">The type representing a tenant.</typeparam>
    /// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
    public class TenantStore<TTenant, TContext> : TenantStore<TTenant, TContext, string>
        where TTenant : TenancyTenant<string>
        where TContext : DbContext, ITenantDbContext<TTenant, string>
    {
        /// <summary>
        /// Constructs a new instance of <see cref="TenantStore{TTenant, TContext}"/>.
        /// </summary>
        /// <param name="context">The <see cref="DbContext"/>.</param>
        /// <param name="describer">The <see cref="TenancyErrorDescriber"/>.</param>
        public TenantStore(TContext context, ILogger<TenantStore<TTenant, TContext>> logger, TenancyErrorDescriber describer = null)
            : base(context, logger, describer) { }
    }

    /// <summary>
    /// Represents a new instance of a persistence store for the specified tenant context, and key types.
    /// </summary>
    /// <typeparam name="TTenant">The type representing a tenant.</typeparam>
    /// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
    /// <typeparam name="TKey">The type of the primary key for a tenant.</typeparam>
    public class TenantStore<TTenant, TContext, TKey> :
        TenantStoreBase<TTenant, TKey>,
        IQueryableTenantStore<TTenant>
        where TTenant : TenancyTenant<TKey>
        where TContext : DbContext, ITenantDbContext<TTenant, TKey>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Constructs a new instance of <see cref="TenantStore{TTenant, TRole, TContext, TKey}"/>.
        /// </summary>
        /// <param name="context">The <see cref="DbContext"/>.</param>
        /// <param name="describer">The <see cref="TenancyErrorDescriber"/>.</param>
        public TenantStore(TContext context, ILogger<TenantStore<TTenant, TContext, TKey>> logger, TenancyErrorDescriber describer = null)
           : base(describer ?? new TenancyErrorDescriber(), logger)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Gets the database context for this store.
        /// </summary>
        public TContext Context { get; private set; }

        /// <summary>
        /// Gets or sets a flag indicating if changes should be persisted after CreateAsync, UpdateAsync and DeleteAsync are called.
        /// </summary>
        /// <value>
        /// True if changes should be automatically persisted, otherwise false.
        /// </value>
        public bool AutoSaveChanges { get; set; } = true;

        /// <summary>Saves the current store.</summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        protected Task SaveChanges(CancellationToken cancellationToken)
        {
            return AutoSaveChanges ? Context.SaveChangesAsync(cancellationToken) : Task.CompletedTask;
        }

        /// <summary>
        /// Creates the specified <paramref name="tenant"/> in the tenant store.
        /// </summary>
        /// <param name="tenant">The tenant to create.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="TenancyResult"/> of the creation operation.</returns>
        public override async Task<TenancyResult> CreateAsync(TTenant tenant, CancellationToken cancellationToken = default)
        {
            ArgCheck.NotNull(nameof(tenant), tenant);
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            Context.Add(tenant);
            await SaveChanges(cancellationToken);
            return TenancyResult.Success;
        }

        /// <summary>
        /// Updates the specified <paramref name="tenant"/> in the tenant store.
        /// </summary>
        /// <param name="tenant">The tenant to update.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="TenancyResult"/> of the update operation.</returns>
        public override async Task<TenancyResult> UpdateAsync(TTenant tenant, CancellationToken cancellationToken = default)
        {
            ArgCheck.NotNull(nameof(tenant), tenant);
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            Context.Attach(tenant);
            tenant.ConcurrencyStamp = Guid.NewGuid().ToString();
            Context.Update(tenant);

            try
            {
                await SaveChanges(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return TenancyResult.Failed(ErrorDescriber.ConcurrencyFailure());
            }

            return TenancyResult.Success;
        }

        /// <summary>
        /// Deletes the specified <paramref name="tenant"/> from the tenant store.
        /// </summary>
        /// <param name="tenant">The tenant to delete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="TenancyResult"/> of the update operation.</returns>
        public override async Task<TenancyResult> DeleteAsync(TTenant tenant, CancellationToken cancellationToken = default)
        {
            ArgCheck.NotNull(nameof(tenant), tenant);
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            Context.Remove(tenant);

            try
            {
                await SaveChanges(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return TenancyResult.Failed(ErrorDescriber.ConcurrencyFailure());
            }

            return TenancyResult.Success;
        }

        /// <summary>
        /// Finds and returns a tenant, if any, who has the specified <paramref name="tenantId"/>.
        /// </summary>
        /// <param name="tenantId">The tenant ID to search for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>
        /// The <see cref="ValueTask"/> that represents the asynchronous operation, containing the tenant matching the specified <paramref name="tenantId"/> if it exists.
        /// </returns>
        public override ValueTask<TTenant> FindByIdAsync(string tenantId, CancellationToken cancellationToken = default)
        {
            ArgCheck.NotNull(nameof(tenantId), tenantId);
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var id = ConvertIdFromString(tenantId);
            return Context.Tenants.FindAsync(new object[] { id }, cancellationToken);
        }

        /// <summary>
        /// Finds and returns a tenant, if any, who has the specified normalized canonical name.
        /// </summary>
        /// <param name="normalizedCanonicalName">The normalized canonical name to search for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>
        /// The <see cref="ValueTask"/> that represents the asynchronous operation, containing the tenant matching the specified <paramref name="normalizedCanonicalName"/> if it exists.
        /// </returns>
        public override ValueTask<TTenant> FindByCanonicalNameAsync(string normalizedCanonicalName, CancellationToken cancellationToken = default)
        {
            ArgCheck.NotNull(nameof(normalizedCanonicalName), normalizedCanonicalName);
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return new ValueTask<TTenant>(Tenants.FirstOrDefaultAsync(u => u.NormalizedCanonicalName == normalizedCanonicalName, cancellationToken));
        }

        /// <summary>
        /// A navigation property for the tenants the store contains.
        /// </summary>
        public override IQueryable<TTenant> Tenants
        {
            get { return Context.Tenants; }
        }

        /// <summary>
        /// Return a tenant with the matching tenantId if it exists.
        /// </summary>
        /// <param name="tenantId">The tenant's id.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The tenant if it exists.</returns>
        protected override ValueTask<TTenant> FindTenantAsync(TKey tenantId, CancellationToken cancellationToken)
        {
            return new ValueTask<TTenant>(Tenants.SingleOrDefaultAsync(u => u.Id.Equals(tenantId), cancellationToken));
        }
    }
}
