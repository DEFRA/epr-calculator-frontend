﻿using System.Text;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using static System.Net.Mime.MediaTypeNames;

namespace EPR.Calculator.Frontend.Controllers
{
    public class BaseController : Controller
    {
        private readonly ITokenAcquisition tokenAcquisition;

        public BaseController(IConfiguration configuration, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient)
        {
            this.tokenAcquisition = tokenAcquisition;
            this.TelemetryClient = telemetryClient;
            this.Configuration = configuration;
        }

#pragma warning disable SA1600
        protected TelemetryClient TelemetryClient { get; set; }
#pragma warning restore SA1600

#pragma warning disable SA1600
        protected IConfiguration Configuration { get; set; }
#pragma warning restore SA1600

#pragma warning disable SA1600
        protected async Task<string> AcquireToken()
#pragma warning restore SA1600
        {
            var token = this.HttpContext?.Session?.GetString("accessToken");
            if (string.IsNullOrEmpty(token))
            {
                var scope = this.Configuration.GetSection("DownstreamApi:Scopes").AsEnumerable();
                var scopes = scope.Select(s => s.Value).Where(x => x != null && !string.IsNullOrEmpty(x));
                token = await this.tokenAcquisition.GetAccessTokenForUserAsync(scopes);
                this.TelemetryClient.TrackTrace($"scope is {scope}");
                this.TelemetryClient.TrackTrace("after generating..");
                this.HttpContext?.Session?.SetString("accessToken", token);
            }

            var accessToken = $"Bearer {token}";
            this.TelemetryClient.TrackTrace($"accessToken is {accessToken}", SeverityLevel.Information);
            this.TelemetryClient.TrackTrace($"accessToken length {accessToken.Length}", SeverityLevel.Information);
            return accessToken;
        }
    }
}
