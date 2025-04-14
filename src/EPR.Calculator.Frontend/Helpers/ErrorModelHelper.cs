using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EPR.Calculator.Frontend.Helpers
{
    /// <summary>
    /// Error Model Helper.
    /// </summary>
    public static class ErrorModelHelper
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

    public static class ModelStateHelper
    {
        public static List<ErrorViewModel> GetErrors(ModelStateDictionary modelState)
        {
            return modelState.Where(x => x.Value!.Errors.Count > 0)
                                                                    .SelectMany(x => x.Value!.Errors
                                                                        .Select(e => ErrorModelHelper.CreateErrorViewModel(
                                                                            $"{x.Key}-Error",
                                                                            e.ErrorMessage))
                                                                    ).ToList();

        }
    }
}
