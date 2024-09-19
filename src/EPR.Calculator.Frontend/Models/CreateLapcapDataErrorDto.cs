using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class CreateLapcapDataErrorDto
    {
        public string Message { get; set; }

        public string Description { get; set; }

        public string UniqueReference { get; set; }

        public string Country { get; set; }

        public string Material { get; set; }
    }
}
