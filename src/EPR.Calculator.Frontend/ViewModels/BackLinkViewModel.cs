namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// View model used to hold the data for the back link partial view.
    /// </summary>
    public class BackLinkViewModel
    {
        /// <summary>
        /// Gets or sets the back link URL.
        /// </summary>
        public required string BackLink { get; set; }

        /// <summary>
        /// Gets or sets the current user.
        /// </summary>
        public required string CurrentUser { get; set; }

        /// <summary>
        /// Gets or sets the run ID.
        /// </summary>
        public int? RunId { get; set; }
    }
}
