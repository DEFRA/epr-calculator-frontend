// <copyright file="CommonUntil.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace EPR.Calculator.Frontend.Helpers
{
    using EPR.Calculator.Frontend.Controllers;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CodeAnalysis.Elfie.Extensions;

    public static class CommonUntil
    {
        public static string GetControllerName(Type controllerType)
        {
            string contollerName = controllerType.Name;

            if (contollerName.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
            {
                contollerName = contollerName.Remove(contollerName.Length - 10, 10);
            }

            return contollerName;
        }
    }
}
