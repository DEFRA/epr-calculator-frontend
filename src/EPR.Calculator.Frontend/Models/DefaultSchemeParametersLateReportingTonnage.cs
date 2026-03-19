using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class DefaultSchemeParametersLateReportingTonnage
    {
        public required string Material { get; set; }
        public required decimal Red { get; set; }
        public required decimal Amber { get; set; }
        public required decimal Green { get; set; }
    }
}
