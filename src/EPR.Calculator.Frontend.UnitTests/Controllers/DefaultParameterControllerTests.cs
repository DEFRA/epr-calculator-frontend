using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Extensions;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class DefaultParameterControllerTests
{
    private const string TestUser = "Test User";
    private const int RelativeYearValue = 2024;

    private Mock<IEprCalculatorApiService> apiService = null!;
    private IConfiguration configuration = null!;
    private List<DefaultSchemeParameters> defaultParameters = null!;
    private DefaultHttpContext httpContext = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        apiService = new Mock<IEprCalculatorApiService>();
        configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [CommonConstants.RelativeYearStartingMonth] = "4"
            })
            .Build();
        httpContext = new DefaultHttpContext
        {
            Session = BuildSession(),
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, TestUser)
            ], "TestAuth"))
        };
        httpContext.Session.SetInt32(SessionConstants.RelativeYear, RelativeYearValue);
        defaultParameters = BuildDefaultSchemeParameters();
    }

    [TestMethod]
    public async Task Index_WhenApiReturnsDefaultParameters_ReturnsViewModelWithMappedCategoriesAndLateTonnage()
    {
        // Arrange
        apiService
            .Setup(service => service.CallApi(
                HttpMethod.Get,
                $"v1/defaultParameterSetting/{RelativeYearValue}",
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()))
            .ReturnsAsync(CreateJsonResponse(HttpStatusCode.OK, defaultParameters));
        var controller = BuildController();

        // Act
        var result = await controller.Index() as ViewResult;
        var model = result?.Model as DefaultParametersViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(model);
        Assert.IsTrue(model.IsDataAvailable);
        Assert.AreEqual(TestUser, model.LastUpdatedBy);
        Assert.AreEqual(Enum.GetValues<ParameterType>().Length, model.SchemeParameters.Count);
        Assert.AreEqual(defaultParameters[0].EffectiveFrom, model.EffectiveFrom);

        var redModulation = model.SchemeParameters
            .Single(parameter => parameter.SchemeParameterName == ParameterType.RedModulationFactor.GetDisplayName())
            .DefaultSchemeParameters
            .Single(parameter => parameter.ParameterUniqueRef == "REDM-RF");
        Assert.AreEqual(1.200m, redModulation.ParameterValue);

        Assert.AreEqual(2, model.LateReportingTonnageParams.Count());
        var aluminium = model.LateReportingTonnageParams.Single(material => material.Material == "Aluminium");
        Assert.AreEqual(170.55m, aluminium.Red);
        Assert.AreEqual(70.55m, aluminium.Amber);
        Assert.AreEqual(270.55m, aluminium.Green);

        apiService.Verify(service => service.CallApi(
            HttpMethod.Get,
            $"v1/defaultParameterSetting/{RelativeYearValue}",
            It.IsAny<IDictionary<string, string?>?>(),
            It.IsAny<object?>()), Times.Once);
    }

    [TestMethod]
    public async Task Index_WhenApiReturnsNotFound_ReturnsViewModelWithDataUnavailableFlag()
    {
        // Arrange
        apiService
            .Setup(service => service.CallApi(
                HttpMethod.Get,
                $"v1/defaultParameterSetting/{RelativeYearValue}",
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));
        var controller = BuildController();

        // Act
        var result = await controller.Index() as ViewResult;
        var model = result?.Model as DefaultParametersViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(model);
        Assert.IsFalse(model.IsDataAvailable);
        Assert.AreEqual(string.Empty, model.LastUpdatedBy);
    }

    [TestMethod]
    public async Task Index_WhenApiReturnsUnexpectedFailure_RedirectsToStandardError()
    {
        // Arrange
        apiService
            .Setup(service => service.CallApi(
                HttpMethod.Get,
                $"v1/defaultParameterSetting/{RelativeYearValue}",
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var controller = BuildController();

        // Act
        var result = await controller.Index() as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    private DefaultParametersController BuildController()
    {
        return new DefaultParametersController(configuration, apiService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    private static HttpResponseMessage CreateJsonResponse(HttpStatusCode statusCode, object? body)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(body ?? new { }),
                Encoding.UTF8,
                "application/json")
        };
    }

    private static List<DefaultSchemeParameters> BuildDefaultSchemeParameters()
    {
        var effectiveFrom = new DateTime(2024, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        var parameterTypeName = ParameterType.LateReportingTonnage.GetDisplayName();

        var parameters = new List<DefaultSchemeParameters>
        {
            CreateDefaultParameter("COMC-AL", ParameterType.CommunicationCostsByMaterial.GetDisplayName(), "Aluminium", 2210.45m, effectiveFrom),
            CreateDefaultParameter("COMC-UK", ParameterType.CommunicationCostsByCountry.GetDisplayName(), "United Kingdom", 250.00m, effectiveFrom),
            CreateDefaultParameter("SAOC-ENG", ParameterType.SchemeAdministratorOperatingCosts.GetDisplayName(), "England", 500.00m, effectiveFrom),
            CreateDefaultParameter("LAPC-ENG", ParameterType.LocalAuthorityDataPreparationCosts.GetDisplayName(), "England", 115.45m, effectiveFrom),
            CreateDefaultParameter("SCSC-ENG", ParameterType.SchemeSetupCosts.GetDisplayName(), "England", 325.55m, effectiveFrom),
            CreateDefaultParameter("BADEBT-P", ParameterType.BadDebtProvision.GetDisplayName(), "Percentage", 5.25m, effectiveFrom),
            CreateDefaultParameter("MATT-AI", ParameterType.MaterialityThreshold.GetDisplayName(), "Amount Increase", 5000.00m, effectiveFrom),
            CreateDefaultParameter("TONT-AI", ParameterType.TonnageChangeThreshold.GetDisplayName(), "Amount Increase", 50.00m, effectiveFrom),
            CreateDefaultParameter("REDM-RF", ParameterType.RedModulationFactor.GetDisplayName(), "Modulation Factor", 1.200m, effectiveFrom),
            CreateDefaultParameter("LRET-AL-R", parameterTypeName, "Aluminium-R", 170.55m, effectiveFrom),
            CreateDefaultParameter("LRET-AL-A", parameterTypeName, "Aluminium-A", 70.55m, effectiveFrom),
            CreateDefaultParameter("LRET-AL-G", parameterTypeName, "Aluminium-G", 270.55m, effectiveFrom),
            CreateDefaultParameter("LRET-FC-R", parameterTypeName, "Fibre composite-R", 180.00m, effectiveFrom),
            CreateDefaultParameter("LRET-FC-A", parameterTypeName, "Fibre composite-A", 80.00m, effectiveFrom),
            CreateDefaultParameter("LRET-FC-G", parameterTypeName, "Fibre composite-G", 280.00m, effectiveFrom)
        };

        return parameters;
    }

    private static DefaultSchemeParameters CreateDefaultParameter(
        string uniqueRef,
        string parameterType,
        string parameterCategory,
        decimal value,
        DateTime effectiveFrom)
    {
        return new DefaultSchemeParameters
        {
            RelativeYear = new RelativeYear(RelativeYearValue),
            EffectiveFrom = effectiveFrom,
            CreatedBy = TestUser,
            CreatedAt = effectiveFrom,
            ParameterUniqueRef = uniqueRef,
            ParameterType = parameterType,
            ParameterCategory = parameterCategory,
            ParameterValue = value
        };
    }

    private static ISession BuildSession()
    {
        var storage = new Dictionary<string, byte[]>();
        var session = new Mock<ISession>();
        session.SetupGet(s => s.Id).Returns("default-parameter-session");
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
