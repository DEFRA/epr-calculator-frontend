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
    public class UploadCSVErrorControllerTest
    {
        [TestMethod]
        public void UploadCSVErrorController_View_Test()
        {
            var errors = new List<CreateDefaultParameterSettingErrorDto>();
            errors.AddRange([
                new CreateDefaultParameterSettingErrorDto { Message = "Parameter Unique reference is incorrect", Description = string.Empty },
                new CreateDefaultParameterSettingErrorDto { Message = "Parameter value is incorrect", Description = string.Empty }
            ]);

            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString("Default_Parameter_Upload_Errors", JsonConvert.SerializeObject(errors));

            var controller = new UploadCSVErrorController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.UploadCSVErrorIndex, result.ViewName);
        }

        [TestMethod]
        public void UploadCSVErrorController_View_Global_Validation_Test()
        {
            var errors = new List<ValidationErrorDto>();
            errors.AddRange([
                new ValidationErrorDto { ErrorMessage = "Parameter Unique reference is incorrect", Exception = string.Empty },
                new ValidationErrorDto { ErrorMessage = "Parameter value is incorrect", Exception = string.Empty }
            ]);

            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString("Default_Parameter_Upload_Errors", JsonConvert.SerializeObject(errors));

            var controller = new UploadCSVErrorController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.UploadCSVErrorIndex, result.ViewName);
        }

        public void UploadCSVErrorController_Standard_Error_Test()
        {
            var mockHttpSession = new MockHttpSession();

            var controller = new UploadCSVErrorController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index() as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public void UploadCSVErrorController_No_Error_Messages_Test()
        {
            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString("Default_Parameter_Upload_Errors", string.Empty);

            var controller = new UploadCSVErrorController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index() as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public void UploadCsvErrorController_Upload_Test()
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

            var controller = new UploadCSVErrorController()
            {
                TempData = tempData
            };

            var result = controller.Upload(file);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UploadCsvErrorController_Post_Returns_Ok_Test()
        {
            var mockHttpSession = new MockHttpSession();

            var controller = new UploadCSVErrorController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index("some errors") as OkResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [TestMethod]
        public async Task UploadCSVErrorController_Upload_View_Post_Incorrect_File_Extension_Error_Test()
        {
            var content = string.Empty;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, string.Empty, "SchemeParameters.xlsx");

            var controller = new UploadCSVErrorController();
            var result = await controller.Upload(file) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.UploadCSVErrorIndex, result.ViewName);
        }

        [TestMethod]
        public async Task UploadCSVErrorController_Upload_View_Post_Test()
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
            tempData["Default_Parameter_Upload_Errors"] = string.Empty;

            var controller = new UploadCSVErrorController()
            {
                TempData = tempData
            };

            var result = await controller.Upload(file) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.UploadFileRefresh, result.ViewName);
        }
    }
}
