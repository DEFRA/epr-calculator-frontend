﻿using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class CreateCalculatorRunDto
    {
        public required string CalculatorRunName { get; set; }

        public required string FinancialYear { get; set; }

        public required string CreatedBy { get; set; }
    }
}