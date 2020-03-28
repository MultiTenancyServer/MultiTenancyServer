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
    /// Tenant canonical name can be set via an HTTP request header
    /// within the headers collection of a request.
    /// </summary>
    public class HeaderParser : RequestParser
    {
        /// <summary>
        /// The header name of the tenant canonical name.
        /// Eg. use "X-TENANT" for matching on X-TENANT = tenant1
        /// </summary>
        public string HeaderName { get; set; }

        /// <summary>
        /// Retrieves the value of the HTTP header named <see cref="HeaderName"/> from a request.
        /// </summary>
        /// <param name="httpContext">The request to retrieve the value of the HTTP header named <see cref="HeaderName"/> from.</param>
        /// <returns>The value of the HTTP header named <see cref="HeaderName"/>.</returns>
        public override Task<string> ParseRequestAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(httpContext.Request.Headers.FirstOrDefault(h => string.Equals(h.Key, HeaderName, StringComparison.OrdinalIgnoreCase)).Value.FirstOrDefault());
        }
    }
}
