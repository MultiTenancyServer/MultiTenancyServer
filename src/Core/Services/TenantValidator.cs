// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KodeAid;

namespace MultiTenancyServer.Services
{
    /// <summary>
    /// Provides validation services for tenant classes.
    /// </summary>
    /// <typeparam name="TTenant">The type encapsulating a tenant.</typeparam>
    public class TenantValidator<TTenant> : ITenantValidator<TTenant> where TTenant : class
    {
        /// <summary>
        /// Creates a new instance of <see cref="TenantValidator{TTenant}"/>/
        /// </summary>
        /// <param name="errors">The <see cref="TenancyErrorDescriber"/> used to provider error messages.</param>
        public TenantValidator(TenancyErrorDescriber errors = null)
        {
            Describer = errors ?? new TenancyErrorDescriber();
        }

        /// <summary>
        /// Gets the <see cref="TenancyErrorDescriber"/> used to provider error messages for the current <see cref="TenantValidator{TTenant}"/>.
        /// </summary>
        /// <value>The <see cref="TenancyErrorDescriber"/> used to provider error messages for the current <see cref="TenantValidator{TTenant}"/>.</value>
        public TenancyErrorDescriber Describer { get; private set; }

        /// <summary>
        /// Validates the specified <paramref name="tenant"/> as an asynchronous operation.
        /// </summary>
        /// <param name="manager">The <see cref="TenancyManager{TTenant}"/> that can be used to retrieve tenant properties.</param>
        /// <param name="tenant">The tenant to validate.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="TenancyResult"/> of the validation operation.</returns>
        public virtual async Task<TenancyResult> ValidateAsync(TenancyManager<TTenant> manager, TTenant tenant)
        {
            ArgCheck.NotNull(nameof(manager), manager);
            ArgCheck.NotNull(nameof(tenant), tenant);
            var errors = new List<TenancyError>();
            await ValidateCanonicalName(manager, tenant, errors);
            return errors.Count > 0 ? TenancyResult.Failed(errors.ToArray()) : TenancyResult.Success;
        }

        private async Task ValidateCanonicalName(TenancyManager<TTenant> manager, TTenant tenant, ICollection<TenancyError> errors)
        {
            var canonicalName = await manager.GetCanonicalNameAsync(tenant);
            if (string.IsNullOrWhiteSpace(canonicalName))
            {
                errors.Add(Describer.InvalidCanonicalName(canonicalName));
            }
            else if (!string.IsNullOrEmpty(manager.Options.Tenant.AllowedCanonicalNameCharacters) &&
                canonicalName.Any(c => !manager.Options.Tenant.AllowedCanonicalNameCharacters.Contains(c)))
            {
                errors.Add(Describer.InvalidCanonicalName(canonicalName));
            }
            else
            {
                var owner = await manager.FindByNameAsync(canonicalName);
                if (owner != null &&
                    !string.Equals(await manager.GetTenantIdAsync(owner), await manager.GetTenantIdAsync(tenant)))
                {
                    errors.Add(Describer.DuplicateCanonicalName(canonicalName));
                }
            }
        }
    }
}
