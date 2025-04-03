namespace EPR.Calculator.Frontend.Constants
{
    /// <summary>
    /// List of confirmation messages.
    /// </summary>
    public static class ConfirmationMessages
    {
        /// <summary>
        /// Confirmation text for Billing File Success page
        /// </summary>
        public static class BillingFileSuccess
        {
            /// <summary>
            /// Title for confirmation partial.
            /// </summary>
            public const string Title = "Billing file sent";

            /// <summary>
            /// Body text for confirmation partial.
            /// </summary>
            public const string Body = "You have successfully sent the billing file to the FSS.";

            /// <summary>
            /// Redirect controller for confirmation partial.
            /// </summary>
            public const string RedirectController = "Dashboard";

            /// <summary>
            /// Additional text paragraphs for confirmation partial.
            /// </summary>
            public static readonly List<string> NextText =
            [
                "The FSS will process the billing file based on the invoice instructions you accepted.",
                "You can return to the dashboard to review calculation runs or start a new run."
            ];
        }
    }
}