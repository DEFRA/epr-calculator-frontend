﻿using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class DeleteConfirmationController : Controller
    {
        public IActionResult Index()
        {
            return this.View();
        }
    }
}
