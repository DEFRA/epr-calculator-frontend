using EPR.Calculator.Frontend.Common.Constants;
using Microsoft.Extensions.Configuration;

namespace EPR.Calculator.Frontend.Common
{
    public static class FeatureManagementService
    {
        public static bool IsShowFinancialYearEnabled(IConfiguration configuration)
        {
            var featureManagementSetting = configuration.GetSection(ConfigSection.FeatureManagement);
            if (featureManagementSetting == null)
            {
                throw new ArgumentNullException(nameof(featureManagementSetting));
            }

            var showFinancialYearSetting = configuration.GetSection(ConfigSection.FeatureManagement).GetSection(ConfigSection.ShowFinancialYear);
            if (showFinancialYearSetting == null)
            {
                throw new ArgumentNullException(nameof(showFinancialYearSetting));
            }

            return bool.TryParse(showFinancialYearSetting.Value, out _);
        }
    }
}
