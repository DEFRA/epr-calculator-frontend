using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class DefaultSchemeParameters
    {
        public int Id { get; set; }

        public required RelativeYear RelativeYear { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public required string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public int DefaultParameterSettingMasterId { get; set; }

        public required string ParameterUniqueRef { get; set; }

        public required string ParameterType { get; set; }

        public required string ParameterCategory { get; set; }

        public required string ParameterValue { get; set; }

        public decimal ParameterDecimalValue()
        {
            if (decimal.TryParse(ParameterValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            throw new FormatException($"Parameter '{ParameterUniqueRef}' with value '{ParameterValue}' is not a valid decimal.");
        }
    }
}
