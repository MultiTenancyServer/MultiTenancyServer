// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KodeAid;
using KodeAid.Text.Normalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using MultiTenancyServer.Http.Parsers;
using MultiTenancyServer.Stores;

namespace MultiTenancyServer.Http
{
    /// <summary>
    /// Tenancy provider for HTTP requests.
    /// </summary>
    public class HttpTenancyProvider<TTenant> : ITenancyProvider<TTenant> where TTenant : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpTenancyProvider"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">Context accessor of the current HTTP request.</param>
        /// <param name="requestParsers">The parsers.</param>
        /// <param name="tenantStore">The tenant store.</param>
        /// <param name="lookupNormalizer">The lookup normalizer for string comparison.</param>
        /// <param name="logger">The logger.</param>
        public HttpTenancyProvider(
            IHttpContextAccessor httpContextAccessor,
            IEnumerable<IRequestParser> requestParsers,
            ITenantStore<TTenant> tenantStore,
            ILookupNormalizer lookupNormalizer,
            ILogger<HttpTenancyProvider<TTenant>> logger)
        {
            ArgCheck.NotNull(nameof(httpContextAccessor), httpContextAccessor);
            ArgCheck.NotNull(nameof(requestParsers), requestParsers);
            ArgCheck.NotNull(nameof(tenantStore), tenantStore);
            ArgCheck.NotNull(nameof(lookupNormalizer), lookupNormalizer);
            ArgCheck.NotNull(nameof(logger), logger);

            _httpContextAccessor = httpContextAccessor;
            _requestParsers = requestParsers;
            _tenantStore = tenantStore;
            _lookupNormalizer = lookupNormalizer;
            _logger = logger;
        }

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEnumerable<IRequestParser> _requestParsers;
        private readonly ITenantStore<TTenant> _tenantStore;
        private readonly ILookupNormalizer _lookupNormalizer;
        private readonly ILogger _logger;
        private bool _tenantLoaded;
        private TTenant _tenant;

        /// <summary>
        /// Gets the tenant from the current HTTP request asynchronously.
        /// </summary>
        /// <returns>A task that when completed results in the tenant the request is for, otherwise null if undeterministic or not found.</returns>
        public async ValueTask<TTenant> GetCurrentTenantAsync(CancellationToken cancellationToken = default)
        {
            if (!_tenantLoaded)
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    _logger.LogError($"{nameof(IHttpContextAccessor)}.{nameof(IHttpContextAccessor.HttpContext)} is not available yet, it may be too early in the request pipeline to access it.");
                    return default;
                }

                foreach (var parser in _requestParsers)
                {
                    var canonicalName = await parser.ParseRequestAsync(httpContext, cancellationToken).ConfigureAwait(false);
                    if (canonicalName != null)
                    {
                        var normalizedCanonicalName = _lookupNormalizer.Normalize(canonicalName);
                        var tenant = await _tenantStore.FindByCanonicalNameAsync(normalizedCanonicalName, cancellationToken).ConfigureAwait(false);
                        if (tenant != null)
                        {
                            if (_logger.IsEnabled(LogLevel.Debug))
                            {
                                var tenantId = await _tenantStore.GetTenantIdAsync(tenant, cancellationToken).ConfigureAwait(false);
                                _logger.LogDebug("Tenant {TenantId} matched canonical name {CanonicalName}, parsed by {RequestParser} for request {RequestUrl}.",
                                    tenantId, canonicalName, parser.GetType().Name, httpContext.Request.GetDisplayUrl());
                            }
                            _tenant = tenant;
                            break;
                        }
                        else if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug("No tenant matched canonical name {CanonicalName}, parsed by {RequestParser} for request {RequestUrl}.",
                                canonicalName, parser.GetType().Name, httpContext.Request.GetDisplayUrl());
                        }
                    }
                    else if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Canonical name could not be parsed by {RequestParser} for request {RequestUrl}.",
                            parser.GetType().Name, httpContext.Request.GetDisplayUrl());
                    }
                }
                _tenantLoaded = true;
            }

            return _tenant;
        }
    }
}
