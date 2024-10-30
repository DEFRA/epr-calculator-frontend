using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.UnitTests.Common
{
    public static class FileNameTest
    {
        public static void AssignFileName(Controller controller, string fileName)
        {
            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString(SessionConstants.FileName, fileName);

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;
        }
    }
}
