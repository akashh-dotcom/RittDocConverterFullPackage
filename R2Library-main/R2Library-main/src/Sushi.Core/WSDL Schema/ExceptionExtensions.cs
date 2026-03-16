#region

using System;
using System.Collections.Generic;
using SystemException = System.Exception;

#endregion

namespace Sushi.Core
{
    /// <summary>
    ///     Extends the <see cref="System.Exception" /> class with Sushi-specific functionality.
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        ///     Flattens the hierarchical <see cref="System.Exception" /> instance into a collection of
        ///     <see cref="Exception" /> instances.
        /// </summary>
        /// <param name="exception">The <see cref="System.Exception" /> to convert.</param>
        /// <returns>A <see cref="IEnumerable{Exception}" /> collection.</returns>
        public static IEnumerable<Exception> ToSushiExceptions(this SystemException exception)
        {
            var exceptions = new List<Exception>
            {
                new Exception
                {
                    Created = DateTime.Now,
                    CreatedSpecified = true,
                    Message = exception.Message,
                    HelpUrl = exception.HelpLink,
                    Severity = ExceptionSeverity.Error,
                    Data = exception.StackTrace
                }
            };

            if (exception.InnerException != null)
            {
                exceptions.AddRange(exception.InnerException.ToSushiExceptions());
            }

            return exceptions;
        }
    }
}