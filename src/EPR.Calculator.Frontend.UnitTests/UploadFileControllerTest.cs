using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public void UploadFileController_Upload_View_Test()
        {
            var content = MockData.GetSchemeParametersFileContent();
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, string.Empty, "SchemeParameters.csv");

            var controller = new UploadFileController();
            var result = controller.Upload(file) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.UploadFileRefresh, result.ViewName);
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