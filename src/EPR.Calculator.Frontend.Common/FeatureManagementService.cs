using EPR.Calculator.Frontend.Common.Constants;
using Microsoft.Extensions.Configuration;

namespace EPR.Calculator.Frontend.Common
{
    public static class FeatureManagementService
    {
        public static bool IsShowFinancialYearEnabled(IConfiguration configuration)
        {
            var showFinancialYearSetting = configuration.GetSection(ConfigSection.FeatureManagement).GetSection(ConfigSection.ShowFinancialYear);

            var result = bool.TryParse(showFinancialYearSetting.Value, out bool value);

            return result && value;
        }
    }
}
