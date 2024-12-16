using System.Text;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class ParameterUploadFileErrorControllerTests
    {
        private Mock<IMemoryCache> _mockMemoryCache;

        public ParameterUploadFileErrorControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
        }

        private Fixture Fixture { get; init; }

        private Mock<HttpContext> MockHttpContext { get; init; }

        [TestInitialize]
        public void Setup()
        {
            _mockMemoryCache = new Mock<IMemoryCache>();
        }

        [TestMethod]
        public void ParameterUploadCSVErrorController_View_Global_Validation_Test()
        {
            var errors = new List<ValidationErrorDto>();
            errors.AddRange([
                new ValidationErrorDto { ErrorMessage = "Parameter Unique reference is incorrect", Exception = string.Empty },
                new ValidationErrorDto { ErrorMessage = "Parameter value is incorrect", Exception = string.Empty }
            ]);

            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString(UploadFileErrorIds.DefaultParameterUploadErrors, JsonConvert.SerializeObject(errors));

            var key = "testKey";
            var mockData = "[{\"parameterUniqueRef\":\"BADEBT-P\",\"parameterCategory\":\"Percentage\",\"parameterType\":\"Bad debt provision\",\"message\":\"The Bad debt provision must be between 0% and 999.99%\",\"description\":\"\"}]";
            var encodedData = Convert.ToBase64String(Encoding.UTF8.GetBytes(mockData));

            object cachedValue = encodedData;

            var mockMemoryCache = new Mock<IMemoryCache>();
            mockMemoryCache
                .Setup(m => m.TryGetValue(key, out cachedValue))
                .Returns(true);

            var controller = new ParameterUploadFileErrorController(mockMemoryCache.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index("testKey") as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.ParameterUploadFileErrorIndex, result.ViewName);
        }

        [TestMethod]
        public void ParameterUploadCSVErrorController_Standard_Error_Test()
        {
            var mockHttpSession = new MockHttpSession();

            var key = "testKey";
            var mockData = "[{\"parameterUniqueRef\":\"BADEBT-P\",\"parameterCategory\":\"Percentage\",\"parameterType\":\"Bad debt provision\",\"message\":\"The Bad debt provision must be between 0% and 999.99%\",\"description\":\"\"}]";
            var encodedData = Convert.ToBase64String(Encoding.UTF8.GetBytes(mockData));

            object cachedValue = encodedData;

            var mockMemoryCache = new Mock<IMemoryCache>();
            mockMemoryCache
                .Setup(m => m.TryGetValue(key, out cachedValue))
                .Returns(true);

            var controller = new ParameterUploadFileErrorController(_mockMemoryCache.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index("testKey") as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public void ParameterUploadCSVErrorController_No_Error_Messages_Test()
        {
            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString(UploadFileErrorIds.DefaultParameterUploadErrors, string.Empty);

            string cacheKey = "testKey";
            string cacheValue = "testValue";

            object outValue;
            _mockMemoryCache.Setup(m => m.TryGetValue(cacheKey, out outValue))
                .Returns(true);

            var controller = new ParameterUploadFileErrorController(_mockMemoryCache.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index("testKey") as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public void ParameterUploadCsvErrorController_Upload_Test()
        {
            var content = MockData.GetSchemeParametersFileContent();
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, string.Empty, "SchemeParameters.csv");

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            tempData["FilePath"] = "some random file location";

            string cacheKey = "testKey";
            string cacheValue = "testValue";

            object outValue;
            _mockMemoryCache.Setup(m => m.TryGetValue(cacheKey, out outValue))
                .Returns(true);

            var controller = new ParameterUploadFileErrorController(_mockMemoryCache.Object)
            {
                TempData = tempData
            };

            var result = controller.Upload(file);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ParameterUploadCsvErrorController_Post_Returns_Ok_Test()
        {
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper
                .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                .Returns("Index");

            var mockHttpSession = new MockHttpSession();
            var controller = new ParameterUploadFileErrorController(_mockMemoryCache.Object)
            {
                Url = mockUrlHelper.Object
            };
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var sampleData = "[{\"parameterUniqueRef\":\"BADEBT-P\",\"parameterCategory\":\"Percentage\",\"parameterType\":\"Bad debt provision\",\"message\":\"The Bad debt provision must be between 0% and 999.99%\",\"description\":\"\"}]";
            var errors = new DataRequest { Data = sampleData };
            var mockCacheEntry = new Mock<ICacheEntry>();

            _mockMemoryCache.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(mockCacheEntry.Object);

            var result = controller.Index(errors) as OkObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [TestMethod]
        public async Task ParameterUploadCSVErrorController_Upload_View_Post_Incorrect_File_Extension_Error_Test()
        {
            var content = string.Empty;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, string.Empty, "SchemeParameters.xlsx");

            var controller = new ParameterUploadFileErrorController(_mockMemoryCache.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = this.MockHttpContext.Object };

            var result = await controller.Upload(file) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.ParameterUploadFileErrorIndex, result.ViewName);
        }

        [TestMethod]
        public async Task ParameterUploadCSVErrorController_Upload_View_Post_Test()
        {
            var content = MockData.GetSchemeParametersFileContent();
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, string.Empty, "SchemeParameters.csv");

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            tempData[UploadFileErrorIds.DefaultParameterUploadErrors] = string.Empty;

            string cacheKey = "testKey";
            string cacheValue = "testValue";

            object outValue;
            _mockMemoryCache.Setup(m => m.TryGetValue(cacheKey, out outValue))
                .Returns(true);

            var controller = new ParameterUploadFileErrorController(_mockMemoryCache.Object)
            {
                TempData = tempData
            };

            var mockSession = new Mock<ISession>();
            MockHttpContext.Setup(s => s.Session).Returns(mockSession.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = this.MockHttpContext.Object };

            var result = await controller.Upload(file) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.ParameterUploadFileRefresh, result.ViewName);
        }

        [TestMethod]
        public async Task ParameterUploadCSVErrorController_View_Get_Error_Test()
        {
            var mockHttpSession = new MockHttpSession();

            var errors = new List<CreateDefaultParameterSettingErrorDto>() { new CreateDefaultParameterSettingErrorDto { Message = "Some message" } };
            mockHttpSession.SetString(UploadFileErrorIds.DefaultParameterUploadErrors, JsonConvert.SerializeObject(errors).ToString());

            var key = "testKey";
            var mockData = "[{\"parameterUniqueRef\":\"BADEBT-P\",\"parameterCategory\":\"Percentage\",\"parameterType\":\"Bad debt provision\",\"message\":\"The Bad debt provision must be between 0% and 999.99%\",\"description\":\"\"}]";
            var encodedData = Convert.ToBase64String(Encoding.UTF8.GetBytes(mockData));

            object cachedValue = encodedData;

            var mockMemoryCache = new Mock<IMemoryCache>();
            mockMemoryCache
                .Setup(m => m.TryGetValue(key, out cachedValue))
                .Returns(true);

            var controller = new ParameterUploadFileErrorController(mockMemoryCache.Object);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index(key) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.ParameterUploadFileErrorIndex, result.ViewName);
        }
    }
}
