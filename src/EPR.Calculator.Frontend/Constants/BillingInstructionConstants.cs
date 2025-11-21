namespace EPR.Calculator.Frontend.Constants
{
    /// <summary>
    /// Contains constant values used throughout the Billing Instructions module.
    /// </summary>
    public static class BillingInstructionConstants
    {
        /// <summary>
        /// The key used to store or retrieve the calculation run ID from route data, query strings, or other data structures.
        /// </summary>
        public const string CalculationRunIdKey = "calculationRunId";

        public const string OrganisationIdKey = "organisationId";

        public const string AmendBillingInstructions = "Amend billing instructions";

        public const string AmendBillingInstructionText = "Review or change the billing instructions before sending the billing file.";

        public const string BillingStatus = "billingStatus";

        public const string BillingInstruction = "billingInstruction";

        public const string BillingStatusBanner = "You’ve accepted {0} instructions and rejected {1}. <br>{2} instructions are still pending.";

        public const string BillingNextStep = "Next step";

        public const string BillingNextStepText = "If you’re happy with all billing instructions, you’ll be able to generate a draft billing file. You can return to this screen to make changes before the billing file is sent to the FSS.";

        public const string GenerateDraftBillingFile = "Generate draft billing file";

        public const string DraftBillingFile = "Draft billing file";

        public const string DraftBillingFileGenerating = "Draft billing file generating";

        public const string GenerateNewDraftBillingFile = "Generate new draft billing file";

        public const string GenerateFileMessage = "This can take up to 15 minutes.";

        public const string BillingFileOutofDateBanner = "The billing instructions have changed, so the draft billing file is now out of date. You must first {0} before it can be sent to the FSS.";

        public const string GenerateBillingFile = "generate a new draft billing file";

        public const string NoSuggestedBillingInstructionPlaceholder = "-";
    }
}
