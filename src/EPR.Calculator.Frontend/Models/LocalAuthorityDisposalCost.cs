using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class LocalAuthorityDisposalCost
    {
        public int Id { get; set; }

        public required string ProjectionYear { get; set; }

        public required DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public required string CreatedBy { get; set; }

        public required DateTime CreatedAt { get; set; }

        public required int LapcapDataMasterId { get; set; }

        public required string LapcapTempUniqueRef { get; set; }

        public required string Country { get; set; }

        public required string Material { get; set; }

        public required decimal TotalCost { get; set; }
    }
}
