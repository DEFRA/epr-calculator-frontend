using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class CreateLocalAuthorityDisposalCostDto
    {
        public string ParameterYear { get; set; }

        public IEnumerable<LocalAuthorityDisposalCostDto> DisposalCosts { get; set; }
    }
}
