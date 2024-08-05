using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class UploadCSVErrorControllerTest
    {
        [TestMethod]
        public void UploadCSVErrorController_View_Test()
        {
            var errors = new List<ErrorDto>();
            errors.AddRange([
                new ErrorDto { Message = "Parameter Unique reference is incorrect", Description = string.Empty },
                new ErrorDto { Message = "Parameter value is incorrect", Description = string.Empty }
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
    }
}
