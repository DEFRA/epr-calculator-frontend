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
            var britishZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            var britishTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc), britishZone);
            return britishTime.ToString("dd MMM yyyy 'at' HH:mm");
        }
    }
}