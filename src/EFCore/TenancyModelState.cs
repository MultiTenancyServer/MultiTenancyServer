// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using MultiTenancyServer.Options;

namespace MultiTenancyServer.EntityFramework
{
    /// <summary>
    /// Contains tenancy state information of the EF model.
    /// </summary>
    public class TenancyModelState<TKey>
    {
        internal TKey UnsetTenantKey { get; set; }
        internal TenantReferenceOptions DefaultOptions { get; set; }
        internal Dictionary<Type, TenantReferenceOptions> Properties { get; } = new Dictionary<Type, TenantReferenceOptions>();
    }
}
