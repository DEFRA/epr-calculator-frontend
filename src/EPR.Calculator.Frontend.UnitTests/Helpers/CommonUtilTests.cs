namespace EPR.Calculator.Frontend.UnitTests.Helpers
{
    using System;
    using System.Text;
    using EPR.Calculator.Frontend.Constants;
    using EPR.Calculator.Frontend.Helpers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
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
            DateTime thisTime = DateTime.UtcNow;
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
            var result = CommonUtil.GetFinancialYear(sessionMock.Object, 4);

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

            var expectedDefault = CommonUtil.GetDefaultFinancialYear(DateTime.UtcNow, 4);

            // Act
            var result = CommonUtil.GetFinancialYear(sessionMock.Object, 4);

            // Assert
            Assert.AreEqual(expectedDefault, result);
        }

        [TestMethod]
        public void GetDefaultFinancialYear()
        {
            var resultBeforeMonth = CommonUtil.GetDefaultFinancialYear(DateTime.Parse("2026-01-20"), 6);
            var resultAfterMonth = CommonUtil.GetDefaultFinancialYear(DateTime.Parse("2026-10-20"), 6);

            // Assert
            Assert.AreEqual("2025-26", resultBeforeMonth);
            Assert.AreEqual("2026-27", resultAfterMonth);
        }

        [TestMethod]
        public void GetFinancialYearStartingMonth()
        {
            IConfiguration BuildConfig(string value)
            {
                var values = new Dictionary<string, string>();

                if (value != null)
                {
                    values[CommonConstants.FinancialYearStartingMonth] = value;
                }

                return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
            }

            var validConfig = BuildConfig("4");
            var validResult = CommonUtil.GetFinancialYearStartingMonth(validConfig);
            Assert.AreEqual(4, validResult);

            var missingConfig = BuildConfig(null);
            var missingEx = Assert.ThrowsException<InvalidOperationException>(() => CommonUtil.GetFinancialYearStartingMonth(missingConfig));
            Assert.AreEqual("FinancialYearStartingMonth configuration is missing", missingEx.Message);

            var invalidConfig = BuildConfig("13");
            var invalidEx = Assert.ThrowsException<InvalidOperationException>(() => CommonUtil.GetFinancialYearStartingMonth(invalidConfig));
            Assert.AreEqual("FinancialYearStartingMonth must be between 1 and 12", invalidEx.Message);
        }
    }
}