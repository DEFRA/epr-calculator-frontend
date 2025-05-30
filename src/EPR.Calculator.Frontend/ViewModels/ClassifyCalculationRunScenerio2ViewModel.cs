﻿using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// classify calculation run scenerio1 view model.
    /// </summary>
    public record ClassifyCalculationRunScenerio2ViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets or sets the errors.
        /// </summary>
        public ErrorViewModel? Errors { get; set; }

        /// <summary>
        /// Gets the data for the run status update.
        /// </summary>
        public required CalculatorRunStatusUpdateDto CalculatorRunStatus { get; init; }

        /// <summary>
        /// Gets or sets the selected calculation run option.
        /// </summary>
        public string? SelectedCalcRunOption { get; set; }
    }
}