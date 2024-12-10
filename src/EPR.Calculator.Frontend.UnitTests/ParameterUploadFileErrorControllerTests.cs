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
    public class ParameterUploadFileErrorControllerTests
    {
        [TestMethod]
        public void ParameterUploadCSVErrorController_View_Test()
        {
            var errors = new List<CreateDefaultParameterSettingErrorDto>();
            errors.AddRange([
                new CreateDefaultParameterSettingErrorDto { Message = "Parameter Unique reference is incorrect", Description = string.Empty },
                new CreateDefaultParameterSettingErrorDto { Message = "Parameter value is incorrect", Description = string.Empty }
            ]);

            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString(UploadFileErrorIds.DefaultParameterUploadErrors, JsonConvert.SerializeObject(errors));

            var controller = new ParameterUploadFileErrorController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.ParameterUploadFileErrorIndex, result.ViewName);
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

            var controller = new ParameterUploadFileErrorController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.ParameterUploadFileErrorIndex, result.ViewName);
        }

        [TestMethod]
        public void ParameterUploadCSVErrorController_Standard_Error_Test()
        {
            var mockHttpSession = new MockHttpSession();

            var controller = new ParameterUploadFileErrorController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index() as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public void ParameterUploadCSVErrorController_No_Error_Messages_Test()
        {
            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString(UploadFileErrorIds.DefaultParameterUploadErrors, string.Empty);

            var controller = new ParameterUploadFileErrorController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index() as RedirectToActionResult;
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

            var controller = new ParameterUploadFileErrorController()
            {
                TempData = tempData
            };

            var result = controller.Upload(file);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ParameterUploadCsvErrorController_Post_Returns_Ok_Test()
        {
            var mockHttpSession = new MockHttpSession();

            var controller = new ParameterUploadFileErrorController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index("some errors") as OkResult;
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

            var controller = new ParameterUploadFileErrorController();
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

            var controller = new ParameterUploadFileErrorController()
            {
                TempData = tempData
            };

            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            var result = await controller.Upload(file) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.ParameterUploadFileRefresh, result.ViewName);
        }

        [TestMethod]
        public async Task ParameterUploadCSVErrorController_View_Get_Test()
        {
            var mockHttpSession = new MockHttpSession();

            var errors = new List<ValidationErrorDto>() { new ValidationErrorDto { ErrorMessage = ErrorMessages.FileMustBeCSV } };
            mockHttpSession.SetString(UploadFileErrorIds.DefaultParameterUploadErrors, JsonConvert.SerializeObject(errors).ToString());

            var controller = new ParameterUploadFileErrorController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.ParameterUploadFileErrorIndex, result.ViewName);
        }

        [TestMethod]
        public async Task ParameterUploadCSVErrorController_View_Get_Error_Test()
        {
            var mockHttpSession = new MockHttpSession();

            var errors = new List<CreateDefaultParameterSettingErrorDto>() { new CreateDefaultParameterSettingErrorDto { Message = "Some message" } };
            mockHttpSession.SetString(UploadFileErrorIds.DefaultParameterUploadErrors, JsonConvert.SerializeObject(errors).ToString());

            var controller = new ParameterUploadFileErrorController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.ParameterUploadFileErrorIndex, result.ViewName);
        }
    }
}
