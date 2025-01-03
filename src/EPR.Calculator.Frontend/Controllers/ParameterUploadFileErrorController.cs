﻿using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    [Authorize(Roles = "SASuperUser")]
    public class ParameterUploadFileErrorController : Controller
    {
        [Authorize(Roles = "SASuperUser")]
        public IActionResult Index()
        {
            try
            {
                var errors = this.HttpContext.Session.GetString(UploadFileErrorIds.DefaultParameterUploadErrors);

                if (!string.IsNullOrEmpty(errors))
                {
                    var validationErrors = JsonConvert.DeserializeObject<List<ValidationErrorDto>>(errors);

                    if (validationErrors?.Find(error => !string.IsNullOrEmpty(error.ErrorMessage)) != null)
                    {
                        this.ViewBag.ValidationErrors = validationErrors;
                    }
                    else
                    {
                        this.ViewBag.Errors = JsonConvert.DeserializeObject<List<CreateDefaultParameterSettingErrorDto>>(errors);
                    }

                    if (this.ViewBag.ValidationErrors is null && this.ViewBag.Errors is not null)
                    {
                        this.ViewBag.ValidationErrors = new List<ValidationErrorDto>()
                        {
                            new ValidationErrorDto()
                            {
                                ErrorMessage = this.ViewBag.Errors.Count > 1 ? $"The file contained {this.ViewBag.Errors.Count} errors." : $"The file contained {this.ViewBag.Errors.Count} error.",
                            },
                        };
                    }

                    return this.View(
                        ViewNames.ParameterUploadFileErrorIndex,
                        new ViewModelCommonData
                            {
                                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                            });
                }
                else
                {
                    return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
                }
            }
            catch (Exception)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        [HttpPost]
        [Authorize(Roles = "SASuperUser")]
        public IActionResult Index([FromBody]string errors)
        {
            this.HttpContext.Session.SetString(UploadFileErrorIds.DefaultParameterUploadErrors, errors);

            return this.Ok();
        }

        [HttpPost]
        [Authorize(Roles = "SASuperUser")]
        public async Task<IActionResult> Upload(IFormFile fileUpload)
        {
            var csvErrors = CsvFileHelper.ValidateCSV(fileUpload);
            if (csvErrors.ErrorMessage is not null)
            {
                this.ViewBag.DefaultError = csvErrors;
                return this.View(
                    ViewNames.ParameterUploadFileErrorIndex,
                    new ViewModelCommonData
                    {
                        CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                    });
            }

            var schemeTemplateParameterValues = await CsvFileHelper.PrepareSchemeParameterDataForUpload(fileUpload);

            this.ViewData["schemeTemplateParameterValues"] = schemeTemplateParameterValues.ToArray();
            this.HttpContext.Session.SetString(SessionConstants.ParameterFileName, fileUpload.FileName);

            return this.View(ViewNames.ParameterUploadFileRefresh);
        }
    }
}
