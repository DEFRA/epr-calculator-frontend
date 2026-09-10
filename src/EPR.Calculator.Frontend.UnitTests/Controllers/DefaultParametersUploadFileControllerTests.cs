using System.Net;
using System.Security.Claims;
using System.Text;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels.CsvUpload;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class DefaultParametersUploadFileControllerTests
{
    private const int RelativeYearStartingMonth = 4;
    private const int SelectedRelativeYear = 2026;
    private const string ApiErrorsKey = "Default_Parameters_Upload_Errors";
    private const string DefaultParametersApiPath = "v1/defaultParameterSetting";

    private Mock<IEprCalculatorApiService> apiService = null!;
    private IConfiguration configuration = null!;
    private InMemorySession session = null!;
    private DefaultParametersUploadFileController controller = null!;

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
        Assert.AreEqual("Upload new default calculator parameters", model.Title);
        Assert.AreEqual("/DefaultParameters/Index", model.BackLinkUrl);
        Assert.AreEqual("/DefaultParametersUploadFile/Upload", model.UploadUrl);
        Assert.IsFalse(model.HasErrors);
        Assert.IsNull(model.Errors);
    }

    [TestMethod]
    public void Index_OffersTheSpreadsheetTemplateForDownload()
    {
        var result = controller.Index() as ViewResult;
        var model = result?.Model as CsvUploadViewModel;

        Assert.IsNotNull(model);
        Assert.IsTrue(model.HasDownloadableTemplate);
        Assert.IsNotNull(model.DownloadTemplate);
        Assert.AreEqual("/DefaultParametersUploadFile/DownloadCsvTemplate", model.DownloadTemplate.Url);
        Assert.AreEqual("default parameters spreadsheet template", model.DownloadTemplate.LinkText);
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
        CollectionAssert.AreEqual(new[] { ErrorMessages.FileNotSelected }, model.Errors!.FileErrors.ToArray());
    }

    [TestMethod]
    public async Task HandleUpload_NonCsvFile_ReturnsIndexWithFileError()
    {
        var file = CreateFormFile("default-parameters.txt", BuildValidParametersCsv());

        var result = await controller.Upload(file, CancellationToken.None) as ViewResult;
        var model = result?.Model as CsvUploadViewModel;

        Assert.IsNotNull(result);
        Assert.AreEqual("Views/CsvUpload/Index", result.ViewName);
        Assert.IsNotNull(model);
        Assert.IsTrue(model.HasErrors);
        CollectionAssert.AreEqual(new[] { ErrorMessages.FileMustBeCSV }, model.Errors!.FileErrors.ToArray());
    }

    [TestMethod]
    public async Task HandleUpload_MissingValueColumn_ReturnsIndexWithContentErrors()
    {
        var csv = """
            Parameter Unique Ref,Parameter Category
            COMC-AL,Aluminium
            """;
        var file = CreateFormFile("default-parameters.csv", csv);

        var result = await controller.Upload(file, CancellationToken.None) as ViewResult;
        Assert.IsNotNull(result);
        Assert.AreEqual("Views/CsvUpload/Index", result.ViewName);

        var model = result.Model as CsvUploadViewModel;
        Assert.IsNotNull(model);
        Assert.IsTrue(model.HasErrors);
        Assert.IsTrue(model.Errors!.ContentErrors.Any(error => error.Contains("Parameter Value", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task HandleUpload_ValidCsv_ReturnsProcessingViewWithParsedParameters()
    {
        var file = CreateFormFile("default-parameters.csv", BuildValidParametersCsv());

        var result = await controller.Upload(file, CancellationToken.None) as ViewResult;
        var model = result?.Model as CsvUploadProcessingViewModel;

        Assert.IsNotNull(result);
        Assert.AreEqual("Views/CsvUpload/Processing", result.ViewName);
        Assert.IsNotNull(model);
        Assert.AreEqual("/DefaultParametersUploadFile/Process", model.ProcessingUrl);
        Assert.AreEqual("/DefaultParametersConfirmation/Index", model.SuccessUrl);
        Assert.AreEqual("/DefaultParametersUploadFile/Errors", model.ErrorUrl);
        Assert.IsNotNull(model.Payload);

        var request = model.Payload as SetDefaultParametersRequest;

        Assert.IsNotNull(request);
        Assert.AreEqual("default-parameters.csv", request.Filename);
        Assert.AreEqual(new RelativeYear(SelectedRelativeYear), request.RelativeYear);
        Assert.AreEqual(2, request.Parameters.Count);
        Assert.AreEqual("COMC-AL", request.Parameters[0].Id);
        Assert.AreEqual("2210.45", request.Parameters[0].Value);
        Assert.AreEqual("COMC-GL", request.Parameters[1].Id);
        Assert.AreEqual("1000.00", request.Parameters[1].Value);
    }

    [TestMethod]
    [DataRow("Parameter upload version v1.0")]
    [DataRow("PARAMETER UPLOAD VERSION V1.0")]
    [DataRow("upload version")]
    public async Task HandleUpload_ValidCsv_DiscardsTheUploadVersionRow(string uploadVersionId)
    {
        var csv = $"""
            Parameter Unique Ref,Parameter Value
            COMC-AL,2210.45
            {uploadVersionId},
            """;
        var file = CreateFormFile("default-parameters.csv", csv);

        var result = await controller.Upload(file, CancellationToken.None) as ViewResult;
        var request = (result?.Model as CsvUploadProcessingViewModel)?.Payload as SetDefaultParametersRequest;

        Assert.IsNotNull(result);
        Assert.AreEqual("Views/CsvUpload/Processing", result.ViewName);
        Assert.IsNotNull(request);
        Assert.AreEqual(1, request.Parameters.Count);
        Assert.AreEqual("COMC-AL", request.Parameters[0].Id);
    }

    [TestMethod]
    public async Task SendToApi_WhenApiReturnsCreated_ReturnsNoContentAndPostsExpectedPayload()
    {
        SetDefaultParametersRequest? capturedRequest = null;
        apiService
            .Setup(service => service.CallApi(
                HttpMethod.Put,
                DefaultParametersApiPath,
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()))
            .Callback<HttpMethod, string, IDictionary<string, string?>?, object?, CancellationToken>(
                (_, _, _, body, _) => capturedRequest = body as SetDefaultParametersRequest)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));

        var model = BuildSetRequest();

        var result = await controller.Process(model, CancellationToken.None) as NoContentResult;

        Assert.IsNotNull(result);
        Assert.AreEqual(StatusCodes.Status204NoContent, result.StatusCode);

        apiService.Verify(service => service.CallApi(
            HttpMethod.Put,
            DefaultParametersApiPath,
            It.IsAny<IDictionary<string, string?>?>(),
            It.IsAny<object?>(),
            It.IsAny<CancellationToken>()), Times.Once);

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(model.Filename, capturedRequest.Filename);
        Assert.AreEqual(new RelativeYear(SelectedRelativeYear), capturedRequest.RelativeYear);
        Assert.AreEqual(model.Parameters.Count, capturedRequest.Parameters.Count);
        Assert.AreEqual(model.Parameters[0].Id, capturedRequest.Parameters[0].Id);
        Assert.AreEqual(model.Parameters[0].Value, capturedRequest.Parameters[0].Value);
    }

    [TestMethod]
    public async Task SendToApi_WhenApiDoesNotReturnCreated_StoresErrorInSessionAndReturnsBadRequest()
    {
        const string apiErrorJson = """{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","errors":{"Parameters":["Invalid parameter"]}}""";
        apiService
            .Setup(service => service.CallApi(
                HttpMethod.Put,
                DefaultParametersApiPath,
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
                "Parameters[0].Value": [ "Value must be greater than zero.", "Value is required." ],
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
        CollectionAssert.AreEqual(Array.Empty<string>(), model.Errors!.FileErrors.ToArray());
        CollectionAssert.AreEquivalent(
            new[]
            {
                "Value must be greater than zero.",
                "Value is required.",
                "Filename is required."
            },
            model.Errors.ContentErrors.ToArray());
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

    [TestMethod]
    public void DownloadCsvTemplate_ReturnsTheSpreadsheetTemplate()
    {
        var result = controller.DownloadCsvTemplate() as VirtualFileResult;

        Assert.IsNotNull(result);
        Assert.AreEqual("~/templates/DefaultParameterTemplate.xlsx", result.FileName);
        Assert.AreEqual("DefaultParameterTemplate.xlsx", result.FileDownloadName);
        Assert.AreEqual(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            result.ContentType);
    }

    private DefaultParametersUploadFileController BuildController(ISession controllerSession)
    {
        var urlHelper = new Mock<IUrlHelper>();
        urlHelper
            .Setup(helper => helper.Action(It.IsAny<UrlActionContext>()))
            .Returns((UrlActionContext context) => $"/{context.Controller}/{context.Action}");

        return new DefaultParametersUploadFileController(configuration, apiService.Object)
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

    private static SetDefaultParametersRequest BuildSetRequest()
    {
        return new SetDefaultParametersRequest
        {
            Filename = "default-parameters.csv",
            RelativeYear = (RelativeYear) SelectedRelativeYear,
            Parameters =
            [
                new SetDefaultParametersRequest.ParameterValue
                {
                    Id = "COMC-AL",
                    Value = "2210.45"
                }
            ],
        };
    }

    private static string BuildValidParametersCsv()
    {
        return """
            Parameter Unique Ref,Parameter Type,Parameter Category,Parameter Value
            COMC-AL,Communication costs by material,Aluminium,2210.45
            COMC-GL,Communication costs by material,Glass,1000.00
            """;
    }

    private static FormFile CreateFormFile(string fileName, string content)
    {
        var contentBytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(contentBytes);
        return new FormFile(stream, 0, contentBytes.Length, "fileUpload", fileName);
    }
}
