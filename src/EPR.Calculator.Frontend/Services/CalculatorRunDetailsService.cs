using EPR.Calculator.Frontend.ViewModels;
using System.Net;

namespace EPR.Calculator.Frontend.Services
{
    public class CalculatorRunDetailsService(
        IEprCalculatorApiService eprCalculatorApiService) : ICalculatorRunDetailsService
    {
        /// <inheritdoc />
        public async Task<CalculatorRunDetailsViewModel> GetCalculatorRundetailsAsync(
            HttpContext httpContext,
            int runId)
        {
            var response = await eprCalculatorApiService.CallApi(
                httpContext: httpContext,
                httpMethod: HttpMethod.Get,
                relativePath: $"v1/calculatorRuns/{runId}");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException(
                    $"Failed to retrieve calculator run details. Status code: {response.StatusCode}");
            }

            return await response.Content.ReadFromJsonAsync<CalculatorRunDetailsViewModel>()
                ?? new CalculatorRunDetailsViewModel();
        }
    }
}
