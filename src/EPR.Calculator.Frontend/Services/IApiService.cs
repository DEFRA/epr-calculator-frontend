namespace EPR.Calculator.Frontend.Services
{
    public interface IApiService
    {
        Task<HttpResponseMessage> CallApi(
            HttpContext httpContext,
            HttpMethod httpMethod,
            Uri apiUrl,
            string argument,
            object? body);

        Uri GetApiUrl(string configSection, string configKey);
    }
}
