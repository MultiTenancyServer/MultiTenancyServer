// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using KodeAid;
using Microsoft.Extensions.Logging;

namespace MultiTenancyServer
{
    public static class TenancyAccessHelper
    {
        public static void CheckTenancyAccess<TKey>(TKey currentTenantId, TKey accessedTenantId, ILogger logger = null)
          where TKey : IEquatable<TKey>
        {
            ArgCheck.NotNull(nameof(currentTenantId), currentTenantId);
            ArgCheck.NotNull(nameof(accessedTenantId), accessedTenantId);
            if (!currentTenantId.Equals(accessedTenantId))
            {
                logger?.LogError($"Cross tenancy access detected from {{CurrentTenant}} to {{AccessedTenant}}.", currentTenantId, accessedTenantId);
                throw new CrossTenancyAccessViolationException();
            }
        }

        public static void CheckTenancyAccess(object currentTenantId, object accessedTenantId, ILogger logger = null)
        {
            ArgCheck.NotNull(nameof(currentTenantId), currentTenantId);
            ArgCheck.NotNull(nameof(accessedTenantId), accessedTenantId);
            if (!currentTenantId.Equals(accessedTenantId))
            {
                logger?.LogError($"Cross tenancy access detected from {{CurrentTenant}} to {{AccessedTenant}}.", currentTenantId, accessedTenantId);
                throw new CrossTenancyAccessViolationException();
            }
        }
    }
}
