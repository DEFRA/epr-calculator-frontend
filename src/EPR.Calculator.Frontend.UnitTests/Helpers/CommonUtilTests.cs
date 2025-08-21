namespace EPR.Calculator.Frontend.UnitTests.Helpers
{
    using System;
    using System.Text;
    using EPR.Calculator.Frontend.Constants;
    using EPR.Calculator.Frontend.Helpers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CommonUtilTests
    {
        [TestMethod]
        public void CanCallGetDateTime()
        {
            // Arrange
            var date = DateTime.UtcNow;
            DateTime thisTime = DateTime.Now;
            bool isDaylight = TimeZoneInfo.FindSystemTimeZoneById(CommonConstants.TimeZone).IsDaylightSavingTime(thisTime);

            var expectedDate = isDaylight ? date.AddHours(1) : date;

            // Act
            var result = CommonUtil.GetDateTime(date);

            // Assert
            Assert.AreEqual(expectedDate, result);
        }

        [TestMethod]
        public void GetFinancialYear_ReturnsYearFromSession_WhenSessionHasValue()
        {
            // Arrange
            var expectedYear = "2024-2025";
            var key = SessionConstants.FinancialYear;
            var byteValue = Encoding.UTF8.GetBytes(expectedYear);

            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(s => s.TryGetValue(key, out byteValue)).Returns(true);

            // Act
            var result = CommonUtil.GetFinancialYear(sessionMock.Object);

            // Assert
            Assert.AreEqual(expectedYear, result);
        }

        [TestMethod]
        public void GetFinancialYear_ReturnsDefaultYear_WhenSessionIsEmpty()
        {
            // Arrange
            var key = SessionConstants.FinancialYear;
            byte[] byteValue;

            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(s => s.TryGetValue(key, out byteValue)).Returns(false);

            var expectedDefault = CommonUtil.GetDefaultFinancialYear(DateTime.Now);

            // Act
            var result = CommonUtil.GetFinancialYear(sessionMock.Object);

            // Assert
            Assert.AreEqual(expectedDefault, result);
        }
    }
}