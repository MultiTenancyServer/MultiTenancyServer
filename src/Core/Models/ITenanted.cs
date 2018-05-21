// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;

namespace MultiTenancyServer.Models
{
    /// <summary>
    /// A entity that is owned by a tenant.
    /// </summary>
    public interface ITenanted<TKey>
      where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Gets the ID of the tenant that owns this entity.
        /// </summary>
        /// <value>The ID of the tenant that owns this entity.</value>
        TKey TenantId { get; set; }
    }
}
