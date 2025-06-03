using EPR.Calculator.Frontend.Extensions;

namespace EPR.Calculator.Frontend.UnitTests.Extensions
{
    [TestClass]
    public class DateTimeExtensionTests
    {
        [TestMethod]
        public void ToUKDateTimeDisplay_ShouldConvertUtcToUKTime()
        {
            // Arrange
            var utcDateTime = new DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc);
            var expected = "01 Oct 2023 at 13:00"; // UK is in BST (UTC+1) during this date.

            // Act
            var result = utcDateTime.ToUKDateTimeDisplay();

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ToUKDateTimeDisplay_ShouldHandleWinterTime()
        {
            // Arrange
            var utcDateTime = new DateTime(2023, 12, 1, 12, 0, 0, DateTimeKind.Utc);
            var expected = "01 Dec 2023 at 12:00"; // UK is in GMT (UTC+0) during this date.

            // Act
            var result = utcDateTime.ToUKDateTimeDisplay();

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ToUKDateTimeDisplay_ShouldHandleDateTimeKindUnspecified()
        {
            // Arrange
            var unspecifiedDateTime = new DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Unspecified);
            var expected = "01 Oct 2023 at 13:00"; // Assume input is UTC.

            // Act
            var result = unspecifiedDateTime.ToUKDateTimeDisplay();

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ToUKDateTimeDisplay_NullDateTime_ReturnsEmptyString()
        {
            DateTime? nullableDate = null;
            string result = nullableDate.ToUKDateTimeDisplay();
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void ToUKDateTimeDisplay_ValidUtcDateTime_ConvertsCorrectly()
        {
            // Arrange
            DateTime utcDate = new DateTime(2025, 6, 3, 14, 0, 0, DateTimeKind.Utc);
            string expectedResult = utcDate.ToUKDateTimeDisplay();

            // Act
            string result = ((DateTime?)utcDate).ToUKDateTimeDisplay();

            // Assert
            Assert.AreEqual(expectedResult, result);
        }
    }
}