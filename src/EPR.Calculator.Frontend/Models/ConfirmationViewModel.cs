using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class ConfirmationViewModel
    {
        public string Title { get; set; }

        public string SubmitText { get; set; }

        public string NextText { get; set; }

        public string RedirectController { get; set; }

        public string Body { get; set; }
    }
}
