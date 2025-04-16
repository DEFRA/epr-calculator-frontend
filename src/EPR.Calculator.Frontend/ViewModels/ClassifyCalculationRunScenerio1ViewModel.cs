﻿using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// classify calculation run scenerio1 view model.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record ClassifyCalculationRunScenerio1ViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets or sets the data for the calculator run status update.
        /// </summary>
        public CalculatorRunStatusUpdateDto? CalculatorRunStatus { get; set; }

        [Required(ErrorMessage = ErrorMessages.ClassifyRunTypeNotSelected)]
        public ClassifyRunType? ClassifyRunType { get; set; }
    }
}