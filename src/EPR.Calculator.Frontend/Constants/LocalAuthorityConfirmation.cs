namespace EPR.Calculator.Frontend.Constants
{
    public static class LocalAuthorityConfirmation
    {
        /// <summary>
        /// The title displayed when a parameter is successfully updated.
        /// </summary>
        public const string Title = "You have successfully updated";

        /// <summary>
        /// The body text indicating the default parameters.
        /// </summary>
        public const string Body = "The local authority disposal costs";

        /// <summary>
        /// The controller to redirect to for viewing default parameters.
        /// </summary>
        public const string RedirectController = "LocalAuthorityDisposalCosts";

        /// <summary>
        /// The text for the submit button to view local authority disposal costs.
        /// </summary>
        public const string SubmitText = "View local authority disposal costs";

        /// <summary>
        /// The text displayed to the user indicating that all calculations will use the new local authority disposal costs unless manually changed.
        /// </summary>
        public static readonly List<string> AdditionalParagraphs = new List<string>
        {
            "All calculations will automatically use the latest local authority disposal costs.",
        };
    }
}