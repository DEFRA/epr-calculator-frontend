﻿using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class CalculationRunNameController : Controller
    {
        private const string CalculationRunNameIndexView = ViewNames.CalculationRunNameIndex;

        public IActionResult Index()
        {
            return this.View(CalculationRunNameIndexView);
        }

        [HttpPost]
        public IActionResult RunCalculator(InitiateCalculatorRunModel calculationRunModel)
        {
            if (!this.ModelState.IsValid)
            {
                var errorMessages = this.ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage);
                this.ViewBag.Errors = CreateErrorViewModel(errorMessages.First());
                return this.View(CalculationRunNameIndexView);
            }

            if (!string.IsNullOrEmpty(calculationRunModel.CalculationName))
            {
                this.HttpContext.Session.SetString(SessionConstants.CalculationName, calculationRunModel.CalculationName);
            }

            return this.RedirectToAction(ActionNames.RunCalculatorConfirmation);
        }

        public ViewResult Confirmation()
        {
            return this.View(ViewNames.CalculationRunConfirmation);
        }

        private static ErrorViewModel CreateErrorViewModel(string errorMessage)
        {
            return new ErrorViewModel
            {
                DOMElementId = ViewControlNames.CalculationRunName,
                ErrorMessage = errorMessage,
            };
        }
    }
}