namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// Represents the status and display information for various classification runs.
    /// </summary>
    public class ImportantSectionViewModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether indicates whether any classification run is currently in progress.
        /// </summary>
        public bool IsAnyRunInProgress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether indicates whether any designated classification run has occurred.
        /// </summary>
        public bool HasAnyDesigRun { get; set; }

        /// <summary>
        /// Gets or sets the ID of the classification run that is currently in progress.
        /// </summary>
        public int RunIdInProgress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether indicates whether the initial run should be displayed.
        /// </summary>
        public bool IsDisplayInitialRun { get; set; }

        /// <summary>
        /// Gets or sets message associated with the display of the initial run.
        /// </summary>
        public string? IsDisplayInitialRunMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether indicates whether the interim run should be displayed.
        /// </summary>
        public bool IsDisplayInterimRun { get; set; }

        /// <summary>
        /// Gets or sets message associated with the display of the interim run.
        /// </summary>
        public string? IsDisplayInterimRunMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether indicates whether the final re-calculation run should be displayed.
        /// </summary>
        public bool IsDisplayFinalRecallRun { get; set; }

        /// <summary>
        /// Gets or sets message associated with the display of the final re-calculation run.
        /// </summary>
        public string? IsDisplayFinalRecallRunMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether indicates whether the final run should be displayed.
        /// </summary>
        public bool IsDisplayFinalRun { get; set; }

        /// <summary>
        /// Gets or sets message associated with the display of the final run.
        /// </summary>
        public string? IsDisplayFinalRunMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether indicates whether the final run should be displayed.
        /// </summary>
        public bool IsDisplayTestRun { get; set; }
    }
}