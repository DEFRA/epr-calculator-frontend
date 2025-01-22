﻿using System.Text;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using static System.Formats.Asn1.AsnWriter;
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
            this.TelemetryClient.TrackTrace("AcquireToken");
            var token = this.HttpContext?.Session?.GetString("accessToken");
            if (string.IsNullOrEmpty(token))
            {
                var scopeArray = new[]
                {
                    "openid",
                    "offline_access",
                    "api://3578935b-788b-4f53-931d-dddd67dca987/Write_Scope",
                    "api://3578935b-788b-4f53-931d-dddd67dca987/Read_Scope",
                    "api://3578935b-788b-4f53-931d-dddd67dca987/default",
                };

                var scope = string.Join(" ", scopeArray);
                try
                {
                    this.TelemetryClient.TrackTrace($"GetAccessTokenForUserAsync with scope- {scope}");
                    token = await this.tokenAcquisition.GetAccessTokenForUserAsync([scope]);
                }
                catch (Exception ex)
                {
                    this.TelemetryClient.TrackException(ex);
                    throw;
                }

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
