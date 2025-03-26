namespace EPR.Calculator.Frontend.Common
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.FeatureManagement;

    [ExcludeFromCodeCoverage]
    public static class FeatureManagementService
    {
        /// <summary>
        /// Returns the state of the feature.
        /// This function can be used in the code to determine a feature's state where FeatureGate does not work.
        /// </summary>
        /// <param name="configuration">Feature flag configuration item.</param>
        /// <param name="feature">Name of the feature.</param>
        /// <returns>State of the feature flag.</returns>
        public static bool IsFeatureEnabled(this IConfiguration configuration, string feature)
        {
            var featureServices = new ServiceCollection();
            featureServices.AddFeatureManagement(configuration);
            using var provider = featureServices.BuildServiceProvider();
            var manager = provider.GetRequiredService<IFeatureManager>();

            return manager.IsEnabledAsync(feature).GetAwaiter().GetResult();
        }
    }
}
