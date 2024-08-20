﻿using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class LocalAuthorityDisposalCostsControllerTests
    {
        [TestMethod]
        public void LocalAuthorityDisposalCostsController_View_Test()
        {
            var controller = new LocalAuthorityDisposalCostsController();
            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.LocalAuthorityDisposalCostsIndex, result.ViewName);
        }
    }
}