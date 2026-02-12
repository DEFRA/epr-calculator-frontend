namespace EPR.Calculator.Frontend.UnitTests.Helpers
{
    using System;
    using EPR.Calculator.Frontend.Constants;
    using EPR.Calculator.Frontend.Helpers;
    using EPR.Calculator.Frontend.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CommonUtilTests
    {
        // Delegate for Moq Callback to match TryGetValue signature
        private delegate void TryGetValueCallback(string key, out byte[] value);

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
        public void GetRelativeYear_ReturnsYearFromSession_WhenSessionHasValue()
        {
            // Arrange
            var expectedYear = 2024;
            var key = SessionConstants.RelativeYear;

            // Encode int in big-endian order (same as SessionExtensions.SetInt32)
            var intBytes = new byte[]
            {
                (byte)(expectedYear >> 24),
                (byte)((expectedYear >> 16) & 0xFF),
                (byte)((expectedYear >> 8) & 0xFF),
                (byte)(expectedYear & 0xFF)
            };

            var sessionMock = new Mock<ISession>();

            // Setup TryGetValue to return our big-endian byte array
            sessionMock
                .Setup(s => s.TryGetValue(key, out It.Ref<byte[]>.IsAny!))
                .Returns(true)
                .Callback(new TryGetValueCallback((string k, out byte[] v) =>
                {
                    v = intBytes;
                }));

            // Act
            var result = CommonUtil.GetRelativeYear(sessionMock.Object, 4);

            // Assert
            Assert.AreEqual(new RelativeYear(expectedYear), result);
        }

        [TestMethod]
        public void GetRelativeYear_ReturnsDefaultYear_WhenSessionIsEmpty()
        {
            // Arrange
            var key = SessionConstants.RelativeYear;
            byte[]? byteValue;

            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(s => s.TryGetValue(key, out byteValue)).Returns(false);

            var expectedDefault = CommonUtil.GetDefaultRelativeYear(DateTime.UtcNow, 4);

            // Act
            var result = CommonUtil.GetRelativeYear(sessionMock.Object, 4);

            // Assert
            Assert.AreEqual(expectedDefault, result);
        }

        [TestMethod]
        public void GetDefaultRelativeYear()
        {
            var resultBeforeMonth = CommonUtil.GetDefaultRelativeYear(DateTime.Parse("2026-01-20"), 6);
            var resultAfterMonth = CommonUtil.GetDefaultRelativeYear(DateTime.Parse("2026-10-20"), 6);

            // Assert
            Assert.AreEqual(new RelativeYear(2025), resultBeforeMonth);
            Assert.AreEqual(new RelativeYear(2026), resultAfterMonth);
        }

        [TestMethod]
        public void GetRelativeYearStartingMonth()
        {
            IConfiguration BuildConfig(string? value)
            {
                var values = new Dictionary<string, string>();

                if (value != null)
                {
                    values[CommonConstants.RelativeYearStartingMonth] = value;
                }

                return new ConfigurationBuilder().AddInMemoryCollection(values!).Build();
            }

            var validConfig = BuildConfig("4");
            var validResult = CommonUtil.GetRelativeYearStartingMonth(validConfig);
            Assert.AreEqual(4, validResult);

            var missingConfig = BuildConfig(null);
            var missingEx = Assert.ThrowsException<InvalidOperationException>(() => CommonUtil.GetRelativeYearStartingMonth(missingConfig));
            Assert.AreEqual("RelativeYearStartingMonth configuration is missing", missingEx.Message);

            var invalidConfig = BuildConfig("13");
            var invalidEx = Assert.ThrowsException<InvalidOperationException>(() => CommonUtil.GetRelativeYearStartingMonth(invalidConfig));
            Assert.AreEqual("RelativeYearStartingMonth must be between 1 and 12", invalidEx.Message);
        }
    }
}