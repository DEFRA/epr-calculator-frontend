namespace EPR.Calculator.Frontend.UnitTests.Helpers
{
    using System;
    using EPR.Calculator.Frontend.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CommonUtilTests
    {
        [TestMethod]
        public void CanCallGetDateTime()
        {
            // Arrange
            var date = DateTime.UtcNow;
            DateTime thisTime = DateTime.Now;
            bool isDaylight = TimeZoneInfo.Local.IsDaylightSavingTime(thisTime);

            var expectedDate = isDaylight ? date.AddHours(1) : date;

            // Act
            var result = CommonUtil.GetDateTime(date);

            // Assert
            Assert.AreEqual(expectedDate, result);
        }
    }
}