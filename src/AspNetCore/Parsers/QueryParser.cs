// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MultiTenancyServer.Http.Parsers
{
    /// <summary>
    /// Tenant canonical name can be set via the query string of a request.
    /// </summary>
    public class QueryParser : RequestParser
    {
        /// <summary>
        /// The query string parameter name of the tenant canonical name.
        /// Eg: use "tenant" for matching on ?tenant=tenant1
        /// </summary>
        public string QueryName { get; set; }

        /// <summary>
        /// Retrieves the value of the query string parameter named <see cref="QueryName"/> from a request.
        /// </summary>
        /// <param name="httpContext">The request to retrieve the value of the query string parameter named <see cref="QueryName"/> from.</param>
        /// <returns>The value of the query string parameter named <see cref="QueryName"/>.</returns>
        public override Task<string> ParseRequestAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(httpContext.Request.Query.FirstOrDefault(h => string.Equals(h.Key, QueryName, StringComparison.OrdinalIgnoreCase)).Value.FirstOrDefault());
        }
    }
}
