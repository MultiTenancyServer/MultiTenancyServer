// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace MultiTenancyServer.Services
{
    /// <summary>
    /// Provides an abstraction for tenant validation.
    /// </summary>
    /// <typeparam name="TTenant">The type encapsulating a tenant.</typeparam>
    public interface ITenantValidator<TTenant> where TTenant : class
    {
        /// <summary>
        /// Validates the specified <paramref name="tenant"/> as an asynchronous operation.
        /// </summary>
        /// <param name="manager">The <see cref="TenancyManager{TTenant}"/> that can be used to retrieve tenant properties.</param>
        /// <param name="tenant">The tenant to validate.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="TenancyResult"/> of the validation operation.</returns>
        Task<TenancyResult> ValidateAsync(TenancyManager<TTenant> manager, TTenant tenant);
    }
}
