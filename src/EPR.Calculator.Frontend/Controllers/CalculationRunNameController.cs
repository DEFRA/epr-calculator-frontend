using System.Net;
using System.Text.Json.Serialization;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

public class CalculationRunNameController(
    IConfiguration configuration,
    IEprCalculatorApiService eprCalculatorApiService,
    ILogger<CalculationRunNameController> logger)
    : BaseController
{
    private readonly int relativeYearStartingMonth = CommonUtil.GetRelativeYearStartingMonth(configuration);

    /// <summary>
    ///     Displays the index view for calculation run names.
    /// </summary>
    /// <returns>The index view.</returns>
    [Route("RunANewCalculation")]
    public IActionResult Index()
    {
        return View(ViewNames.CalculationRunNameIndex, CreateViewModel());
    }

    /// <summary>
    ///     Handles the calculator run initiation.
    /// </summary>
    /// <param name="calculationRunModel">The model containing calculation run details.</param>
    /// <returns>The result of the action.</returns>
    [HttpPost]
    public async Task<IActionResult> RunCalculator(InitiateCalculatorRunFormModel calculationRunModel)
    {
        if (!ModelState.IsValid)
        {
            var errorMessages = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage);
            return View(ViewNames.CalculationRunNameIndex, CreateViewModel(CreateErrorViewModel(errorMessages.First())));
        }

        if (!string.IsNullOrEmpty(calculationRunModel.CalculationName))
        {
            var calculationName = calculationRunModel.CalculationName.Trim();
            var runDto = await eprCalculatorApiService.GetCalculatorRun(calculationName);

            if (runDto != null)
                return View(ViewNames.CalculationRunNameIndex, CreateViewModel(CreateErrorViewModel(ErrorMessages.CalculationRunNameExists)));

            var response = await HttpPostToCalculatorRunApi(calculationName);

            if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
            {
                return View(
                    ViewNames.CalculationRunErrorIndex,
                    new CalculationRunErrorViewModel
                    {
                        ErrorMessage = await ExtractErrorMessageAsync(response)
                    });
            }

            if (response.StatusCode == HttpStatusCode.FailedDependency)
            {
                return View(
                    ViewNames.CalculationRunErrorIndex,
                    new CalculationRunErrorViewModel
                    {
                        ErrorMessage = await response.Content.ReadAsStringAsync()
                    });
            }

            if (!response.IsSuccessStatusCode || response.StatusCode != HttpStatusCode.Accepted)
                return RedirectToError();
        }

        return RedirectToAction(ActionNames.RunCalculatorConfirmation, new { calculationName = calculationRunModel.CalculationName });
    }

    /// <summary>
    ///     Displays the confirmation view after running the calculator.
    /// </summary>
    /// <param name="calculationName">calculation run name.</param>
    /// <returns>The result of the action.</returns>
    [Route("Confirmation")]
    public IActionResult Confirmation(string? calculationName)
    {
        var calculationRunConfirmationViewModel = new ConfirmationViewModel
        {
            Title = CalculatorRunNames.Title,
            Body = calculationName ?? string.Empty,
            AdditionalParagraphs = CalculatorRunNames.AdditionalParagraphs.ToList()
        };

        return View(ViewNames.CalculationRunConfirmation, calculationRunConfirmationViewModel);
    }

    /// <summary>
    ///     Creates an error view model.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>The error view model.</returns>
    private static ErrorViewModel CreateErrorViewModel(string errorMessage)
    {
        return new ErrorViewModel
        {
            DOMElementId = ViewControlNames.CalculationRunName,
            ErrorMessage = errorMessage
        };
    }

    private static InitiateCalculatorRunViewModel CreateViewModel(ErrorViewModel? errors = null)
    {
        return new InitiateCalculatorRunViewModel
        {
            Errors = errors
        };
    }

    /// <summary>
    ///     Sends an HTTP request to the calculator run API.
    /// </summary>
    /// <param name="calculatorRunName">The name of the calculator run.</param>
    /// <returns>The HTTP response message.</returns>
    /// <exception cref="ArgumentNullException">ArgumentNullException will be thrown.</exception>
    private async Task<HttpResponseMessage> HttpPostToCalculatorRunApi(string calculatorRunName)
    {
        return await PostCalculatorRunAsync(new CreateCalculatorRunDto
        {
            CalculatorRunName = calculatorRunName,
            RelativeYear = CommonUtil.GetRelativeYear(HttpContext.Session, relativeYearStartingMonth),
            CreatedBy = CommonUtil.GetUserName(HttpContext)
        });
    }

    private async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return error?.Message ?? "An error occurred. Please try again.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error parsing response");
            return "Unable to process the error response.";
        }
    }

    /// <summary>
    ///     Calls the "calculatorRun" POST endpoint.
    /// </summary>
    /// <param name="dto">The data transfer object to serialise and use as the body of the request.</param>
    /// <returns>The response message returned by the endpoint.</returns>
    private async Task<HttpResponseMessage> PostCalculatorRunAsync(CreateCalculatorRunDto dto)
    {
        return await eprCalculatorApiService.CallApi(
            HttpMethod.Post,
            "v1/calculatorRun",
            body: dto);
    }

    private class ErrorResponse
    {
        [JsonInclude] public string? Message { get; set; }
    }
}
