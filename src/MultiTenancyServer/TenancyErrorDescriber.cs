// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace MultiTenancyServer
{
    /// <summary>
    /// Service to enable localization for application facing tenancy errors.
    /// </summary>
    /// <remarks>
    /// These errors are returned to controllers and are generally used as display messages to end users.
    /// </remarks>
    public class TenancyErrorDescriber
    {
        /// <summary>
        /// Returns the default <see cref="TenancyError"/>.
        /// </summary>
        /// <returns>The default <see cref="TenancyError"/>.</returns>
        public virtual TenancyError DefaultError()
        {
            return new TenancyError
            {
                Code = nameof(DefaultError),
                Description = Resources.DefaultError
            };
        }

        /// <summary>
        /// Returns an <see cref="TenancyError"/> indicating a concurrency failure.
        /// </summary>
        /// <returns>An <see cref="TenancyError"/> indicating a concurrency failure.</returns>
        public virtual TenancyError ConcurrencyFailure()
        {
            return new TenancyError
            {
                Code = nameof(ConcurrencyFailure),
                Description = Resources.ConcurrencyFailure
            };
        }

        /// <summary>
        /// Returns an <see cref="TenancyError"/> indicating the specified tenant <paramref name="canonicalName"/> is invalid.
        /// </summary>
        /// <param name="canonicalName">The canonical name that is invalid.</param>
        /// <returns>An <see cref="TenancyError"/> indicating the specified tenant <paramref name="canonicalName"/> is invalid.</returns>
        public virtual TenancyError InvalidCanonicalName(string canonicalName)
        {
            return new TenancyError
            {
                Code = nameof(InvalidCanonicalName),
                Description = string.Format(Resources.InvalidCanonicalNameFormat1, canonicalName)
            };
        }

        /// <summary>
        /// Returns an <see cref="TenancyError"/> indicating the specified <paramref name="canonicalName"/> already exists.
        /// </summary>
        /// <param name="canonicalName">The tenant canonical name that already exists.</param>
        /// <returns>An <see cref="TenancyError"/> indicating the specified <paramref name="canonicalName"/> already exists.</returns>
        public virtual TenancyError DuplicateCanonicalName(string canonicalName)
        {
            return new TenancyError
            {
                Code = nameof(DuplicateCanonicalName),
                Description = string.Format(Resources.DuplicateCanonicalNameFormat1, canonicalName)
            };
        }
    }
}
