﻿using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    public class UploadCSVErrorController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Errors = JsonConvert.DeserializeObject<List<ErrorViewModel>>(TempData["Errors"].ToString());
            return View();
        }
    }
}
