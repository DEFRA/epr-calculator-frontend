﻿using System.Text;
using EPR.Calculator.Frontend.Common;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

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
                try
                {
                    var scope = this.Configuration.GetSection("DownstreamApi:Scopes").Value!;
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

        /// <summary>
        /// Returns the financial year from session if feature enabled, else from config.
        /// </summary>
        /// <returns>Returns the financial year.</returns>
        /// <exception cref="ArgumentNullException">Returns error if financial year is null or empty.</exception>
        protected string GetFinancialYear()
        {
            var parameterYear = this.HttpContext.Session.GetString(SessionConstants.FinancialYear);

            if (string.IsNullOrWhiteSpace(parameterYear))
            {
                throw new ArgumentNullException(parameterYear, "No financial year was found in the session.");
            }

            return parameterYear;
        }
    }
}
