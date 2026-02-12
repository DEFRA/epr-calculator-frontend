// <copyright file="CommonUtil.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.Helpers
{
    public static class CommonUtil
    {
        public const string Controller = "Controller";

        public static string GetControllerName(Type controllerType)
        {
            string contollerName = controllerType.Name;
            if (contollerName.EndsWith(Controller, StringComparison.OrdinalIgnoreCase))
            {
                contollerName = contollerName.Remove(contollerName.Length - Controller.Length, Controller.Length);
            }

            return contollerName;
        }

        /// <summary>
        /// Gets the name of the currently logged in user from an HTTP context.
        /// </summary>
        /// <param name="context">The HTTP context to get the user from.</param>
        /// <returns>
        /// The user name, or <see cref="ErrorMessages.UnknownUser"/> if no user is logged in.
        /// </returns>
        public static string GetUserName(HttpContext context)
            => context.User.Identity?.Name ?? ErrorMessages.UnknownUser;

        /// <summary>
        /// Gets the relative year based on the date input.
        /// </summary>
        /// <param name="date">Any date.</param>
        /// <param name="relativeYearStartingMonth">Month from which the relative year starts.</param>
        /// <returns>The relative year.</returns>
        public static RelativeYear GetDefaultRelativeYear(DateTime date, int relativeYearStartingMonth)
        {
            return date.Month >= relativeYearStartingMonth
                ? new RelativeYear(date.Year)
                : new RelativeYear(date.Year - 1);
        }

        public static DateTime GetDateTime(DateTime date)
        {
            var britishZone = TimeZoneInfo.FindSystemTimeZoneById(CommonConstants.TimeZone);
            return TimeZoneInfo.ConvertTime(date, TimeZoneInfo.Utc, britishZone);
        }

        /// <summary>
        /// Returns the relative year from session if feature enabled, else from config.
        /// </summary>
        /// <returns>Returns the relative year.</returns>
        /// <exception cref="ArgumentNullException">Returns error if relative year is null or empty.</exception>
        public static RelativeYear GetRelativeYear(ISession session, int startingMonth)
        {
            var yearValue = session.GetInt32(SessionConstants.RelativeYear);
            return yearValue.HasValue
                ? new RelativeYear(yearValue.Value)
                : GetDefaultRelativeYear(DateTime.UtcNow, startingMonth);
        }

        public static int GetRelativeYearStartingMonth(IConfiguration configuration)
        {
            var value = configuration[CommonConstants.RelativeYearStartingMonth]
                ?? throw new InvalidOperationException(
                    "RelativeYearStartingMonth configuration is missing");

            if (!int.TryParse(value, out var month) || month is < 1 or > 12)
            {
                throw new InvalidOperationException(
                    "RelativeYearStartingMonth must be between 1 and 12");
            }

            return month;
        }
    }
}
