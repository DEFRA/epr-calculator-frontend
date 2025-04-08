namespace EPR.Calculator.Frontend.Constants
{
    /// <summary>
    /// Contains constants related to the confirmation of parameter updates.
    /// </summary>
    public static class ParameterConfirmation
    {
        /// <summary>
        /// The title displayed when a parameter is successfully updated.
        /// </summary>
        public const string Title = "You have successfully updated";

        /// <summary>
        /// The body text indicating the default parameters.
        /// </summary>
        public const string Body = "The default parameters";

        /// <summary>
        /// The text displayed to the user indicating that all calculations will use the new parameters unless manually changed.
        /// </summary>
        public const string NextText = "All calculations will automatically use the new parameters, unless they are manually changed.";

        /// <summary>
        /// The controller to redirect to for viewing default parameters.
        /// </summary>
        public const string RedirectController = "DefaultParameters";

        /// <summary>
        /// The text for the submit button to view new default parameters.
        /// </summary>
        public const string SubmitText = "View new default parameters";
    }
}