using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EPR.Calculator.Frontend.Helpers
{
    /// <summary>
    /// Error Model Helper.
    /// </summary>
    public static class ErrorModelHelper
    {
        private const string DomErrorSuffix = "Error";

        public static List<ErrorViewModel> GetErrors(ModelStateDictionary modelState)
        {
            return modelState.Where(x => x.Value!.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors
                .Select(e => ErrorModelHelper.CreateErrorViewModel(
                    $"{x.Key}-{DomErrorSuffix}", e.ErrorMessage)))
                .ToList();
        }

        /// <summary>
        /// Checks if the list of errors contains an error for a specific DOM element ID.
        /// </summary>
        /// <param name="errors">The list of errors.</param>
        /// <param name="domElementId">The DOM element ID to check for errors.</param>
        /// <returns>True if the error exists, otherwise false.</returns>
        public static bool HasError(this List<ErrorViewModel> errors, string domElementId)
        {
            if (errors == null || string.IsNullOrWhiteSpace(domElementId))
            {
                return false;
            }

            var domElementMatchId = domElementId + "-" + DomErrorSuffix;

            // Check if any error matches the DOM element ID
            return errors.Any(e => e.DOMElementId == domElementMatchId);
        }

        /// <summary>
        /// Create Error View Model.
        /// </summary>
        /// <param name="domElementId">dom element id.</param>
        /// <param name="errorMessage">error message.</param>
        /// <returns>Error View Model.</returns>
        private static ErrorViewModel CreateErrorViewModel(string domElementId, string errorMessage)
        {
            return new ErrorViewModel
            {
                DOMElementId = domElementId,
                ErrorMessage = errorMessage,
            };
        }
    }
}