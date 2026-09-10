using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Html;

namespace EPR.Calculator.Frontend.Extensions;

public static class DateTimeExtensions
{
    private static readonly TimeZoneInfo LondonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

    extension(DateTime dt)
    {
        public DateTimeOffset ToLondonTime()
        {
            var utc = dt.Kind switch
            {
                DateTimeKind.Utc => dt,
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dt, DateTimeKind.Utc),
                _ => dt.ToUniversalTime()
            };

            // This ensures the offset remains correct regardless of the host's configured
            // timezone since TimeZoneInfo.ConvertTime returns an Unspecified DateTime.
            var londonDateTime = TimeZoneInfo.ConvertTimeFromUtc(utc, LondonTimeZone);
            var londonOffset = LondonTimeZone.GetUtcOffset(utc);

            return new DateTimeOffset(londonDateTime, londonOffset);
        }

        [ExcludeFromCodeCoverage]
        public HtmlString DisplayAsDateAtTime()
        {
            return new(dt.ToLondonTime().ToString("dd MMM yyyy 'at' H:mm"));
        }

        [ExcludeFromCodeCoverage]
        public HtmlString DisplayAsTimeOnDate()
        {
            return new(dt.ToLondonTime().ToString("<b>HH:mm</b> 'on' d MMM yyyy"));
        }
    }
}
