using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    [ExcludeFromCodeCoverage]
    public record BackLinkPartialViewModel
    {
        public string BackLink { get; set; } = null!;

        public string CurrentUser { get; set; } = null!;

        public int RunId { get; set; }
    }
}
