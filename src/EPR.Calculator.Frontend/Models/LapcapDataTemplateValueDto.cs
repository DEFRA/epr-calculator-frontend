using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class LapcapDataTemplateValueDto
    {
        public required string CountryName { get; set; }

        public required string Material { get; set; }

        public required string TotalCost { get; set; }
    }
}
