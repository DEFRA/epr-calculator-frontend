// <copyright file="CommonUtil.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using EPR.Calculator.Frontend.Constants;

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
    }
}