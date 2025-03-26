using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace EPR.Calculator.Frontend.Common
{
    public static class FeatureManagementService
    {
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
