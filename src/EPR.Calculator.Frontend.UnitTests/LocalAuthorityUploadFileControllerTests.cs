using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class LocalAuthorityUploadFileControllerTests
    {
        [TestMethod]
        public void LocalAuthorityUploadFileController_View_Test()
        {
            var controller = new LocalAuthorityUploadFileController();
            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.LocalAuthorityUploadFileIndex, result.ViewName);
        }

        [TestMethod]
        public async Task LocalAuthorityUploadFileController_Upload_View_File_Valid_Test()
        {
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            tempData["LapcapFilePath"] = Directory.GetCurrentDirectory() + "/Mocks/LocalAuthorityData.csv";
            tempData["LapcapFileName"] = "LocalAuthorityData.csv";

            var controller = new LocalAuthorityUploadFileController()
            {
                TempData = tempData
            };

            var result = await controller.Upload() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.LocalAuthorityUploadFileRefresh, result.ViewName);
            Assert.AreEqual(result.TempData["LapcapFileName"].ToString(), "LocalAuthorityData.csv");
        }

        [TestMethod]
        public async Task LocalAuthorityUploadFileController_Upload_Incorrect_File_Test()
        {
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            tempData["LapcapFilePath"] = Directory.GetCurrentDirectory() + "/Mocks/SchemeParameters.txt";

            var controller = new LocalAuthorityUploadFileController()
            {
                TempData = tempData
            };

            var result = await controller.Upload() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.LocalAuthorityUploadFileIndex, result.ViewName);
            Assert.IsNull(result.TempData["LapcapFileName"]);
        }

        [TestMethod]
        public async Task LocalAuthorityUploadFileController_Upload_View_No_File_Test()
        {
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            tempData["LapcapFilePath"] = null;

            var controller = new LocalAuthorityUploadFileController()
            {
                TempData = tempData
            };

            var result = await controller.Upload() as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public async Task LocalAuthorityUploadFileController_Upload_View_File_Process_Error_Test()
        {
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            tempData["LapcapFilePath"] = "some random file location";

            var controller = new LocalAuthorityUploadFileController()
            {
                TempData = tempData
            };

            var result = await controller.Upload() as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public async Task LocalAuthorityUploadFileController_Upload_View_Post_Test()
        {
            var content = MockData.GetLocalAuthorityDisposalCosts();
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            var file = new FormFile(stream, 0, stream.Length, string.Empty, "LocalAuthorityData.csv");

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            tempData[UploadFileErrorIds.LocalAuthorityUploadErrors] = string.Empty;
            tempData["LapcapFileName"] = "LocalAuthorityData.csv";

            var controller = new LocalAuthorityUploadFileController()
            {
                TempData = tempData
            };

            var result = await controller.Upload(file) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.LocalAuthorityUploadFileRefresh, result.ViewName);
        }

        [TestMethod]
        public async Task LocalAuthorityUploadFileController_Upload_View_Post_Error_Test()
        {
            var content = string.Empty;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            var file = new FormFile(stream, 0, stream.Length, string.Empty, "LocalAuthorityData.csv");

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            tempData[UploadFileErrorIds.LocalAuthorityUploadErrors] = string.Empty;
            tempData["LapcapFileName"] = "LocalAuthorityData.csv";

            var controller = new LocalAuthorityUploadFileController()
            {
                TempData = tempData
            };

            var result = await controller.Upload(file) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.LocalAuthorityUploadFileRefresh, result.ViewName);
        }

        [TestMethod]
        public async Task LocalAuthorityUploadFileController_Upload_View_Post_Incorrect_File_Error_Test()
        {
            var content = MockData.GetLocalAuthorityDisposalCosts();
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            var file = new FormFile(stream, 0, stream.Length, string.Empty, "LocalAuthorityData.txt");

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            tempData[UploadFileErrorIds.LocalAuthorityUploadErrors] = string.Empty;

            var controller = new LocalAuthorityUploadFileController()
            {
                TempData = tempData
            };

            var result = await controller.Upload(file) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.LocalAuthorityUploadFileIndex, result.ViewName);
        }

        [TestMethod]
        public async Task LocalAuthorityUploadFileController_Upload_View_Post_Incorrect_File_Extension_Error_Test()
        {
            var content = string.Empty;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            var file = new FormFile(stream, 0, stream.Length, string.Empty, "LocalAuthorityData.xlsx");

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            tempData[UploadFileErrorIds.LocalAuthorityUploadErrors] = string.Empty;

            var controller = new LocalAuthorityUploadFileController()
            {
                TempData = tempData
            };

            var result = await controller.Upload(file) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.LocalAuthorityUploadFileIndex, result.ViewName);
        }

        [TestMethod]
        public async Task LocalAuthorityUploadFileController_ModelState_Clear_Test()
        {
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            tempData["LapcapFilePath"] = Directory.GetCurrentDirectory() + "/Mocks/LocalAuthorityData.csv";
            tempData["LapcapFileName"] = "LocalAuthorityData.csv";

            var controller = new LocalAuthorityUploadFileController()
            {
                TempData = tempData
            };

            // Arrange
            controller.ModelState.AddModelError("key", "Paper value is missing");

            // Act
            Assert.IsFalse(controller.ModelState.IsValid);

            await controller.Upload();
            // Assert
            Assert.IsTrue(controller.ModelState.IsValid);
        }
    }
}