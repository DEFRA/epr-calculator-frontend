﻿using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// View model for displaying billing instructions, including calculation run details and paginated records.
    /// </summary>
    public record BillingInstructionsViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets the calculation run details for the billing instructions.
        /// </summary>
        public CalculationRunForBillingInstructionsDto CalculationRun { get; init; } = new();

        public ICollection<Organisation> OrganisationBillingInstructions { get; init; } = [];

        /// <summary>
        /// Gets the pagination model containing the billing instruction records and pagination information.
        /// </summary>
        public PaginationViewModel TablePaginationModel { get; init; } = new();

        public OrganisationSelectionsViewModel OrganisationSelections { get; set; } = new();

        public IEnumerable<int>? ProducerIds { get; init; }

        public int TotalRecords { get; set; }

        public int TotalAcceptedRecords { get; set; }

        public int TotalRejectedRecords { get; set; }

        public int TotalPendingRecords { get; set; }
    }
}