// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace MultiTenancyServer.Options
{
    /// <summary>
    /// Options for tenant validation.
    /// </summary>
    public class TenantOptions
    {
        /// <summary>
        /// Gets or sets the list of allowed characters in the canonical name used to validate tenant canonical names.
        /// Defaults to abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+
        /// </summary>
        /// <value>
        /// The list of allowed characters in the canonical name used to validate tenant canonical names.
        /// </value>
        public string AllowedCanonicalNameCharacters { get; set; } = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-.";
    }
}
