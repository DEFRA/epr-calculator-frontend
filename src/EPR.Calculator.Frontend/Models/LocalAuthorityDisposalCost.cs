using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class LocalAuthorityDisposalCost
    {
        public int Id { get; set; }

        [Required]
        public string ParameterYear { get; set; }

        [Required]
        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        [Required]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public int LapcapDataMasterId { get; set; }

        [Required]
        public string LapcapDataTemplateMasterUniqueRef { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string Material { get; set; }

        [Required]
        public decimal TotalCost { get; set; }
    }
}
