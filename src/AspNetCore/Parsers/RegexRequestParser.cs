// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Text.RegularExpressions;

namespace MultiTenancyServer.Http.Parsers
{
    public abstract class RegexRequestParser : RequestParser
    {
        /// <summary>
        /// Returns the first capture of the inner most group of a successful regular expression match.
        /// </summary>
        /// <param name="input">The input string to search.</param>
        /// <param name="pattern">The regular expression to match on.</param>
        /// <returns>The first capture of the inner most group of a successful regular expression match or null.</returns>
        protected string FindMatch(string input, string pattern)
        {
            return Regex.Match(input, pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase).GetFirstInnerMostGroupCapture();
        }
    }
}
