namespace EPR.Calculator.Frontend.Constants
{
    /// <summary>
    /// Contains constant values used throughout the Billing Instructions module.
    /// </summary>
    public static class BillingInstructionConstants
    {
        /// <summary>
        /// The route name for the Billing Instructions index page.
        /// Used for URL generation and routing.
        /// </summary>
        public const string BillingInstructionsIndexRouteName = "BillingInstructions_Index";

        /// <summary>
        /// The key used to store or retrieve the calculation run ID from route data, query strings, or other data structures.
        /// </summary>
        public const string CalculationRunIdKey = "calculationRunId";

        public const string OrganisationIdKey = "organisationId";
    }
}
