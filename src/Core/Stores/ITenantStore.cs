// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace MultiTenancyServer.Stores
{
    /// <summary>
    /// Provides an abstraction for a store which manages tenant accounts.
    /// </summary>
    /// <typeparam name="TTenant">The type encapsulating a tenant.</typeparam>
    public interface ITenantStore<TTenant> : IDisposable where TTenant : class
    {
        /// <summary>
        /// Gets the tenant identifier for the specified <paramref name="tenant"/>.
        /// </summary>
        /// <param name="tenant">The tenant whose identifier should be retrieved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the identifier for the specified <paramref name="tenant"/>.</returns>
        Task<string> GetTenantIdAsync(TTenant tenant, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the canonical name for the specified <paramref name="tenant"/>.
        /// </summary>
        /// <param name="tenant">The tenant whose canonical name should be retrieved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the canonical name for the specified <paramref name="tenant"/>.</returns>
        Task<string> GetCanonicalNameAsync(TTenant tenant, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the given <paramref name="canonicalName" /> for the specified <paramref name="tenant"/>.
        /// </summary>
        /// <param name="tenant">The tenant whose canonical name should be set.</param>
        /// <param name="tenantName">The tenant name to set.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SetCanonicalNameAsync(TTenant tenant, string canonicalName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the normalized canonical name for the specified <paramref name="tenant"/>.
        /// </summary>
        /// <param name="tenant">The tenant whose normalized canonical name should be retrieved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the normalized canonical name for the specified <paramref name="tenant"/>.</returns>
        Task<string> GetNormalizedCanonicalNameAsync(TTenant tenant, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the given normalized canonical name for the specified <paramref name="tenant"/>.
        /// </summary>
        /// <param name="tenant">The tenant whose normalized canonical name should be set.</param>
        /// <param name="normalizedName">The normalized name to set.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SetNormalizedCanonicalNameAsync(TTenant tenant, string normalizedName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates the specified <paramref name="tenant"/> in the tenant store.
        /// </summary>
        /// <param name="tenant">The tenant to create.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="TenancyResult"/> of the creation operation.</returns>
        Task<TenancyResult> CreateAsync(TTenant tenant, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the specified <paramref name="tenant"/> in the tenant store.
        /// </summary>
        /// <param name="tenant">The tenant to update.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="TenancyResult"/> of the update operation.</returns>
        Task<TenancyResult> UpdateAsync(TTenant tenant, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the specified <paramref name="tenant"/> from the tenant store.
        /// </summary>
        /// <param name="tenant">The tenant to delete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="TenancyResult"/> of the update operation.</returns>
        Task<TenancyResult> DeleteAsync(TTenant tenant, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds and returns a tenant, if any, who has the specified <paramref name="tenantId"/>.
        /// </summary>
        /// <param name="tenantId">The tenant ID to search for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the tenant matching the specified <paramref name="tenantId"/> if it exists.</returns>
        Task<TTenant> FindByIdAsync(string tenantId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds and returns a tenant, if any, who has the specified normalized canonical name.
        /// </summary>
        /// <param name="normalizedCanonicalName">The normalized canonical name to search for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the tenant matching the specified <paramref name="normalizedCanonicalName"/> if it exists.</returns>
        Task<TTenant> FindByCanonicalNameAsync(string normalizedCanonicalName, CancellationToken cancellationToken = default);
    }
}
