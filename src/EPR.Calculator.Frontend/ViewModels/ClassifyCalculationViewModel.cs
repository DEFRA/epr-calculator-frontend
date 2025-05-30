﻿namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// Represents the view model for classifying calculations.
    /// </summary>
    /// <param name="CalcName">The name of the calculation.</param>
    /// <param name="FinancialYear">The financial year of the calculation.</param>
    /// <param name="CalculationId">The unique identifier for the calculation.</param>
    /// <param name="ClassificationType">The type of classification.</param>
    /// <param name="RunDate">The date the calculation was run.</param>
    public record ClassifyCalculationViewModel(int RunId, string CalcName, string FinancialYear, int CalculationId, bool ClassificationType, string RunDate) : ViewModelCommonData;
}