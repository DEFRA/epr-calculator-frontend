using System.Net;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class LocalAuthorityUploadFileProcessingControllerTests
{
    private const int RelativeYearStartingMonth = 4;

    private Mock<IEprCalculatorApiService> apiService = null!;
    private LocalAuthorityUploadFileProcessingController controller = null!;
    private DefaultHttpContext httpContext = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        apiService = new Mock<IEprCalculatorApiService>();
        httpContext = new DefaultHttpContext
        {
            Session = BuildSession()
        };

        controller = BuildController(apiService.Object, httpContext);
    }

    [TestMethod]
    public async Task Index_WhenApiReturnsCreated_ReturnsOkResponse()
    {
        // Arrange
        var request = BuildValidRequest();
        var apiResponse = new HttpResponseMessage(HttpStatusCode.Created);
        apiService
            .Setup(service => service.CallApi(
                HttpMethod.Post,
                "v1/lapcapData",
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await controller.Index(request) as OkObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
        Assert.AreSame(apiResponse, result.Value);
    }

    [TestMethod]
    public async Task Index_WhenApiDoesNotReturnCreated_ReturnsBadRequestWithApiErrorContent()
    {
        // Arrange
        var request = BuildValidRequest();
        const string apiError = "lapcap submission rejected";
        apiService
            .Setup(service => service.CallApi(
                HttpMethod.Post,
                "v1/lapcapData",
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(apiError)
            });

        // Act
        var result = await controller.Index(request) as BadRequestObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.AreEqual(apiError, result.Value);
    }

    [TestMethod]
    public async Task Index_WhenRelativeYearInSession_SendsSessionYearInApiPayload()
    {
        // Arrange
        var request = BuildValidRequest();
        const int sessionYear = 2032;
        httpContext.Session.SetInt32(SessionConstants.RelativeYear, sessionYear);
        CreateLapcapDataDto? capturedDto = null;

        apiService
            .Setup(service => service.CallApi(
                HttpMethod.Post,
                "v1/lapcapData",
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()))
            .Callback<HttpMethod, string, IDictionary<string, string?>?, object?>((_, _, _, body) => capturedDto = body as CreateLapcapDataDto)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));

        // Act
        await controller.Index(request);

        // Assert
        apiService.Verify(service => service.CallApi(
            HttpMethod.Post,
            "v1/lapcapData",
            It.IsAny<IDictionary<string, string?>?>(),
            It.IsAny<object?>()), Times.Once);
        Assert.IsNotNull(capturedDto);
        Assert.AreEqual(new RelativeYear(sessionYear), capturedDto.RelativeYear);
        Assert.AreEqual(request.FileName, capturedDto.LapcapFileName);
        CollectionAssert.AreEqual(
            request.LapcapTemplateValue.Select(value => value.CountryName).ToList(),
            capturedDto.LapcapDataTemplateValues.Select(value => value.CountryName).ToList());
    }

    [TestMethod]
    public async Task Index_WhenRelativeYearMissingFromSession_SendsDefaultRelativeYearInApiPayload()
    {
        // Arrange
        var request = BuildValidRequest();
        var expectedYear = CommonUtil.GetDefaultRelativeYear(DateTime.UtcNow, RelativeYearStartingMonth);
        CreateLapcapDataDto? capturedDto = null;

        apiService
            .Setup(service => service.CallApi(
                HttpMethod.Post,
                "v1/lapcapData",
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()))
            .Callback<HttpMethod, string, IDictionary<string, string?>?, object?>((_, _, _, body) => capturedDto = body as CreateLapcapDataDto)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));

        // Act
        await controller.Index(request);

        // Assert
        Assert.IsNotNull(capturedDto);
        Assert.AreEqual(expectedYear, capturedDto.RelativeYear);
    }

    private static LocalAuthorityUploadFileProcessingController BuildController(
        IEprCalculatorApiService apiService,
        HttpContext httpContext)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [CommonConstants.RelativeYearStartingMonth] = RelativeYearStartingMonth.ToString()
            })
            .Build();

        return new LocalAuthorityUploadFileProcessingController(
            configuration,
            apiService,
            new TelemetryClient(new TelemetryConfiguration()))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    private static LapcapRefreshViewModel BuildValidRequest()
    {
        return new LapcapRefreshViewModel
        {
            LapcapTemplateValue =
            [
                new LapcapDataTemplateValueDto
                {
                    CountryName = "England",
                    Material = "Aluminium",
                    TotalCost = "1200.50"
                },
                new LapcapDataTemplateValueDto
                {
                    CountryName = "Scotland",
                    Material = "Glass",
                    TotalCost = "950.00"
                }
            ],
            FileName = "local-authority-data.csv"
        };
    }

    private static ISession BuildSession()
    {
        var storage = new Dictionary<string, byte[]>();
        var session = new Mock<ISession>();
        session.SetupGet(s => s.Id).Returns("local-authority-upload-processing-session");
        session.SetupGet(s => s.IsAvailable).Returns(true);
        session.SetupGet(s => s.Keys).Returns(() => storage.Keys);
        session.Setup(s => s.Clear()).Callback(storage.Clear);
        session.Setup(s => s.Remove(It.IsAny<string>())).Callback<string>(key => storage.Remove(key));
        session.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) => storage[key] = value);
        session.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]?>.IsAny))
            .Returns((string key, out byte[]? value) =>
            {
                var found = storage.TryGetValue(key, out var data);
                value = data;
                return found;
            });
        session.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        session.Setup(s => s.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        return session.Object;
    }
}
