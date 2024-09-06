using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class UploadFileControllerTest
    {
        [TestMethod]
        public void UploadFileController_View_Test()
        {
            var controller = new UploadFileController();
            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.UploadFileIndex, result.ViewName);
        }

        [TestMethod]
        public async Task UploadFileController_Upload_View_File_Valid_Test()
        {
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            tempData["FilePath"] = Directory.GetCurrentDirectory() + "/Mocks/SchemeParameters.csv";

            var controller = new UploadFileController()
            {
                TempData = tempData
            };

            var result = await controller.Upload() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.UploadFileRefresh, result.ViewName);
        }

        [TestMethod]
        public async Task UploadFileController_Upload_Incorrect_File_Test()
        {
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            tempData["FilePath"] = Directory.GetCurrentDirectory() + "/Mocks/SchemeParameters.txt";

            var controller = new UploadFileController()
            {
                TempData = tempData
            };

            var result = await controller.Upload() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.UploadFileIndex, result.ViewName);
        }

        [TestMethod]
        public async Task UploadFileController_Upload_View_No_File_Test()
        {
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            tempData["FilePath"] = null;

            var controller = new UploadFileController()
            {
                TempData = tempData
            };

            var result = await controller.Upload() as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public async Task UploadFileController_Upload_View_File_Process_Error_Test()
        {
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            tempData["FilePath"] = "some random file location";

            var controller = new UploadFileController()
            {
                TempData = tempData
            };

            var result = await controller.Upload() as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public async Task UploadFileController_Upload_View_Post_Test()
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

            var controller = new UploadFileController()
            {
                TempData = tempData
            };

            var result = await controller.Upload(file) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.UploadFileRefresh, result.ViewName);
        }

        [TestMethod]
        public async Task UploadFileController_Upload_View_Post_Error_Test()
        {
            var content = string.Empty;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, string.Empty, "SchemeParameters.csv");

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            tempData["Default_Parameter_Upload_Errors"] = string.Empty;

            var controller = new UploadFileController()
            {
                TempData = tempData
            };

            var result = await controller.Upload(file) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.UploadFileRefresh, result.ViewName);
        }

        [TestMethod]
        public async Task UploadFileController_Upload_View_Post_Incorrect_File_Error_Test()
        {
            var content = MockData.GetSchemeParametersFileContent();
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, string.Empty, "SchemeParameters.txt");

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            tempData["Default_Parameter_Upload_Errors"] = string.Empty;

            var controller = new UploadFileController()
            {
                TempData = tempData
            };

            var result = await controller.Upload(file) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.UploadFileIndex, result.ViewName);
        }

        [TestMethod]
        public async Task UploadFileController_Upload_View_Post_Incorrect_File_Extension_Error_Test()
        {
            var content = string.Empty;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, string.Empty, "SchemeParameters.xlsx");

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            tempData["Default_Parameter_Upload_Errors"] = string.Empty;

            var controller = new UploadFileController()
            {
                TempData = tempData
            };

            var result = await controller.Upload(file) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.UploadFileIndex, result.ViewName);
        }

        [TestMethod]
        public void UploadFileController_DownloadCSVTemplate_Test()
        {
            var controller = new UploadFileController();
            var result = controller.DownloadCsvTemplate() as PhysicalFileResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("SchemeParameterTemplate.v0.1.xlsx", result.FileDownloadName);
        }
    }
}