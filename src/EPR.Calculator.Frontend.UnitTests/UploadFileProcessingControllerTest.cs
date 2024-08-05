using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class UploadFileProcessingControllerTest
    {
        [TestMethod]
        public void UploadFileProcessingController_Success_Result_Test()
        {
            var controller = new UploadFileProcessingController(GetConfigurationValues());
            var result = controller.Index(MockData.GetSchemeParameterTemplateValues().ToList()) as OkObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(result.StatusCode, 201);
        }

        [TestMethod]
        public void UploadFileProcessingController_Failure_Result_Test()
        {
            var controller = new UploadFileProcessingController(GetConfigurationValues());
            var result = controller.Index(MockData.GetSchemeParameterTemplateValues().ToList()) as BadRequestObjectResult;
            Assert.IsNotNull(result);
            Assert.AreNotEqual(result.StatusCode, 201);
        }

        private IConfiguration GetConfigurationValues()
        {
            string projectPath = AppDomain.CurrentDomain.BaseDirectory.Split(new string[] { @"bin\" }, StringSplitOptions.None)[0];
            IConfiguration config = new ConfigurationBuilder()
               .SetBasePath(projectPath)
               .AddJsonFile("appsettings.Test.json")
               .Build();

            return config;
        }
    }
}
