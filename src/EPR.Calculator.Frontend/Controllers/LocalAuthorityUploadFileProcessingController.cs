﻿using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace EPR.Calculator.Frontend.Controllers
{
    public class LocalAuthorityUploadFileProcessingController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;

        public LocalAuthorityUploadFileProcessingController(IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
        }

        [HttpPost]
        public IActionResult Index([FromBody] List<LapcapDataTemplateValueDto> lapcapDataTemplateValues)
        {
            try
            {
                var lapcapSettingsApi = this.configuration.GetSection("LapcapSettings").GetSection("LapcapSettingsApi").Value;

                var client = this.clientFactory.CreateClient();
                client.BaseAddress = new Uri(lapcapSettingsApi);

                var payload = this.Transform(lapcapDataTemplateValues);

                var content = new StringContent(payload, System.Text.Encoding.UTF8, StaticHelpers.MediaType);

                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(lapcapSettingsApi));
                request.Content = content;

                var response = client.SendAsync(request);

                response.Wait();

                if (response.Result.IsSuccessStatusCode && response.Result.StatusCode == HttpStatusCode.Created)
                {
                    return this.Ok(response.Result);
                }

                return this.BadRequest(response.Result.Content.ReadAsStringAsync().Result);
            }
            catch (Exception)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        private string Transform(List<LapcapDataTemplateValueDto> lapcapDataTemplateValues)
        {
            var lapcapData = new CreateLapcapDataDto
            {
                ParameterYear = this.configuration.GetSection("LapcapSettings").GetSection("ParameterYear").Value,
                LapcapDataTemplateValues = lapcapDataTemplateValues,
            };

            return JsonConvert.SerializeObject(lapcapData);
        }
    }
}