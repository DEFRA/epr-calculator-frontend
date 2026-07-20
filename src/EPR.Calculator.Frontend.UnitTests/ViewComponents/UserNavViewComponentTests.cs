using System.Security.Claims;
using EPR.Calculator.Frontend.Components.UserNav;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.ViewComponents;

[TestClass]
public class UserNavViewComponentTests
{
    private UserNavViewComponent component = null!;
    private DefaultHttpContext httpContext = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        httpContext = new DefaultHttpContext
        {
            Request =
            {
                Host = new HostString("example.local"),
                Path = "/current-page"
            },
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, "Test User")
            ], "test-auth"))
        };

        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
        var viewContext = new ViewContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            Mock.Of<IView>(),
            viewData,
            tempData,
            TextWriter.Null,
            new HtmlHelperOptions());

        component = new UserNavViewComponent
        {
            ViewComponentContext = new ViewComponentContext
            {
                ViewContext = viewContext,
                Arguments = new Dictionary<string, object?>(),
                ViewComponentDescriptor = new ViewComponentDescriptor()
            }
        };
    }

    [TestMethod]
    public void Invoke_WhenShowBackLinkIsFalse_DoesNotSetBackLink()
    {
        // Arrange
        const string backLink = "/dashboard";
        SetReferer("https://example.local/dashboard");

        // Act
        var result = component.Invoke(backLink, showBackLink: false);
        var model = GetModel(result);

        // Assert
        Assert.IsNull(model.BackLinkUrl);
    }

    [TestMethod]
    public void Invoke_WhenLocalReferrerMatchesAllowedPath_UsesReferrerIncludingQueryString()
    {
        // Arrange
        const string fallbackBackLink = "/dashboard";
        const string referrer = "https://example.local/dashboard?page=2&sort=desc";
        SetReferer(referrer);

        // Act
        var result = component.Invoke(fallbackBackLink);
        var model = GetModel(result);

        // Assert
        Assert.AreEqual("/dashboard?page=2&sort=desc", model.BackLinkUrl);
    }

    [TestMethod]
    public void Invoke_WhenReferrerIsExternal_UsesFallbackBackLink()
    {
        // Arrange
        const string fallbackBackLink = "/dashboard";
        SetReferer("https://evil.example/dashboard?page=9");

        // Act
        var result = component.Invoke(fallbackBackLink);
        var model = GetModel(result);

        // Assert
        Assert.AreEqual(fallbackBackLink, model.BackLinkUrl);
    }

    [TestMethod]
    public void Invoke_WhenReferrerIsCurrentPage_UsesFallbackBackLink()
    {
        // Arrange
        const string fallbackBackLink = "/dashboard";
        SetReferer("https://example.local/current-page?tab=summary");

        // Act
        var result = component.Invoke(fallbackBackLink);
        var model = GetModel(result);

        // Assert
        Assert.AreEqual(fallbackBackLink, model.BackLinkUrl);
    }

    [TestMethod]
    public void Invoke_WhenReferrerMatchesAllowedBackLink_UsesReferrerInsteadOfFallback()
    {
        // Arrange
        const string fallbackBackLink = "/dashboard";
        string[] allowedBackLinks = ["/search", "/reports"];
        SetReferer("https://example.local/reports?year=2026");

        // Act
        var result = component.Invoke(fallbackBackLink, allowedBackLinks);
        var model = GetModel(result);

        // Assert
        Assert.AreEqual("/reports?year=2026", model.BackLinkUrl);
    }

    [TestMethod]
    public void Invoke_WhenFallbackBackLinkIsNull_UsesFirstAllowedBackLink()
    {
        // Arrange
        string[] allowedBackLinks = ["/search", "/reports"];
        SetReferer("https://example.local/not-allowed");

        // Act
        var result = component.Invoke(backLink: null, allowedBackLinks);
        var model = GetModel(result);

        // Assert
        Assert.AreEqual("/search", model.BackLinkUrl);
    }

    private void SetReferer(string referer)
    {
        httpContext.Request.Headers.Referer = referer;
    }

    private static UserNavViewModel GetModel(IViewComponentResult result)
    {
        var viewResult = result as ViewViewComponentResult;
        Assert.IsNotNull(viewResult);
        Assert.IsNotNull(viewResult.ViewData);
        var model = viewResult.ViewData.Model as UserNavViewModel;
        Assert.IsNotNull(model);
        return model;
    }
}
