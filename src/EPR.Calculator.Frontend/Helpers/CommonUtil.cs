// <copyright file="CommonUtil.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.ViewModels;

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
        public static string GetDefaultFinancialYear(DateTime date)
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
        /// Returns the financial year from session if feature enabled, else from config.
        /// </summary>
        /// <returns>Returns the financial year.</returns>
        /// <exception cref="ArgumentNullException">Returns error if financial year is null or empty.</exception>
        public static string GetFinancialYear(ISession session)
        {
            var parameterYear = session.GetString(SessionConstants.FinancialYear);

            if (string.IsNullOrWhiteSpace(parameterYear))
            {
                parameterYear = CommonUtil.GetDefaultFinancialYear(DateTime.Now);
            }

            return parameterYear;
        }

        /// <summary>
        /// Returns the URL for the back link based on the run details.
        /// </summary>
        /// <param name="runDetails">Run details.</param>
        /// <returns>Back link URL.</returns>
        public static string GetBackLinkUrl(CalculatorRunDetailsViewModel? runDetails = null)
        {
            if (runDetails == null)
            {
                return string.Empty;
            }

            return runDetails.RunClassificationId switch
            {
                RunClassification.UNCLASSIFIED => string.Format(ActionNames.CalculationRunNewDetails, runDetails.RunId),
                RunClassification.INITIAL_RUN when !runDetails.IsBillingFileGenerating ?? false => ControllerNames.ClassifyRunConfirmation,
                RunClassification.INITIAL_RUN when runDetails.IsBillingFileGenerating ?? true => ControllerNames.CalculationRunOverview,
                RunClassification.INITIAL_RUN_COMPLETED => string.Format(ActionNames.PostBillingFile, runDetails.RunId),
                RunClassification.ERROR => ControllerNames.CalculationRunDetails,
                _ => string.Empty,
            };
        }
    }
}
