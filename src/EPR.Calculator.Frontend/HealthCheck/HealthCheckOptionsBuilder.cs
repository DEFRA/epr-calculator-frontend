using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.HealthCheck
{
    /// <summary>
        /// Provides a method to build <see cref="HealthCheckOptions"/> with predefined settings.
        /// </summary>
    [ExcludeFromCodeCoverage]
    public class HealthCheckOptionsBuilder
    {
        /// <summary>
                /// Builds and returns a <see cref="HealthCheckOptions"/> instance with predefined settings.
                /// </summary>
                /// <returns>
                /// A <see cref="HealthCheckOptions"/> instance with <see cref="HealthCheckOptions.AllowCachingResponses"/> set to <c>false</c>
                /// and <see cref="HealthCheckOptions.ResultStatusCodes"/> configured to return <see cref="StatusCodes.Status200OK"/> for <see cref="HealthStatus.Healthy"/>.
                /// </returns>
        public static HealthCheckOptions Build()
        {
            return new HealthCheckOptions
            {
                AllowCachingResponses = false,
                ResultStatusCodes =
                {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                },
            };
        }
    }
}