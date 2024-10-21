using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class ConfirmationViewModel
    {
        public required string Title { get; set; }

        public string? SubmitText { get; set; }

        public required string NextText { get; set; }

        public string? RedirectController { get; set; }

        public required string Body { get; set; }
    }
}
