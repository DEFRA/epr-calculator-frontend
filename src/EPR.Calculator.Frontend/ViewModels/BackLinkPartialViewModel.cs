using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    [ExcludeFromCodeCoverage(Justification= "POCO")]
    public class BackLinkPartialViewModel
    {
        public string BackLink { get; set; }

        public string CurrentUser { get; set; }

        public int RunId { get; set; }

        public bool HideBackLink { get; set; }
    }
}
