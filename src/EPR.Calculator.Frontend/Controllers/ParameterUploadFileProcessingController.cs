using System.Net;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

public class ParameterUploadFileProcessingController(
    IConfiguration configuration,
    IEprCalculatorApiService eprCalculatorApiService,
    TelemetryClient telemetryClient)
    : BaseController
{
    private readonly int relativeYearStartingMonth = CommonUtil.GetRelativeYearStartingMonth(configuration);

    [HttpPost]
    public async Task<IActionResult> Index([FromBody] ParameterRefreshViewModel parameterRefreshViewModel)
    {
        var response = await PostDefaultParametersAsync(
            new CreateDefaultParameterSettingDto(
                parameterRefreshViewModel,
                CommonUtil.GetRelativeYear(HttpContext.Session, relativeYearStartingMonth)));

        if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Created)
            return Ok(response);

        telemetryClient.TrackTrace($"2.File name before BadRequest :{parameterRefreshViewModel.FileName}");
        telemetryClient.TrackTrace($"3.Reason for BadRequest :{response.Content.ReadAsStringAsync().Result}");
        return BadRequest(response.Content.ReadAsStringAsync().Result);
    }

    /// <summary>
    ///     Calls the "postDefaultParameterSettings" POST endpoint.
    /// </summary>
    /// <param name="dto">The data transfer object to serialise and use as the body of the request.</param>
    /// <returns>The response message returned by the endpoint.</returns>
    protected async Task<HttpResponseMessage> PostDefaultParametersAsync(CreateDefaultParameterSettingDto dto)
    {
        return await eprCalculatorApiService.CallApi(
            HttpMethod.Post,
            "v1/defaultParameterSetting",
            body: dto);
    }
}
