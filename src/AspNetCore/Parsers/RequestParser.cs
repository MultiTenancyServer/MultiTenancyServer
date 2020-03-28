// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MultiTenancyServer.Http.Parsers
{
    public abstract class RequestParser : IRequestParser
    {
        /// <summary>
        /// Retrieves a value from a request.
        /// </summary>
        /// <param name="httpContext">The request to retrieve the value from.</param>
        /// <returns>The parsed/matched value.</returns>
        public abstract Task<string> ParseRequestAsync(HttpContext httpContext, CancellationToken cancellationToken = default);
    }
}
