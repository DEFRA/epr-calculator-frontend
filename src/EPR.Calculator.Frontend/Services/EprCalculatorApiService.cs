using System.Configuration;
using EPR.Calculator.Frontend.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Services;

public interface IEprCalculatorApiService
{
    Task<HttpResponseMessage> CallApi(HttpMethod httpMethod, string relativePath, IDictionary<string, string?>? queryParams = null, object? body = null);
    Task<CalculatorRunDto?> GetCalculatorRun(int runId);
    Task<CalculatorRunDto?> GetCalculatorRun(string runName);
    Task<List<CalculatorRunDto>> FindCalculatorRuns(RelativeYear relativeYear);
    Task<List<RelativeYear>> FindRelativeYears();
    Task DeleteCalculatorRun(int runId);

    Task<T?> Get<T>(string relativePath, IDictionary<string, string?>? queryParams = null)
        where T : class;
}

public class EprCalculatorApiService(
    IConfiguration configuration,
    TelemetryClient telemetryClient,
    IHttpClientFactory clientFactory,
    ITokenAcquisition tokenAcquisition)
    : IEprCalculatorApiService
{
    private readonly Uri baseUri = new(configuration.GetRequiredSection("EprCalculatorApiService").GetValue<string>("BaseUrl")!);

    /// <inheritdoc />
    public async Task<HttpResponseMessage> CallApi(
        HttpMethod httpMethod,
        string relativePath,
        IDictionary<string, string?>? queryParams = null,
        object? body = null)
    {
        var uri = queryParams is { Count: > 0 }
            ? new Uri(QueryHelpers.AddQueryString(new Uri(baseUri, relativePath).ToString(), queryParams))
            : new Uri(baseUri, relativePath);

        var request = new HttpRequestMessage(httpMethod, uri);

        if (body is not null)
            request.Content = JsonContent.Create(body);

        var client = await GetHttpClient();
        return await client.SendAsync(request);
    }

    public Task<CalculatorRunDto?> GetCalculatorRun(int runId)
    {
        return Get<CalculatorRunDto>($"v1/calculatorRuns/{runId}");
    }

    public Task<CalculatorRunDto?> GetCalculatorRun(string runName)
    {
        var encodedRunName = Uri.EscapeDataString(runName);
        return Get<CalculatorRunDto>($"v1/calculatorRuns/{encodedRunName}");
    }

    public async Task<List<CalculatorRunDto>> FindCalculatorRuns(RelativeYear relativeYear)
    {
        var response = await CallApi(
            HttpMethod.Post,
            "v1/calculatorRuns",
            body: new { RelativeYear = relativeYear });

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<List<CalculatorRunDto>>() ?? [];

        telemetryClient.TrackTrace("Unable to fetch calculator runs from API", SeverityLevel.Error);
        return [];
    }

    public async Task<List<RelativeYear>> FindRelativeYears()
    {
        var response = await CallApi(HttpMethod.Get, "v1/RelativeYears");

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<List<RelativeYear>>() ?? [];

        telemetryClient.TrackTrace("Unable to fetch relative years from API", SeverityLevel.Error);
        return [];
    }

    public async Task DeleteCalculatorRun(int runId)
    {
        var response = await CallApi(HttpMethod.Delete, $"v1/calculatorRuns/{runId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<T?> Get<T>(string relativePath, IDictionary<string, string?>? queryParams = null)
        where T : class
    {
        var response = await CallApi(HttpMethod.Get, relativePath, queryParams);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<T>();

        return null;
    }

    private async Task<string> AcquireToken()
    {
        telemetryClient.TrackTrace("AcquireToken");

        var scopes = configuration["DownstreamApi:Scopes"]?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];

        if (scopes.Length == 0)
            throw new ConfigurationErrorsException("DownstreamApi:Scopes is null or empty. Please check the configuration settings.");

        try
        {
            telemetryClient.TrackTrace($"GetAccessTokenForUserAsync with scopes: {string.Join(",", scopes)}");

            var token = await tokenAcquisition.GetAccessTokenForUserAsync(scopes);

            return $"Bearer {token}";
        }
        catch (MsalUiRequiredException ex)
        {
            throw new MicrosoftIdentityWebChallengeUserException(ex, scopes);
        }
    }

    private async Task<HttpClient> GetHttpClient()
    {
        var client = clientFactory.CreateClient();

        if (!client.DefaultRequestHeaders.Contains("Authorization"))
        {
            var accessToken = await AcquireToken();
            client.DefaultRequestHeaders.Add("Authorization", accessToken);
        }

        return client;
    }
}
