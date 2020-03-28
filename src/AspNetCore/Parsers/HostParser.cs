// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MultiTenancyServer.Http.Parsers
{
    /// <summary>
    /// Tenant canonical name can be set via a hostname 
    /// within the full domain name of a request.
    /// </summary>
    public class HostParser : RegexRequestParser
    {
        /// <summary>
        /// A regular expression to retreive the tenant canonical name from the full hostname (domain) of the request.
        /// Eg: use @"^([a-z0-9][a-z0-9-]*[a-z0-9])(?:\.[a-z][a-z])?\.tenants\.multitenancyserver\.io$" for 
        /// matching on tenant1.eu.tenants.multitenancyserver.io where '.eu.' is an optional and dynamic two letter region code.
        /// The first group capture of a successful match is used, use anonymouse groups (?:) to avoid unwanted captures.
        /// </summary>
        public string HostPattern { get; set; }

        /// <summary>
        /// Retrieves a segment of the hostname from a request matching the regular expression pattern <see cref="HostPattern"/>.
        /// </summary>
        /// <param name="httpContext">The request to retrieve the segment of the hostname from.</param>
        /// <returns>The matched segment of the hostname from the request.</returns>
        public override Task<string> ParseRequestAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(FindMatch(httpContext.Request.Host.Host, HostPattern));
        }
    }
}
