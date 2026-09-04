using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels.CsvUpload;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class LocalAuthorityUploadFileControllerTests
{
    private const int RelativeYearStartingMonth = 4;
    private const int SelectedRelativeYear = 2026;
    private const string ApiErrorsKey = "Local_Authority_Upload_Errors";
    private const string LapcapApiPath = "v1/lapcapData";

    private Mock<IEprCalculatorApiService> apiService = null!;
    private IConfiguration configuration = null!;
    private InMemorySession session = null!;
    private LocalAuthorityUploadFileController controller = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        apiService = new Mock<IEprCalculatorApiService>();
        configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>(
                    CommonConstants.RelativeYearStartingMonth,
                    RelativeYearStartingMonth.ToString())
            ])
            .Build();

        session = new InMemorySession();
        session.SetInt32(SessionConstants.RelativeYear, SelectedRelativeYear);
        controller = BuildController(session);
    }

    [TestMethod]
    public void Index_ReturnsEmptyUploadViewModel()
    {
        var result = controller.Index() as ViewResult;
        var model = result?.Model as CsvUploadViewModel;

        Assert.IsNotNull(result);
        Assert.AreEqual("Views/CsvUpload/Index", result.ViewName);
        Assert.IsNotNull(model);
        Assert.IsFalse(model.HasErrors);
        Assert.IsNull(model.ErrorsViewModel);
    }

    [TestMethod]
    public async Task HandleUpload_NullFile_ReturnsIndexWithFileError()
    {
        var result = await controller.Upload(null, CancellationToken.None) as ViewResult;
        var model = result?.Model as CsvUploadViewModel;

        Assert.IsNotNull(result);
        Assert.AreEqual("Views/CsvUpload/Index", result.ViewName);
        Assert.IsNotNull(model);
        Assert.IsTrue(model.HasErrors);
        CollectionAssert.AreEqual(new[] { ErrorMessages.FileNotSelected }, model.ErrorsViewModel!.FileErrors.ToArray());
    }

    [TestMethod]
    public async Task HandleUpload_NonCsvFile_ReturnsIndexWithFileError()
    {
        var file = CreateFormFile("lapcap.txt", BuildValidLapcapCsv());

        var result = await controller.Upload(file, CancellationToken.None) as ViewResult;
        var model = result?.Model as CsvUploadViewModel;

        Assert.IsNotNull(result);
        Assert.AreEqual("Views/CsvUpload/Index", result.ViewName);
        Assert.IsNotNull(model);
        Assert.IsTrue(model.HasErrors);
        CollectionAssert.AreEqual(new[] { ErrorMessages.FileMustBeCSV }, model.ErrorsViewModel!.FileErrors.ToArray());
    }

    [TestMethod]
    public async Task HandleUpload_InvalidTotalCost_ReturnsIndexWithContentErrors()
    {
        var csv = """
            country,material,total_cost
            England,Aluminium,not-a-number
            Wales,Glass,20
            """;
        var file = CreateFormFile("lapcap.csv", csv);

        var result = await controller.Upload(file, CancellationToken.None) as ViewResult;
        Assert.IsNotNull(result);
        Assert.AreEqual("Views/CsvUpload/Index", result.ViewName);

        var model = result.Model as CsvUploadViewModel;
        Assert.IsNotNull(model);
        Assert.IsTrue(model.HasErrors);
        Assert.IsTrue(model.ErrorsViewModel!.ContentErrors.Any(error => error.Contains("Aluminium in England", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task HandleUpload_ValidCsv_ReturnsProcessingViewWithParsedValues()
    {
        var file = CreateFormFile("lapcap.csv", BuildValidLapcapCsv());

        var result = await controller.Upload(file, CancellationToken.None) as ViewResult;
        var model = result?.Model as CsvUploadProcessingViewModel;

        Assert.IsNotNull(result);
        Assert.AreEqual("Views/CsvUpload/Processing", result.ViewName);
        Assert.IsNotNull(model);
        Assert.IsNotNull(model.JsonPayload);

        var request = JsonSerializer.Deserialize<SetLapcapDataRequest>(model.JsonPayload);

        Assert.IsNotNull(request);
        Assert.AreEqual("lapcap.csv", request.Filename);
        Assert.AreEqual(2, request.Values.Count);
        Assert.AreEqual("England", request.Values[0].Country);
        Assert.AreEqual("Aluminium", request.Values[0].Material);
        Assert.AreEqual(2210.45m, request.Values[0].TotalCost);
        Assert.AreEqual("Wales", request.Values[1].Country);
        Assert.AreEqual("Glass", request.Values[1].Material);
        Assert.AreEqual(20m, request.Values[1].TotalCost);
    }

    [TestMethod]
    public async Task SendToApi_WhenApiReturnsCreated_ReturnsNoContentAndPostsExpectedPayload()
    {
        SetLapcapDataRequest? capturedRequest = null;
        apiService
            .Setup(service => service.CallApi(
                HttpMethod.Post,
                LapcapApiPath,
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()))
            .Callback<HttpMethod, string, IDictionary<string, string?>?, object?, CancellationToken>(
                (_, _, _, body, _) => capturedRequest = body as SetLapcapDataRequest)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));

        var model = BuildSetRequest();

        var result = await controller.Process(model, CancellationToken.None) as NoContentResult;

        Assert.IsNotNull(result);
        Assert.AreEqual(StatusCodes.Status204NoContent, result.StatusCode);

        apiService.Verify(service => service.CallApi(
            HttpMethod.Post,
            LapcapApiPath,
            It.IsAny<IDictionary<string, string?>?>(),
            It.IsAny<object?>(),
            It.IsAny<CancellationToken>()), Times.Once);

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(model.Filename, capturedRequest.Filename);
        Assert.AreEqual(new RelativeYear(SelectedRelativeYear), capturedRequest.RelativeYear);
        Assert.AreEqual(model.Values.Count, capturedRequest.Values.Count);
        Assert.AreEqual(model.Values[0].Country, capturedRequest.Values[0].Country);
        Assert.AreEqual(model.Values[0].TotalCost, capturedRequest.Values[0].TotalCost);
    }

    [TestMethod]
    public async Task SendToApi_WhenApiDoesNotReturnCreated_StoresErrorInSessionAndReturnsBadRequest()
    {
        const string apiErrorJson = """{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","errors":{"Values":["Invalid material"]}}""";
        apiService
            .Setup(service => service.CallApi(
                HttpMethod.Post,
                LapcapApiPath,
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(apiErrorJson, Encoding.UTF8, "application/json")
            });

        var result = await controller.Process(BuildSetRequest(), CancellationToken.None) as BadRequestResult;

        Assert.IsNotNull(result);
        Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.AreEqual(apiErrorJson, session.GetString(ApiErrorsKey));
    }

    [TestMethod]
    public void Errors_WhenSessionHasValidProblemDetails_ReturnsIndexWithContentErrorsAndClearsSession()
    {
        const string apiErrorJson = """
            {
              "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
              "title": "One or more validation errors occurred.",
              "status": 400,
              "errors": {
                "Values[0].TotalCost": [ "Total cost must be greater than zero.", "Total cost is required." ],
                "Filename": [ "Filename is required." ]
              }
            }
            """;
        session.SetString(ApiErrorsKey, apiErrorJson);

        var result = controller.Errors() as ViewResult;
        var model = result?.Model as CsvUploadViewModel;

        Assert.IsNotNull(result);
        Assert.AreEqual("Views/CsvUpload/Index", result.ViewName);
        Assert.IsNotNull(model);
        Assert.IsTrue(model.HasErrors);
        CollectionAssert.AreEquivalent(
            new[]
            {
                "Total cost must be greater than zero.",
                "Total cost is required.",
                "Filename is required."
            },
            model.ErrorsViewModel!.ContentErrors.ToArray());
        Assert.IsNull(session.GetString(ApiErrorsKey));
    }

    [TestMethod]
    public void Errors_WhenSessionMissing_RedirectsToStandardError()
    {
        var result = controller.Errors() as RedirectToActionResult;

        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    [TestMethod]
    public void Errors_WhenSessionHasInvalidJson_RedirectsToStandardError()
    {
        session.SetString(ApiErrorsKey, "not-problem-details");

        var result = controller.Errors() as RedirectToActionResult;

        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
        Assert.IsNull(session.GetString(ApiErrorsKey));
    }

    private LocalAuthorityUploadFileController BuildController(ISession controllerSession)
    {
        var urlHelper = new Mock<IUrlHelper>();
        urlHelper
            .Setup(helper => helper.Action(It.IsAny<UrlActionContext>()))
            .Returns((UrlActionContext context) => $"/{context.Controller}/{context.Action}");

        return new LocalAuthorityUploadFileController(
            configuration,
            apiService.Object,
            NullLogger<LocalAuthorityUploadFileController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Session = controllerSession,
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "test.user@paycal")], "TestAuth"))
                }
            },
            Url = urlHelper.Object
        };
    }

    private static SetLapcapDataRequest BuildSetRequest()
    {
        return new SetLapcapDataRequest
        {
            Filename = "lapcap.csv",
            RelativeYear = (RelativeYear) SelectedRelativeYear,
            Values =
            [
                new SetLapcapDataRequest.LapcapValue
                {
                    Country = "England",
                    Material = "Aluminium",
                    TotalCost = 2210.45m
                }
            ],
        };
    }

    private static string BuildValidLapcapCsv()
    {
        return """
            country,material,total_cost
            England,Aluminium,2210.45
            Wales,Glass,20
            """;
    }

    private static FormFile CreateFormFile(string fileName, string content)
    {
        var contentBytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(contentBytes);
        return new FormFile(stream, 0, contentBytes.Length, "fileUpload", fileName);
    }

    private sealed class InMemorySession : ISession
    {
        private readonly Dictionary<string, byte[]> store = new();

        public IEnumerable<string> Keys => store.Keys;

        public string Id { get; } = Guid.NewGuid().ToString();

        public bool IsAvailable => true;

        public void Clear() => store.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => store.Remove(key);

        public void Set(string key, byte[] value) => store[key] = value;

        public bool TryGetValue(string key, out byte[] value) => store.TryGetValue(key, out value!);
    }
}
