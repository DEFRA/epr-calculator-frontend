using System.Configuration;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Services;

[TestClass]
public class EprCalculatorApiServiceTests
{
    private const string BaseApiUrl = "https://calculator-api.local/";
    private const string DefaultScopes = "scope.read scope.write";
    private const string AccessToken = "access-token";

    private Mock<IHttpClientFactory> _httpClientFactory = null!;
    private HttpClient _httpClient = null!;
    private CapturingMessageHandler _messageHandler = null!;
    private Mock<ITokenAcquisition> _tokenAcquisition = null!;
    private EprCalculatorApiService _service = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _messageHandler = new CapturingMessageHandler();
        _httpClient = new HttpClient(_messageHandler);

        _httpClientFactory = new Mock<IHttpClientFactory>();
        _httpClientFactory
            .Setup(factory => factory.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);

        _tokenAcquisition = new Mock<ITokenAcquisition>();
        _tokenAcquisition
            .Setup(token => token.GetAccessTokenForUserAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<TokenAcquisitionOptions>()))
            .ReturnsAsync(AccessToken);

        _service = CreateService();
    }

    [TestMethod]
    public async Task CallApi_BuildsRequestWithQueryBodyAndAuthorization()
    {
        // Arrange
        const string relativePath = "v1/calculatorRuns";
        var queryParams = new Dictionary<string, string?>
        {
            ["status"] = "open",
            ["page"] = "2",
        };
        var body = new TestRequestBody { RunName = "Billing Run 2026" };
        _messageHandler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.Accepted);

        // Act
        var response = await _service.CallApi(HttpMethod.Post, relativePath, queryParams, body);

        // Assert
        Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
        Assert.IsNotNull(_messageHandler.LastRequest);
        Assert.AreEqual(HttpMethod.Post, _messageHandler.LastRequest!.Method);
        Assert.AreEqual("/v1/calculatorRuns", _messageHandler.LastRequest.RequestUri?.AbsolutePath);

        var parsedQuery = QueryHelpers.ParseQuery(_messageHandler.LastRequest.RequestUri?.Query ?? string.Empty);
        Assert.AreEqual("open", parsedQuery["status"].ToString());
        Assert.AreEqual("2", parsedQuery["page"].ToString());

        var requestBodyJson = await _messageHandler.LastRequest.Content!.ReadAsStringAsync();
        var requestBody = JsonSerializer.Deserialize<TestRequestBody>(
            requestBodyJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.IsNotNull(requestBody);
        Assert.AreEqual("Billing Run 2026", requestBody.RunName);

        Assert.IsTrue(_httpClient.DefaultRequestHeaders.Contains("Authorization"));
        Assert.AreEqual("Bearer access-token", _httpClient.DefaultRequestHeaders.GetValues("Authorization").Single());

        _tokenAcquisition.Verify(token => token.GetAccessTokenForUserAsync(
            It.Is<IEnumerable<string>>(scopes => scopes.SequenceEqual(new[] { "scope.read", "scope.write" })),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<TokenAcquisitionOptions>()), Times.Once);
    }

    [TestMethod]
    public async Task CallApi_DoesNotAcquireToken_WhenAuthorizationHeaderAlreadyExists()
    {
        // Arrange
        _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer pre-existing-token");
        _messageHandler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK);

        // Act
        var response = await _service.CallApi(HttpMethod.Get, "v1/calculatorRuns/45");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        _tokenAcquisition.Verify(token => token.GetAccessTokenForUserAsync(
            It.IsAny<IEnumerable<string>>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<TokenAcquisitionOptions>()), Times.Never);
    }

    [TestMethod]
    public async Task CallApi_ThrowsConfigurationErrorsException_WhenScopesAreMissing()
    {
        // Arrange
        var serviceWithoutScopes = CreateService(BuildConfiguration(string.Empty));

        // Act
        var action = async () => await serviceWithoutScopes.CallApi(HttpMethod.Get, "v1/calculatorRuns/45");

        // Assert
        await Assert.ThrowsExceptionAsync<ConfigurationErrorsException>(action);
    }

    [TestMethod]
    public async Task Get_ReturnsDeserializedPayload_WhenResponseIsSuccessful()
    {
        // Arrange
        var expectedPayload = new TestResponseBody { Id = 11, Name = "run-11" };
        _messageHandler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedPayload),
        };

        // Act
        var result = await _service.Get<TestResponseBody>("v1/calculatorRuns/11");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(11, result.Id);
        Assert.AreEqual("run-11", result.Name);
    }

    [TestMethod]
    public async Task Get_ReturnsNull_WhenResponseIsNotSuccessful()
    {
        // Arrange
        _messageHandler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.BadRequest);

        // Act
        var result = await _service.Get<TestResponseBody>("v1/calculatorRuns/11");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetCalculatorRun_EncodesRunNameBeforeCallingApi()
    {
        // Arrange
        const string runName = "My Run / 2026";
        var expectedPath = $"/v1/calculatorRuns/{Uri.EscapeDataString(runName)}";
        _messageHandler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.BadRequest);

        // Act
        var result = await _service.GetCalculatorRun(runName);

        // Assert
        Assert.IsNull(result);
        Assert.IsNotNull(_messageHandler.LastRequest);
        Assert.AreEqual(expectedPath, _messageHandler.LastRequest!.RequestUri?.AbsolutePath);
    }

    [TestMethod]
    public async Task DeleteCalculatorRun_Throws_WhenApiReturnsFailure()
    {
        // Arrange
        _messageHandler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.InternalServerError);

        // Act
        var action = async () => await _service.DeleteCalculatorRun(78);

        // Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(action);
        Assert.IsNotNull(_messageHandler.LastRequest);
        Assert.AreEqual(HttpMethod.Delete, _messageHandler.LastRequest!.Method);
        Assert.AreEqual("/v1/calculatorRuns/78", _messageHandler.LastRequest.RequestUri?.AbsolutePath);
    }

    [TestMethod]
    public async Task DeleteCalculatorRun_SendsDeleteRequest_WhenApiReturnsSuccess()
    {
        // Arrange
        _messageHandler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.NoContent);

        // Act
        await _service.DeleteCalculatorRun(78);

        // Assert
        Assert.IsNotNull(_messageHandler.LastRequest);
        Assert.AreEqual(HttpMethod.Delete, _messageHandler.LastRequest!.Method);
        Assert.AreEqual("/v1/calculatorRuns/78", _messageHandler.LastRequest.RequestUri?.AbsolutePath);
    }

    [TestMethod]
    public async Task GetCalculatorRun_ByRunId_ReturnsDeserializedRun_WhenResponseIsSuccessful()
    {
        // Arrange
        var expectedRun = new CalculatorRunDto { RunId = 42, RunName = "Billing Run 42" };
        _messageHandler.ResponseFactory = _ => JsonResponse(HttpStatusCode.OK, expectedRun);

        // Act
        var result = await _service.GetCalculatorRun(42);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(42, result.RunId);
        Assert.AreEqual("Billing Run 42", result.RunName);
        Assert.IsNotNull(_messageHandler.LastRequest);
        Assert.AreEqual("/v1/calculatorRuns/42", _messageHandler.LastRequest!.RequestUri?.AbsolutePath);
    }

    [TestMethod]
    public async Task FindCalculatorRuns_ReturnsRuns_WhenResponseIsSuccessful()
    {
        // Arrange
        var relativeYear = new RelativeYear(2026);
        var expectedRuns = new[]
        {
            new CalculatorRunDto { RunId = 1, RunName = "run-1" },
            new CalculatorRunDto { RunId = 2, RunName = "run-2" },
        };
        _messageHandler.ResponseFactory = _ => JsonResponse(HttpStatusCode.OK, expectedRuns);

        // Act
        var result = await _service.FindCalculatorRuns(relativeYear);

        // Assert
        CollectionAssert.AreEqual(new[] { 1, 2 }, result.Select(run => run.RunId).ToArray());
        Assert.IsNotNull(_messageHandler.LastRequest);
        Assert.AreEqual(HttpMethod.Post, _messageHandler.LastRequest!.Method);
        Assert.AreEqual("/v1/calculatorRuns", _messageHandler.LastRequest.RequestUri?.AbsolutePath);

        var requestBodyJson = await _messageHandler.LastRequest.Content!.ReadAsStringAsync();
        var requestBody = JsonSerializer.Deserialize<FindCalculatorRunsRequestBody>(
            requestBodyJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.IsNotNull(requestBody);
        Assert.AreEqual(2026, requestBody.RelativeYear);
    }

    [TestMethod]
    public async Task FindCalculatorRuns_ReturnsEmptyList_WhenResponseIsNotSuccessful()
    {
        // Arrange
        _messageHandler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.InternalServerError);

        // Act
        var result = await _service.FindCalculatorRuns(new RelativeYear(2026));

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task FindRelativeYears_ReturnsYears_WhenResponseIsSuccessful()
    {
        // Arrange
        var expectedYears = new[] { new RelativeYear(2025), new RelativeYear(2026) };
        _messageHandler.ResponseFactory = _ => JsonResponse(HttpStatusCode.OK, expectedYears);

        // Act
        var result = await _service.FindRelativeYears();

        // Assert
        CollectionAssert.AreEqual(new[] { 2025, 2026 }, result.Select(year => year.Value).ToArray());
        Assert.IsNotNull(_messageHandler.LastRequest);
        Assert.AreEqual(HttpMethod.Get, _messageHandler.LastRequest!.Method);
        Assert.AreEqual("/v1/RelativeYears", _messageHandler.LastRequest.RequestUri?.AbsolutePath);
    }

    [TestMethod]
    public async Task FindRelativeYears_ReturnsEmptyList_WhenResponseIsNotSuccessful()
    {
        // Arrange
        _messageHandler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);

        // Act
        var result = await _service.FindRelativeYears();

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task CallApi_ThrowsChallengeException_WhenInteractiveSignInIsRequired()
    {
        // Arrange
        _tokenAcquisition
            .Setup(token => token.GetAccessTokenForUserAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<TokenAcquisitionOptions>()))
            .ThrowsAsync(new MsalUiRequiredException("ui_required", "Interaction required"));

        // Act
        var action = async () => await _service.CallApi(HttpMethod.Get, "v1/calculatorRuns/45");

        // Assert
        await Assert.ThrowsExceptionAsync<MicrosoftIdentityWebChallengeUserException>(action);
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, object payload)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = JsonContent.Create(payload),
        };
    }

    private static IConfiguration BuildConfiguration(string? scopes = DefaultScopes)
    {
        var values = new Dictionary<string, string?>
        {
            ["EprCalculatorApiService:BaseUrl"] = BaseApiUrl,
            ["DownstreamApi:Scopes"] = scopes,
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private EprCalculatorApiService CreateService(IConfiguration? configuration = null)
    {
        return new EprCalculatorApiService(
            configuration ?? BuildConfiguration(),
            new TelemetryClient(new TelemetryConfiguration()),
            _httpClientFactory.Object,
            _tokenAcquisition.Object);
    }

    private sealed class TestRequestBody
    {
        public string RunName { get; init; } = string.Empty;
    }

    private sealed class TestResponseBody
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;
    }

    private sealed class FindCalculatorRunsRequestBody
    {
        public int RelativeYear { get; init; }
    }

    private sealed class CapturingMessageHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, HttpResponseMessage> ResponseFactory { get; set; } =
            _ => new HttpResponseMessage(HttpStatusCode.OK);

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(ResponseFactory(request));
        }
    }
}
