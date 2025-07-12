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

        public const string BillingStatus = "billingStatus";

        public const string BillingStatusBanner = "You’ve accepted {0} instructions and rejected {1}. <br>{0} instructions are still pending.";
    }
}
