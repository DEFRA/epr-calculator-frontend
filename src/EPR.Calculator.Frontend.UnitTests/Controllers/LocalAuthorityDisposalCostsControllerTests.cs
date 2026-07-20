using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class LocalAuthorityDisposalCostsControllerTests
{
    private const int RelativeYearStartingMonth = 4;
    private const int SelectedRelativeYear = 2024;
    private const string TestUser = "test.user@paycal";
    private const string TestCreatedBy = "Test User";
    private const string ExpectedLapcapPath = "v1/lapcapData/2024";

    private Mock<IEprCalculatorApiService> apiService = null!;
    private IConfiguration configuration = null!;

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
    }

    [TestMethod]
    public async Task Index_WhenApiReturnsSuccess_ReturnsViewWithGroupedCountryData()
    {
        // Arrange
        var localAuthorityDisposalCosts = new List<LocalAuthorityDisposalCost>
        {
            BuildDisposalCost(1, "England", MaterialTypes.Other, 300m),
            BuildDisposalCost(2, "England", "Plastic", 100m),
            BuildDisposalCost(3, "Scotland", "Glass", 200m)
        };
        SetupApiResponse(HttpStatusCode.OK, localAuthorityDisposalCosts);
        var controller = BuildController(TestUser, SelectedRelativeYear);

        // Act
        var result = await controller.Index() as ViewResult;
        var model = result?.Model as LocalAuthorityViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.LocalAuthorityDisposalCostsIndex, result.ViewName);
        Assert.IsNotNull(model);
        Assert.AreEqual(TestCreatedBy, model.LastUpdatedBy);
        Assert.IsNotNull(model.ByCountry);
        Assert.AreEqual(2, model.ByCountry.Count);

        var englandMaterials = model.ByCountry
            .Single(countryGroup => countryGroup.Key == "England")
            .Select(countryData => countryData.Material)
            .ToList();
        CollectionAssert.AreEqual(new[] { "Plastic", MaterialTypes.Other }, englandMaterials);

        apiService.Verify(service => service.CallApi(
            HttpMethod.Get,
            ExpectedLapcapPath,
            It.IsAny<IDictionary<string, string?>>(),
            It.IsAny<object>()), Times.Once);
    }

    [TestMethod]
    public async Task Index_WhenApiReturnsNotFound_ReturnsEmptyViewWithUnknownUserState()
    {
        // Arrange
        SetupApiResponse(HttpStatusCode.NotFound, "No data available.");
        var controller = BuildController(null, SelectedRelativeYear);

        // Act
        var result = await controller.Index() as ViewResult;
        var model = result?.Model as LocalAuthorityViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.LocalAuthorityDisposalCostsIndex, result.ViewName);
        Assert.IsNotNull(model);
        Assert.AreEqual(ErrorMessages.UnknownUser, model.LastUpdatedBy);
        Assert.IsNotNull(model.ByCountry);
        Assert.AreEqual(0, model.ByCountry.Count);
    }

    [TestMethod]
    public async Task Index_WhenApiReturnsFailure_RedirectsToStandardError()
    {
        // Arrange
        SetupApiResponse(HttpStatusCode.InternalServerError, "Unexpected error");
        var controller = BuildController(TestUser, SelectedRelativeYear);

        // Act
        var result = await controller.Index() as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    private void SetupApiResponse(HttpStatusCode statusCode, object responseBody)
    {
        apiService
            .Setup(service => service.CallApi(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<IDictionary<string, string?>>(),
                It.IsAny<object>()))
            .ReturnsAsync(() => new HttpResponseMessage(statusCode)
            {
                Content = statusCode == HttpStatusCode.OK
                    ? JsonContent.Create(responseBody)
                    : new StringContent(responseBody.ToString() ?? string.Empty)
            });
    }

    private LocalAuthorityDisposalCostsController BuildController(string? userName, int relativeYear)
    {
        var session = new InMemorySession();
        session.SetInt32(SessionConstants.RelativeYear, relativeYear);

        var identity = string.IsNullOrWhiteSpace(userName)
            ? new ClaimsIdentity()
            : new ClaimsIdentity([new Claim(ClaimTypes.Name, userName)], "TestAuth");

        return new LocalAuthorityDisposalCostsController(configuration, apiService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Session = session,
                    User = new ClaimsPrincipal(identity)
                }
            }
        };
    }

    private static LocalAuthorityDisposalCost BuildDisposalCost(int id, string country, string material, decimal totalCost)
    {
        return new LocalAuthorityDisposalCost
        {
            Id = id,
            RelativeYear = new RelativeYear(SelectedRelativeYear),
            EffectiveFrom = new DateTime(2024, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedBy = TestCreatedBy,
            CreatedAt = new DateTime(2024, 4, 2, 0, 0, 0, DateTimeKind.Utc),
            LapcapDataMasterId = 100 + id,
            LapcapTempUniqueRef = $"REF-{id}",
            Country = country,
            Material = material,
            TotalCost = totalCost
        };
    }

    private sealed class InMemorySession : ISession
    {
        private readonly Dictionary<string, byte[]> store = new();

        public IEnumerable<string> Keys => store.Keys;

        public string Id { get; } = Guid.NewGuid().ToString();

        public bool IsAvailable => true;

        public void Clear()
        {
            store.Clear();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            store.Remove(key);
        }

        public void Set(string key, byte[] value)
        {
            store[key] = value;
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            return store.TryGetValue(key, out value!);
        }
    }
}
