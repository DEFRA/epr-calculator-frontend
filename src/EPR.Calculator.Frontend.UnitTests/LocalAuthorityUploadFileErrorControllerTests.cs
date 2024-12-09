using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class LocalAuthorityUploadFileErrorControllerTests
    {
        public LocalAuthorityUploadFileErrorControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
        }

        private Fixture Fixture { get; init; }

        private Mock<HttpContext> MockHttpContext { get; init; }

        [TestMethod]
        public void LocalAuthorityUploadFileErrorController_View_Test()
        {
            var errors = new List<CreateDefaultParameterSettingErrorDto>();
            errors.AddRange([
                new CreateDefaultParameterSettingErrorDto { Message = "Parameter Unique reference is incorrect", Description = string.Empty },
                new CreateDefaultParameterSettingErrorDto { Message = "Parameter value is incorrect", Description = string.Empty }
            ]);

            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString(UploadFileErrorIds.LocalAuthorityUploadErrors, JsonConvert.SerializeObject(errors));

            var controller = new LocalAuthorityUploadFileErrorController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.LocalAuthorityUploadFileErrorIndex, result.ViewName);
        }

        [TestMethod]
        public void LocalAuthorityUploadFileErrorController_View_Global_Validation_Test()
        {
            var errors = new List<ValidationErrorDto>();
            errors.AddRange([
                new ValidationErrorDto { ErrorMessage = "Parameter Unique reference is incorrect", Exception = string.Empty },
                new ValidationErrorDto { ErrorMessage = "Parameter value is incorrect", Exception = string.Empty }
            ]);

            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString(UploadFileErrorIds.LocalAuthorityUploadErrors, JsonConvert.SerializeObject(errors));

            var controller = new LocalAuthorityUploadFileErrorController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.LocalAuthorityUploadFileErrorIndex, result.ViewName);
        }

        [TestMethod]
        public void LocalAuthorityUploadFileErrorController_Standard_Error_Test()
        {
            var mockHttpSession = new MockHttpSession();

            var controller = new LocalAuthorityUploadFileErrorController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index() as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public void LocalAuthorityUploadFileErrorController_No_Error_Messages_Test()
        {
            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString(UploadFileErrorIds.LocalAuthorityUploadErrors, string.Empty);

            var controller = new LocalAuthorityUploadFileErrorController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index() as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public void LocalAuthorityUploadFileErrorController_Upload_Test()
        {
            var content = MockData.GetSchemeParametersFileContent();
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, string.Empty, "LocalAuthorityData.csv");

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            tempData["LapcapFilePath"] = "some random file location";

            var controller = new LocalAuthorityUploadFileErrorController()
            {
                TempData = tempData
            };

            var result = controller.Upload(file);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void LocalAuthorityUploadFileErrorController_Post_Returns_Ok_Test()
        {
            var mockHttpSession = new MockHttpSession();

            var controller = new LocalAuthorityUploadFileErrorController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index("some errors") as OkResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [TestMethod]
        public async Task LocalAuthorityUploadFileErrorController_Upload_View_Post_Incorrect_File_Extension_Error_Test()
        {
            var content = string.Empty;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, string.Empty, "LocalAuthorityData.xlsx");

            var controller = new LocalAuthorityUploadFileErrorController();
            controller.ControllerContext.HttpContext = this.MockHttpContext.Object;

            var result = await controller.Upload(file) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.LocalAuthorityUploadFileErrorIndex, result.ViewName);
        }

        [TestMethod]
        public async Task LocalAuthorityUploadFileErrorController_Upload_View_Post_Test()
        {
            var content = MockData.GetSchemeParametersFileContent();
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, string.Empty, "LocalAuthorityData.csv");

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            tempData[UploadFileErrorIds.LocalAuthorityUploadErrors] = string.Empty;

            var controller = new LocalAuthorityUploadFileErrorController()
            {
                TempData = tempData
            };
            controller.ControllerContext.HttpContext = this.MockHttpContext.Object;

            var result = await controller.Upload(file) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.LocalAuthorityUploadFileRefresh, result.ViewName);
        }

        [TestMethod]
        public async Task LocalAuthorityUploadFileErrorController_View_Get_Test()
        {
            var mockHttpSession = new MockHttpSession();

            var errors = new List<ValidationErrorDto>() { new ValidationErrorDto { ErrorMessage = ErrorMessages.FileMustBeCSV } };
            mockHttpSession.SetString(UploadFileErrorIds.LocalAuthorityUploadErrors, JsonConvert.SerializeObject(errors).ToString());

            var controller = new LocalAuthorityUploadFileErrorController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.LocalAuthorityUploadFileErrorIndex, result.ViewName);
        }

        [TestMethod]
        public async Task LocalAuthorityUploadFileErrorController_View_Get_Error_Test()
        {
            var mockHttpSession = new MockHttpSession();

            var errors = new List<CreateDefaultParameterSettingErrorDto>() { new CreateDefaultParameterSettingErrorDto { Message = "Some message" } };
            mockHttpSession.SetString(UploadFileErrorIds.LocalAuthorityUploadErrors, JsonConvert.SerializeObject(errors).ToString());

            var controller = new LocalAuthorityUploadFileErrorController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.LocalAuthorityUploadFileErrorIndex, result.ViewName);
        }
    }
}
