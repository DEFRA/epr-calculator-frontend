using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.ViewModels;
using System.Net;

namespace EPR.Calculator.Frontend.Services
{
    public class CalculatorRunDetailsService(
        IApiService apiService) : ICalculatorRunDetailsService
    {
        /// <inheritdoc />
        public async Task<CalculatorRunDetailsViewModel> GetCalculatorRundetailsAsync(
            HttpContext httpContext,
            int runId)
        {
            var apiUrl = apiService.GetApiUrl(
                    ConfigSection.DashboardCalculatorRun,
                    ConfigSection.DashboardCalculatorRunApi);

            var response = await apiService.CallApi(
                httpContext,
                HttpMethod.Get,
                apiUrl,
                runId.ToString(),
                null);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException(
                    $"Failed to retrieve calculator run details. Status code: {response.StatusCode}");
            }

            var runDetails = response.Content.ReadFromJsonAsync<CalculatorRunDetailsViewModel>().Result;

            return runDetails;
        }
    }
}
