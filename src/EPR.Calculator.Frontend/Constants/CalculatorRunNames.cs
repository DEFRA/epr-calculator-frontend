namespace EPR.Calculator.Frontend.Constants
{
    /// <summary>
    /// Contains constants related to the naming of calculator runs.
    /// </summary>
    public static class CalculatorRunNames
    {
        /// <summary>
        /// The title for the when a new calculation is started.
        /// </summary>
        public const string Title = "You have started a calculation";

        /// <summary>
        /// The text displayed to the user indicating the time required for the calculation to complete.
        /// </summary>
        public static readonly List<string> AdditionalParagraphs = new List<string>
        {
            "It will take up to 15 minutes for the calculation to finish. You will be able to view it in the list of calculations on the dashboard.",
        };
    }
}