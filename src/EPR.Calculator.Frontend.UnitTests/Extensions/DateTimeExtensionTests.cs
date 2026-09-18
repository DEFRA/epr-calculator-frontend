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
            var result = utcDateTime.DisplayAsDateAtTime().ToString();

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
            var result = utcDateTime.DisplayAsDateAtTime().ToString();

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
            var result = unspecifiedDateTime.DisplayAsDateAtTime().ToString();

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ToLondonTime_ShouldReturnBstOffset_WhenUtcDuringBritishSummerTime()
        {
            // Arrange
            var utcDateTime = new DateTime(2023, 7, 1, 12, 0, 0, DateTimeKind.Utc);
            var expected = new DateTimeOffset(2023, 7, 1, 13, 0, 0, TimeSpan.FromHours(1));

            // Act
            var result = utcDateTime.ToLondonTime();

            // Assert
            Assert.AreEqual(expected.Offset, result.Offset);
            Assert.AreEqual(expected.DateTime, result.DateTime);
            Assert.AreEqual(utcDateTime, result.UtcDateTime);
        }

        [TestMethod]
        public void ToLondonTime_ShouldReturnGmtOffset_WhenUtcDuringWinter()
        {
            // Arrange
            var utcDateTime = new DateTime(2023, 12, 1, 12, 0, 0, DateTimeKind.Utc);
            var expected = new DateTimeOffset(2023, 12, 1, 12, 0, 0, TimeSpan.Zero);

            // Act
            var result = utcDateTime.ToLondonTime();

            // Assert
            Assert.AreEqual(expected.Offset, result.Offset);
            Assert.AreEqual(expected.DateTime, result.DateTime);
            Assert.AreEqual(utcDateTime, result.UtcDateTime);
        }

        [TestMethod]
        public void ToLondonTime_ShouldTreatUnspecifiedKindAsUtc()
        {
            // Arrange
            var unspecifiedDateTime = new DateTime(2023, 7, 1, 12, 0, 0, DateTimeKind.Unspecified);
            var expected = new DateTimeOffset(2023, 7, 1, 13, 0, 0, TimeSpan.FromHours(1));

            // Act
            var result = unspecifiedDateTime.ToLondonTime();

            // Assert
            Assert.AreEqual(expected.Offset, result.Offset);
            Assert.AreEqual(expected.DateTime, result.DateTime);
            Assert.AreEqual(DateTime.SpecifyKind(unspecifiedDateTime, DateTimeKind.Utc), result.UtcDateTime);
        }

        [TestMethod]
        public void ToLondonTime_ShouldConvertLocalKind_UsingSystemLocalZoneAsSourceOfTruth()
        {
            // Arrange
            // DateTimeKind.Local is, by definition, interpreted using whichever time zone the
            // executing machine is configured with, so the expectation is computed the same way
            // here rather than hard-coded, keeping this test deterministic on any host/CI agent.
            var localDateTime = new DateTime(2023, 7, 1, 12, 0, 0, DateTimeKind.Local);
            var expectedUtc = localDateTime.ToUniversalTime();
            var londonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
            var expectedOffset = londonTimeZone.GetUtcOffset(expectedUtc);

            // Act
            var result = localDateTime.ToLondonTime();

            // Assert
            Assert.AreEqual(expectedUtc, result.UtcDateTime);
            Assert.AreEqual(expectedOffset, result.Offset);
        }

        [TestMethod]
        public void ToLondonTime_ShouldReturnGmtOffset_JustBeforeSpringForwardTransition()
        {
            // Arrange: UK clocks went forward at 2023-03-26 01:00 UTC (00:59 GMT is the last GMT minute).
            var utcDateTime = new DateTime(2023, 3, 26, 0, 59, 0, DateTimeKind.Utc);
            var expected = new DateTimeOffset(2023, 3, 26, 0, 59, 0, TimeSpan.Zero);

            // Act
            var result = utcDateTime.ToLondonTime();

            // Assert
            Assert.AreEqual(expected.Offset, result.Offset);
            Assert.AreEqual(expected.DateTime, result.DateTime);
        }

        [TestMethod]
        public void ToLondonTime_ShouldReturnBstOffset_AtSpringForwardTransition()
        {
            // Arrange: at/after 2023-03-26 01:00 UTC, clocks are BST (UTC+1).
            var utcDateTime = new DateTime(2023, 3, 26, 1, 0, 0, DateTimeKind.Utc);
            var expected = new DateTimeOffset(2023, 3, 26, 2, 0, 0, TimeSpan.FromHours(1));

            // Act
            var result = utcDateTime.ToLondonTime();

            // Assert
            Assert.AreEqual(expected.Offset, result.Offset);
            Assert.AreEqual(expected.DateTime, result.DateTime);
        }

        [TestMethod]
        public void ToLondonTime_ShouldReturnBstOffset_JustBeforeAutumnTransition()
        {
            // Arrange: UK clocks went back at 2023-10-29 01:00 UTC (01:59 BST is the last BST minute).
            var utcDateTime = new DateTime(2023, 10, 29, 0, 59, 0, DateTimeKind.Utc);
            var expected = new DateTimeOffset(2023, 10, 29, 1, 59, 0, TimeSpan.FromHours(1));

            // Act
            var result = utcDateTime.ToLondonTime();

            // Assert
            Assert.AreEqual(expected.Offset, result.Offset);
            Assert.AreEqual(expected.DateTime, result.DateTime);
        }

        [TestMethod]
        public void ToLondonTime_ShouldReturnGmtOffset_AtAutumnTransition()
        {
            // Arrange: at/after 2023-10-29 01:00 UTC, clocks are back to GMT (UTC+0).
            var utcDateTime = new DateTime(2023, 10, 29, 1, 0, 0, DateTimeKind.Utc);
            var expected = new DateTimeOffset(2023, 10, 29, 1, 0, 0, TimeSpan.Zero);

            // Act
            var result = utcDateTime.ToLondonTime();

            // Assert
            Assert.AreEqual(expected.Offset, result.Offset);
            Assert.AreEqual(expected.DateTime, result.DateTime);
        }
    }
}
