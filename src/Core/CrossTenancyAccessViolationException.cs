// Copyright (c) Kris Penner. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Runtime.Serialization;

namespace MultiTenancyServer
{
    public class CrossTenancyAccessViolationException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the System.InvalidOperationException class.
        /// </summary>
        public CrossTenancyAccessViolationException()
          : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the System.InvalidOperationException class with
        /// a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CrossTenancyAccessViolationException(string message)
          : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the System.InvalidOperationException class with
        /// a specified error message and a reference to the inner exception that is the
        /// cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the innerException
        /// parameter is not a null reference (Nothing in Visual Basic), the current exception
        /// is raised in a catch block that handles the inner exception.</param>
        public CrossTenancyAccessViolationException(string message, Exception innerException)
          : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the System.InvalidOperationException class with
        /// serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected CrossTenancyAccessViolationException(SerializationInfo info, StreamingContext context)
          : base()
        {
        }
    }
}
