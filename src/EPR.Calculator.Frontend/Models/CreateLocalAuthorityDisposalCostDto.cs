using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class CreateLocalAuthorityDisposalCostDto
    {
        public required string ParameterYear { get; set; }

        public required IEnumerable<LocalAuthorityDisposalCostDto> DisposalCosts { get; set; }
    }
}
