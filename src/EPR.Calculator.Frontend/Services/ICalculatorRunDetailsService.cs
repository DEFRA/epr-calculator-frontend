using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.Services
{
    /// <summary>
    /// Interface for the calculator run details service.
    /// </summary>
    public interface ICalculatorRunDetailsService
    {
        /// <summary>
        /// Gets the details of a calculator run by its ID.
        /// </summary>
        /// <param name="runId">The run ID to fetch the details for.</param>
        /// <returns>A <see cref="Task<CalculatorRunDetailsViewModel>"/>.</returns>
        Task<CalculatorRunDetailsViewModel> GetCalculatorRundetailsAsync(HttpContext httpContext, int runId);
    }
}
