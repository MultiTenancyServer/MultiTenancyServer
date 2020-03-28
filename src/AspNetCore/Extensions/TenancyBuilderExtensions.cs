// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KodeAid;
using Microsoft.AspNetCore.Http;
using MultiTenancyServer.Configuration.DependencyInjection;
using MultiTenancyServer.Http.Parsers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TenancyBuilderExtensions
    {
        /// <summary>
        /// Helper functions for parsing the tenant from an HTTP request.
        /// </summary>
        /// <param name="builder">Builder to add the <see cref="IRequestParser"/> to.</param>
        /// <typeparam name="TTenant">The type representing a tenant.</typeparam>
        /// <typeparam name="TKey">The type of the primary key for a tenant.</typeparam>
        public static TenancyBuilder<TTenant, TKey> AddRequestParser<TTenant, TKey>(this TenancyBuilder<TTenant, TKey> builder, Func<IServiceProvider, IRequestParser> parserFactory)
            where TTenant : class
            where TKey : IEquatable<TKey>
        {
            ArgCheck.NotNull(nameof(builder), builder);
            builder.Services.AddScoped(parserFactory);
            return builder;
        }

        /// <summary>
        /// Adds a <see cref="DomainParser"/> to the collection of parsers for detecting the current tenant's canonical name by a custom domain name.
        /// </summary>
        /// <typeparam name="TTenant">The type representing a tenant.</typeparam>
        /// <typeparam name="TKey">The type of the primary key for a tenant.</typeparam>
        /// <param name="builder">Builder to add the <see cref="DomainParser"/> to.</param>
        /// <returns><paramref name="builder"/> for fluent API.</returns>
        public static TenancyBuilder<TTenant, TKey> AddDomainParser<TTenant, TKey>(this TenancyBuilder<TTenant, TKey> builder)
            where TTenant : class
            where TKey : IEquatable<TKey>
        {
            ArgCheck.NotNull(nameof(builder), builder);
            return builder.AddRequestParser(sp => new DomainParser());
        }

        /// <summary>
        /// Adds a <see cref="HeaderParser"/> to the collection of parsers for detecting the current tenant's canonical name by an HTTP header.
        /// Eg. use "X-TENANT" for matching on X-TENANT = tenant1
        /// </summary>
        /// <typeparam name="TTenant">The type representing a tenant.</typeparam>
        /// <typeparam name="TKey">The type of the primary key for a tenant.</typeparam>
        /// <param name="builder">Builder to add the <see cref="HeaderParser"/> to.</param>
        /// <param name="headerName">The HTTP header name which will contain the tenant's canonical name of the request.</param>
        /// <returns><paramref name="parsers"/> for fluent API.</returns>
        public static TenancyBuilder<TTenant, TKey> AddHeaderParser<TTenant, TKey>(this TenancyBuilder<TTenant, TKey> builder, string headerName)
            where TTenant : class
            where TKey : IEquatable<TKey>
        {
            ArgCheck.NotNull(nameof(builder), builder);
            ArgCheck.NotNullOrEmpty(nameof(headerName), headerName);
            return builder.AddRequestParser(sp => new HeaderParser() { HeaderName = headerName });
        }

        /// <summary>
        /// Adds a <see cref="HostParser"/> to the collection of parsers for detecting the current tenant's canonical name by a sub-domain host based on a parent domain.
        /// Eg: use ".tenants.multitenancyserver.io" to match on "tenant1.tenants.multitenancyserver.io"
        /// </summary>
        /// <typeparam name="TTenant">The type representing a tenant.</typeparam>
        /// <typeparam name="TKey">The type of the primary key for a tenant.</typeparam>
        /// <param name="builder">Builder to add the <see cref="HostParser"/> to.</param>
        /// <param name="parentHostSuffix">The parent hostname suffix which will contain the tenant's canonical name as its only sub-domain hostname of the request.</param>
        /// <returns><paramref name="parsers"/> for fluent API.</returns>
        public static TenancyBuilder<TTenant, TKey> AddSubdomainParser<TTenant, TKey>(this TenancyBuilder<TTenant, TKey> builder, string parentHostSuffix)
            where TTenant : class
            where TKey : IEquatable<TKey>
        {
            ArgCheck.NotNull(nameof(builder), builder);
            ArgCheck.NotNullOrEmpty(nameof(parentHostSuffix), parentHostSuffix);
            return builder.AddHostnameParser($@"^([a-z0-9-]+){Regex.Escape(parentHostSuffix).Replace(@"\*", @"[a-z0-9-]+")}$");
        }

        /// <summary>
        /// Adds a <see cref="HostParser"/> to the collection of parsers for detecting the current tenant's canonical name by using a regular expression on the request's hostname.
        /// Eg: use @"^([a-z0-9][a-z0-9-]*[a-z0-9])(?:\.[a-z][a-z])?\.tenants\.multitenancyserver\.io$" for 
        /// matching on tenant1.eu.tenants.multitenancyserver.io where '.eu.' is an optional and dynamic two letter region code.
        /// The first group capture of a successful match is used, use anonymouse groups (?:) to avoid unwanted captures.
        /// </summary>
        /// <typeparam name="TTenant">The type representing a tenant.</typeparam>
        /// <typeparam name="TKey">The type of the primary key for a tenant.</typeparam>
        /// <param name="builder">Builder to add the <see cref="HostParser"/> to.</param>
        /// <param name="hostPattern">A regular expression to retreive the tenant canonical name from the full hostname (domain) of the request.</param>
        /// <returns><paramref name="parsers"/> for fluent API.</returns>
        public static TenancyBuilder<TTenant, TKey> AddHostnameParser<TTenant, TKey>(this TenancyBuilder<TTenant, TKey> builder, string hostPattern)
            where TTenant : class
            where TKey : IEquatable<TKey>
        {
            ArgCheck.NotNull(nameof(builder), builder);
            ArgCheck.NotNullOrEmpty(nameof(hostPattern), hostPattern);
            return builder.AddRequestParser(sp => new HostParser() { HostPattern = hostPattern });
        }

        /// <summary>
        /// Adds a <see cref="PathParser"/> to the collection of parsers for detecting the current tenant's canonical name by a child path based on a parent path.
        /// Eg: use "/tenants/" for matching on multitenancyserver.io/tenants/tenant1
        /// </summary>
        /// <typeparam name="TTenant">The type representing a tenant.</typeparam>
        /// <typeparam name="TKey">The type of the primary key for a tenant.</typeparam>
        /// <param name="builder">Builder to add the <see cref="PathParser"/> to.</param>
        /// <param name="parentPathPrefix">The parent path prefix which will contain the tenant's canonical name as its child path segment of the request.</param>
        /// <returns><paramref name="parsers"/> for fluent API.</returns>
        public static TenancyBuilder<TTenant, TKey> AddChildPathParser<TTenant, TKey>(this TenancyBuilder<TTenant, TKey> builder, string parentPathPrefix)
            where TTenant : class
            where TKey : IEquatable<TKey>
        {
            ArgCheck.NotNull(nameof(builder), builder);
            ArgCheck.NotNullOrEmpty(nameof(parentPathPrefix), parentPathPrefix);
            return builder.AddPathParser($@"^{Regex.Escape(parentPathPrefix).Replace(@"\*", @"[a-z0-9-]+")}([a-z0-9._~!$&'()*+,;=:@%-]+)(?:$|[#/?].*$)");
        }

        /// <summary>
        /// Adds a <see cref="PathParser"/> to the collection of parsers for detecting the current tenant's canonical name by using a regular expression on the request's path.
        /// Eg: use "^/tenants/([a-z0-9]+)(?:[/]?)$" for matching on multitenancyserver.io/tenants/tenant1 or multitenancyserver.io/tenants/tenant1/
        /// </summary>
        /// <typeparam name="TTenant">The type representing a tenant.</typeparam>
        /// <typeparam name="TKey">The type of the primary key for a tenant.</typeparam>
        /// <param name="builder">Builder to add the <see cref="PathParser"/> to.</param>
        /// <param name="pathPattern">A regular expression to retreive the tenant canonical name from the path of the request.</param>
        /// <returns><paramref name="parsers"/> for fluent API.</returns>
        public static TenancyBuilder<TTenant, TKey> AddPathParser<TTenant, TKey>(this TenancyBuilder<TTenant, TKey> builder, string pathPattern)
            where TTenant : class
            where TKey : IEquatable<TKey>
        {
            ArgCheck.NotNull(nameof(builder), builder);
            ArgCheck.NotNullOrEmpty(nameof(pathPattern), pathPattern);
            return builder.AddRequestParser(sp => new PathParser() { PathPattern = pathPattern });
        }

        /// <summary>
        /// Adds a <see cref="QueryParser"/> to the collection of parsers for detecting the current tenant's canonical name by a query string parameter.
        /// Eg: use "tenant" for matching on ?tenant=tenant1
        /// </summary>
        /// <typeparam name="TTenant">The type representing a tenant.</typeparam>
        /// <typeparam name="TKey">The type of the primary key for a tenant.</typeparam>
        /// <param name="builder">Builder to add the <see cref="QueryParser"/> to.</param>
        /// <param name="queryName">The query string parameter name of the tenant canonical name.</param>
        /// <returns><paramref name="parsers"/> for fluent API.</returns>
        public static TenancyBuilder<TTenant, TKey> AddQueryParser<TTenant, TKey>(this TenancyBuilder<TTenant, TKey> builder, string queryName)
            where TTenant : class
            where TKey : IEquatable<TKey>
        {
            ArgCheck.NotNull(nameof(builder), builder);
            ArgCheck.NotNullOrEmpty(nameof(queryName), queryName);
            return builder.AddRequestParser(sp => new QueryParser() { QueryName = queryName });
        }

        /// <summary>
        /// Adds a <see cref="UserClaimParser"/> to the collection of parsers for detecting the current tenant's canonical name
        /// from a user claim on the authenticated user principal.
        /// Eg: claim type "http://schemas.microsoft.com/identity/claims/tenantid" or "tid".
        /// </summary>
        /// <typeparam name="TTenant">The type representing a tenant.</typeparam>
        /// <typeparam name="TKey">The type of the primary key for a tenant.</typeparam>
        /// <param name="builder">Builder to add the <see cref="UserClaimParser"/> to.</param>
        /// <param name="claimType">Claim type that contains the tenant canonical name as its value.</param>
        /// <returns><paramref name="parsers"/> for fluent API.</returns>
        public static TenancyBuilder<TTenant, TKey> AddClaimParser<TTenant, TKey>(this TenancyBuilder<TTenant, TKey> builder, string claimType)
            where TTenant : class
            where TKey : IEquatable<TKey>
        {
            ArgCheck.NotNull(nameof(builder), builder);
            ArgCheck.NotNullOrEmpty(nameof(claimType), claimType);
            return builder.AddRequestParser(sp => new UserClaimParser() { ClaimType = claimType });
        }

        /// <summary>
        /// Adds a <see cref="CustomParser"/> to the collection of parsers for detecting the current tenant's canonical name
        /// from a custom function.
        /// </summary>
        /// <typeparam name="TTenant">The type representing a tenant.</typeparam>
        /// <typeparam name="TKey">The type of the primary key for a tenant.</typeparam>
        /// <param name="builder">Builder to add the <see cref="CustomParser"/> to.</param>
        /// <param name="parser">Func that returns the tenant's canonical name from the current request.</param>
        /// <returns><paramref name="parsers"/> for fluent API.</returns>
        public static TenancyBuilder<TTenant, TKey> AddCustomParser<TTenant, TKey>(this TenancyBuilder<TTenant, TKey> builder, Func<HttpContext, string> parser)
            where TTenant : class
            where TKey : IEquatable<TKey>
        {
            return builder.AddCustomParser(httpContext => Task.FromResult(parser(httpContext)));
        }

        /// <summary>
        /// Adds a <see cref="CustomParser"/> to the collection of parsers for detecting the current tenant's canonical name
        /// from a custom function.
        /// </summary>
        /// <typeparam name="TTenant">The type representing a tenant.</typeparam>
        /// <typeparam name="TKey">The type of the primary key for a tenant.</typeparam>
        /// <param name="builder">Builder to add the <see cref="CustomParser"/> to.</param>
        /// <param name="parser">Async func that returns the tenant's canonical name from the current request.</param>
        /// <returns><paramref name="parsers"/> for fluent API.</returns>
        public static TenancyBuilder<TTenant, TKey> AddCustomParser<TTenant, TKey>(this TenancyBuilder<TTenant, TKey> builder, Func<HttpContext, Task<string>> parser)
            where TTenant : class
            where TKey : IEquatable<TKey>
        {
            ArgCheck.NotNull(nameof(builder), builder);
            ArgCheck.NotNull(nameof(parser), parser);
            return builder.AddRequestParser(sp => new CustomParser() { Parser = parser });
        }
    }
}
