namespace EPR.Calculator.Frontend.ViewModels
{
    public record BackLinkPartialViewModel
    {
        public string BackLink { get; set; } = null!;

        public string CurrentUser { get; set; } = null!;

        public int RunId { get; set; }
    }
}
