using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;

namespace EPR.Calculator.Frontend.UnitTests.Helpers
{
    [TestClass]
    public class CommonUtilTests
    {
        public CommonUtilTests()
        {
            this.Fixture = new Fixture();
        }

        private Fixture Fixture { get; init; }

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
        public void GetBillingDownloadFileNameTest()
        {
            int runId = this.Fixture.Create<int>();
            var runName = this.Fixture.Create<string>();
            var dateTime = DateTime.UtcNow;
            var dateTimePart = dateTime.ToString("yyyyMMdd");

            var resultString = CommonUtil.GetBillingDownloadFileName(runId, runName, dateTime);
            Assert.AreEqual($"{runId}-{runName}_Billing File_{dateTimePart}.csv", resultString);
        }
    }
}