using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class LocalAuthorityDisposalCost
    {
        public int Id { get; set; }

        public string ParameterYear { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public int LapcapDataMasterId { get; set; }

        public string LapcapDataTemplateMasterUniqueRef { get; set; }

        public string Country { get; set; }

        public string Material { get; set; }

        public decimal TotalCost { get; set; }
    }
}
