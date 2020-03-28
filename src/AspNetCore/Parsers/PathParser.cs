// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MultiTenancyServer.Http.Parsers
{
    /// <summary>
    /// Tenant canonical name can be set via a path segment 
    /// within the URL path of a request.
    /// </summary>
    public class PathParser : RegexRequestParser
    {
        /// <summary>
        /// A regular expression to retreive the tenant canonical name from the full URL path of the request.
        /// Eg: use "^/tenants/([a-z0-9]+)(?:[/]?)$" for matching on multitenancyserver.io/tenants/tenant1 or multitenancyserver.io/tenants/tenant1/
        /// The first group capture of a successful match is used, use anonymouse groups (?:) to avoid unwanted captures.
        /// </summary>
        public string PathPattern { get; set; }

        /// <summary>
        /// Retrieves a segment of the path from a request matching the regular expression pattern <see cref="PathPattern"/>.
        /// </summary>
        /// <param name="httpContext">The request to retrieve the segment of the path from.</param>
        /// <returns>The matched segment of the path from the request.</returns>
        public override Task<string> ParseRequestAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(FindMatch(httpContext.Request.Path, PathPattern));
        }
    }
}
