using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.Extensions
{
    public static class ErrorViewModelExtension
    {
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

            // Check if any error matches the DOM element ID
            return errors.Any(e => e.DOMElementId == domElementId);
        }
    }
}
