using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.Helpers
{
    /// <summary>
    /// Error Model Helper.
    /// </summary>
    public class ErrorModelHelper
    {
        /// <summary>
        /// Create Error View Model.
        /// </summary>
        /// <param name="domElementId">dom element id.</param>
        /// <param name="errorMessage">error message.</param>
        /// <returns>Error View Model.</returns>
        public static ErrorViewModel CreateErrorViewModel(string domElementId, string errorMessage)
        {
            return new ErrorViewModel
            {
                DOMElementId = domElementId,
                ErrorMessage = errorMessage,
            };
        }
    }
}
