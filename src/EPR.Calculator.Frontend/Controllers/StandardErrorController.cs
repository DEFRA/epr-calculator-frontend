﻿using EPR.Calculator.Frontend.Constants;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class StandardErrorController : Controller
    {
        public IActionResult Index()
        {
            return this.View(ViewNames.StandardErrorIndex);
        }
    }
}
