namespace EPR.Calculator.Frontend.UnitTests.Helpers
{
    using System;
    using System.Security.Principal;
    using AutoFixture;
    using EPR.Calculator.Frontend.Constants;
    using EPR.Calculator.Frontend.Helpers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CommonUtilTests
    {
        /// <summary>
        /// Checks that the user is returned from the HTTP context when one is set.
        /// </summary>
        [TestMethod]
        public void CanCallGetUserName()
        {
            // Arrange
            var fixture = new Fixture();
            var mockUserName = fixture.Create<string>();
            var context = new Mock<HttpContext>();
            context.Setup(c => c.User.Identity.Name).Returns(mockUserName);

            // Act
            var result = CommonUtil.GetUserName(context.Object);

            // Assert
            Assert.AreEqual(mockUserName, result);
        }

        /// <summary>
        /// Checks that "Unknown User" is returned when the HTTP context doesn't have a user set.
        /// </summary>
        [TestMethod]
        public void CanCallGetUserName_UserNameIsNull()
        {
            // Arrange
            var fixture = new Fixture();
            var context = new Mock<HttpContext>();
            context.Setup(c => c.User.Identity.Name).Returns(default(string));

            // Act
            var result = CommonUtil.GetUserName(context.Object);

            // Assert
            Assert.AreEqual(ErrorMessages.UnknownUser, result);
        }

        [TestMethod]
        public void CanCallGetUserName_IdentityIsNull()
        {
            // Arrange
            var fixture = new Fixture();
            var context = new Mock<HttpContext>();
            context.Setup(c => c.User.Identity).Returns(default(IIdentity));

            // Act
            var result = CommonUtil.GetUserName(context.Object);

            // Assert
            Assert.AreEqual(ErrorMessages.UnknownUser, result);
        }

        [TestMethod]
        [DataRow("2025-05-20", "2025-26")]
        [DataRow("2025-01-20", "2024-25")]
        [DataRow("2024-03-24", "2023-24")]
        public void CanCallGetCurrentFinancialYear(string dateInput, string expectedFinancialYear)
        {
            // Arrange
            var date = DateTime.Parse(dateInput);

            // Act
            var result = CommonUtil.GetFinancialYear(date);

            // Assert
            Assert.AreEqual(expectedFinancialYear, result);
        }
    }
}