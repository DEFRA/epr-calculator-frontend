using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    public class BaseController : Controller
    {
        private readonly ITokenAcquisition tokenAcquisition;
        protected readonly TelemetryClient telemetryClient;
        protected IConfiguration configuration;

        public BaseController(IConfiguration configuration, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient)
        {
            this.tokenAcquisition = tokenAcquisition;
            this.telemetryClient = telemetryClient;
            this.configuration = configuration;
        }

#pragma warning disable SA1600
        protected async Task<string> AcquireToken()
#pragma warning restore SA1600
        {
            var token = HttpContext?.Session?.GetString("accessToken");
            if (string.IsNullOrEmpty(token))
            {
                var scopes = new List<string> { "api://542488b9-bf70-429f-bad7-1e592efce352/Read_Scope" };
                token = await this.tokenAcquisition.GetAccessTokenForUserAsync(scopes);
                this.telemetryClient.TrackTrace("after generating..");
                HttpContext?.Session?.SetString("accessToken", token);
            }

            var accessToken = $"Bearer {token}";
            this.telemetryClient.TrackTrace($"accessToken is {accessToken}", SeverityLevel.Information);
            this.telemetryClient.TrackTrace($"accessToken length {accessToken.Length}", SeverityLevel.Information);
            return accessToken;
        }
    }
}
