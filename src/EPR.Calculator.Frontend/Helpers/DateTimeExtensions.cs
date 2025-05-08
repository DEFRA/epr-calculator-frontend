namespace EPR.Calculator.Frontend.Helpers
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
            var ukTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            var ukTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc), ukTimeZone);
            return ukTime.ToString("dd MMM yyyy 'at' HH:mm");
        }
    }
}