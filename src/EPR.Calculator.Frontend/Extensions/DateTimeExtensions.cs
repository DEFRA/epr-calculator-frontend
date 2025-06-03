using EPR.Calculator.Frontend.Constants;

namespace EPR.Calculator.Frontend.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts a UTC DateTime to a UK DateTime string format.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToUKDateTimeDisplay(this DateTime dateTime)
        {
            // Get UK time zone, which automatically adjusts for BST
            var britishZone = TimeZoneInfo.FindSystemTimeZoneById(CommonConstants.TimeZone);

            // Ensure the input is treated as UTC and  // Convert to UK local time (BST or GMT depending on the date)
            var britishTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc), britishZone);

            // Format the date and time
            return britishTime.ToString($"{CommonConstants.DateFormat} 'at' {CommonConstants.TimeFormat}");
        }

        /// <summary>
        /// Converts a UTC DateTime to a UK DateTime string format.
        /// </summary>
        /// <param name="dateTime">Nullable DateTime.</param>
        /// <returns>Formatted date in string.</returns>
        public static string ToUKDateTimeDisplay(this DateTime? dateTime)
        {
            if (dateTime is null)
            {
                return string.Empty;
            }

            return ((DateTime)dateTime).ToUKDateTimeDisplay();
        }
    }
}