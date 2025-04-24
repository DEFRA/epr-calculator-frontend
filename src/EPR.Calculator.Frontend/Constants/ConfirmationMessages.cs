namespace EPR.Calculator.Frontend.Constants
{
    /// <summary>
    /// List of confirmation messages.
    /// </summary>
    public static class ConfirmationMessages
    {
        /// <summary>
        /// Title for confirmation partial.
        /// </summary>
        public const string BillingFileSuccessTitle = "Billing file sent";

        /// <summary>
        /// Body text for confirmation partial.
        /// </summary>
        public const string BillingFileSuccessBody = "You have successfully sent the billing file to the FSS.";

        /// <summary>
        /// Additional text paragraphs for confirmation partial.
        /// </summary>
        public static readonly List<string> BillingFileSuccessAdditionalParagraphs =
        [
            "The FSS will process the billing file based on the billing instructions you accepted.",
                "You can return to the dashboard to review calculation runs or start a new run."
        ];
    }
}