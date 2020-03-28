// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiTenancyServer.Hosting;
using MultiTenancyServer.Options;
using MultiTenancyServer.Stores;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Pipeline extension methods for adding multi-tenancy.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds the multi-tenancy system to the pipeline.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <returns>The builder.</returns>
        public static IApplicationBuilder UseMultiTenancy<TTenant>(this IApplicationBuilder app) where TTenant : class
        {
            return app
              .Validate<TTenant>()
              .UseMiddleware<TenancyMiddleware<TTenant>>();
        }

        internal static IApplicationBuilder Validate<TTenant>(this IApplicationBuilder app) where TTenant : class
        {
            var loggerFactory = app.ApplicationServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            var logger = loggerFactory.CreateLogger($"{nameof(MultiTenancyServer)}.Startup");

            var scopeFactory = app.ApplicationServices.GetService<IServiceScopeFactory>();

            using (var scope = scopeFactory.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                TestService(serviceProvider, typeof(ITenantStore<TTenant>), logger, 
                    "No storage mechanism for tenants specified. Use the 'AddInMemoryStore' extension method to register a development version.");

                var options = serviceProvider.GetRequiredService<TenancyOptions>();
                if (options == null)
                    throw new InvalidOperationException($"Options must be set.");
            }

            return app;
        }

        internal static object TestService(IServiceProvider serviceProvider, Type service, ILogger logger, string message = null, bool throwOnError = true)
        {
            var appService = serviceProvider.GetService(service);
            if (appService == null)
            {
                var error = message ?? $"Required service {service.FullName} is not registered in the DI container. Aborting startup.";
                logger.LogCritical(error);
                if (throwOnError)
                    throw new InvalidOperationException(error);
            }
            return appService;
        }
    }
}
