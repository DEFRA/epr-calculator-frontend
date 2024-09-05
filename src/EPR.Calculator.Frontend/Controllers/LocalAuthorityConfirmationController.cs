﻿using EPR.Calculator.Frontend.Constants;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class LocalAuthorityConfirmationController : Controller
    {
        public IActionResult Index()
        {
            return View(ViewNames.LocalAuthorityConfirmationIndex);
        }
    }
}