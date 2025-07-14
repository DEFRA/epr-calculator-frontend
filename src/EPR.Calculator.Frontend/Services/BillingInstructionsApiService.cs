using System.Configuration;
using System.Text;
using System.Text.Json;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.Services
{
    /// <summary>
    /// Service responsible for handling API operations related to producer billing instructions.
    /// Provides methods to accept or reject billing instructions for a given calculation run.
    /// </summary>
    public class BillingInstructionsApiService(IHttpClientFactory clientFactory, IConfiguration configuration) : IBillingInstructionsApiService
    {
        /// <summary>
        /// Sends a PUT request to the API to accept or reject billing instructions for the specified calculation run.
        /// </summary>
        /// <param name="calculationRunId">The ID of the calculation run for which billing instructions are being updated.</param>
        /// <param name="requestDto">The request data containing organisation IDs, status, and optional reason for rejection.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.
        /// </returns>
        /// <remarks>
        /// The method is not yet implemented and will throw a <see cref="NotImplementedException"/>.
        /// </remarks>
        public async Task<bool> PutAcceptRejectBillingInstructions(int calculationRunId, ProducerBillingInstructionsHttpPutRequestDto requestDto)
        {
            var apiUrl = this.GetApiUrl(ConfigSection.ProducerBillingInstructions, ConfigSection.ProducerBillingInstructionsV2);

            var response = await this.CallApi(HttpMethod.Put, apiUrl, calculationRunId.ToString(), requestDto);

            return response.IsSuccessStatusCode;
        }

        protected async Task<HttpResponseMessage> CallApi(
            HttpMethod httpMethod,
            Uri apiUrl,
            string argument,
            ProducerBillingInstructionsHttpPutRequestDto dto)
        {
            var argsString = !string.IsNullOrEmpty(argument)
                ? $"/{argument}"
                : string.Empty;
            argsString = !argument.Contains('&') ? argsString : $"?{argument}";
            var contentString = JsonSerializer.Serialize(dto);
            var request = new HttpRequestMessage(
                httpMethod,
                new Uri($"{apiUrl}{argsString}"));
            request.Content = new StringContent(
                contentString,
                Encoding.UTF8,
                StaticHelpers.MediaType);
            var client = this.GetHttpClient(dto.AuthorizationToken);
            return await client.SendAsync(request);
        }

        protected Uri GetApiUrl(string configSection, string configKey)
            => new Uri(this.GetConfigSetting(configSection, configKey));

        private HttpClient GetHttpClient(string authToken)
        {
            var client = clientFactory.CreateClient();
            if (!client.DefaultRequestHeaders.Contains("Authorization"))
            {
                client.DefaultRequestHeaders.Add("Authorization", authToken);
            }

            return client;
        }

        private string GetConfigSetting(string configSection, string configKey)
        {
            var value = configuration
                .GetSection(configSection)
                .GetValue<string>(configKey);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ConfigurationErrorsException(
                    $"{configSection}:{configKey} is null or empty. Please check the configuration settings. " +
                    $"{ConfigSection.CalculationRunSettings}");
            }

            return value;
        }
    }
}