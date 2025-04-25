// <copyright file="CommonUtil.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using EPR.Calculator.Frontend.Constants;
using Microsoft.AspNetCore.Http.HttpResults;

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
        /// Gets the financial year based on the date input.
        /// </summary>
        /// <param name="date">Any date.</param>
        /// <returns>The financial year in the format YYYY-YY.</returns>
        public static string GetFinancialYear(DateTime date)
        {
            var year = date.Year;

            var startYear = date.Month >= 4
                ? year
                : year - 1;

            var endYear = date.Month >= 4
                ? year + 1
                : year;

            return $"{startYear}-{endYear.ToString().Substring(2, 2)}";
        }

        public static DateTime GetDateTime(DateTime date)
        {
            var britishZone = TimeZoneInfo.FindSystemTimeZoneById(CommonConstants.TimeZone);
            return TimeZoneInfo.ConvertTime(date, TimeZoneInfo.Utc, britishZone);
        }

        /// <summary>
        /// Get formatted billing download filename from calculator run id and run name.
        /// </summary>
        /// <param name="runId">Calculator run id.</param>
        /// <param name="runName">Calculator run name.</param>
        /// <param name="dateTime">Date Time.</param>
        /// <returns>filename as a string.</returns>
        public static string GetBillingDownloadFileName(int runId, string runName, DateTime dateTime)
        {
            var datePart = dateTime.ToString("yyyyMMdd");
            return $"{runId}-{runName}_Billing File_{datePart}.csv";
        }
    }
}