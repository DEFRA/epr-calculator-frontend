using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class LocalAuthorityDisposalCostDto
    {
        public required string LapcapDataTemplateMasterUniqueRef { get; set; }

        public required string TotalCost { get; set; }
    }
}
