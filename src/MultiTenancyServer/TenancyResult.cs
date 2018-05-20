// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace MultiTenancyServer
{
    /// <summary>
    /// Represents the result of an tenancy operation.
    /// </summary>
    public class TenancyResult
    {
        private List<TenancyError> _errors = new List<TenancyError>();

        /// <summary>
        /// Flag indicating whether if the operation succeeded or not.
        /// </summary>
        /// <value>True if the operation succeeded, otherwise false.</value>
        public bool Succeeded { get; protected set; }

        /// <summary>
        /// An <see cref="IEnumerable{T}"/> of <see cref="TenancyError"/>s containing an errors
        /// that occurred during the tenancy operation.
        /// </summary>
        /// <value>An <see cref="IEnumerable{T}"/> of <see cref="TenancyError"/>s.</value>
        public IEnumerable<TenancyError> Errors => _errors;

        /// <summary>
        /// Returns an <see cref="TenancyResult"/> indicating a successful tenancy operation.
        /// </summary>
        /// <returns>An <see cref="TenancyResult"/> indicating a successful operation.</returns>
        public static TenancyResult Success { get; } = new TenancyResult { Succeeded = true };

        /// <summary>
        /// Creates an <see cref="TenancyResult"/> indicating a failed tenancy operation, with a list of <paramref name="errors"/> if applicable.
        /// </summary>
        /// <param name="errors">An optional array of <see cref="TenancyError"/>s which caused the operation to fail.</param>
        /// <returns>An <see cref="TenancyResult"/> indicating a failed tenancy operation, with a list of <paramref name="errors"/> if applicable.</returns>
        public static TenancyResult Failed(params TenancyError[] errors)
        {
            var result = new TenancyResult { Succeeded = false };
            if (errors != null)
            {
                result._errors.AddRange(errors);
            }
            return result;
        }

        /// <summary>
        /// Converts the value of the current <see cref="TenancyResult"/> object to its equivalent string representation.
        /// </summary>
        /// <returns>A string representation of the current <see cref="TenancyResult"/> object.</returns>
        /// <remarks>
        /// If the operation was successful the ToString() will return "Succeeded" otherwise it returned 
        /// "Failed : " followed by a comma delimited list of error codes from its <see cref="Errors"/> collection, if any.
        /// </remarks>
        public override string ToString()
        {
            return Succeeded ?
                   "Succeeded" :
                   string.Format("{0} : {1}", "Failed", string.Join(",", Errors.Select(x => x.Code).ToList()));
        }
    }
}
