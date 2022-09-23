using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using CalpineAzureDashboard.Web.Models;
using log4net;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.Rest;

namespace CalpineAzureDashboard.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly string _apiUrl;
        private readonly string _authorityUri;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _groupId;
        private readonly string _homeReportName;
        private readonly string _resourceUri;

        private static readonly ILog Log = LogManager.GetLogger(typeof(DashboardController));

        public DashboardController()
        {
            _apiUrl = ConfigurationManager.AppSettings["powerbi:ApiUrl"];
            _groupId = ConfigurationManager.AppSettings["powerbi:GroupId"];
            _homeReportName = ConfigurationManager.AppSettings["powerbi:HomeReportName"];
            _resourceUri = ConfigurationManager.AppSettings["powerbi:ResourceUri"];

            var keyVaultUri = ConfigurationManager.AppSettings["Azure:KeyVaultUri"];
            var tenantIdTask = GetKeyVaultSecretAsync(keyVaultUri, "tenantId");
            var clientIdTask = GetKeyVaultSecretAsync(keyVaultUri, "clientId");
            var clientSecretTask = GetKeyVaultSecretAsync(keyVaultUri, "clientSecret");

            Task.WaitAll(tenantIdTask, clientIdTask, clientSecretTask);
            _clientId = clientIdTask.Result;
            _clientSecret = clientSecretTask.Result;
            
            _authorityUri = string.Format(System.Globalization.CultureInfo.InvariantCulture, ConfigurationManager.AppSettings["Azure:AuthorityUri"], tenantIdTask.Result);
        }

        [Route("", Name = "dashboard")]
        public ActionResult Index()
        {
            var roles = GetUserRoles();
            if (!GetUserRoles().Any()) {
                return View("Unauthorized");
            }

            var reports = GetReportsInGroupForUser(_groupId).ToList();
            var report = reports.FirstOrDefault(x => x.Name.Equals(_homeReportName)) ?? reports.FirstOrDefault();

            if (report == null) {
                return RedirectToAction("Index", "Error");
            }

            return RedirectToAction("Report",
                new {
                    groupId = _groupId,
                    reportId = report.Id,
                    reportName = report.Name,
                    datasetId = report.DatasetId,
                    embedUrl = report.EmbedUrl
                });
        }

        [Authorize]
        public async Task<ActionResult> Report(string groupId, string reportId, string reportName, string datasetId, string embedUrl)
        {
            try {
                var report = new Report {
                    Id = reportId,
                    Name = reportName,
                    DatasetId = datasetId,
                    EmbedUrl = embedUrl
                };

                var reportViewModel = new ReportViewModel {
                    GroupId = groupId,
                    Report = report
                };

                return View(reportViewModel);
            }
            catch (Exception ex) {
                Log.Error("An exception occurred getting report: " + reportName, ex);
                return RedirectToAction("Index", "Error");
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<JsonResult> GetToken(string groupId, string reportId, string datasetId)
        {
            using (var client = new PowerBIClient(new Uri(_apiUrl), GetTokenCredentials())) {
                try {
                    var generateTokenRequestParameters = new GenerateTokenRequest {
                        AccessLevel = "view",
                        Identities = new List<EffectiveIdentity> {
                            new EffectiveIdentity {
                                Username = User.Identity.Name,
                                Roles = GetUserRoles()?.ToList() ?? new List<string>(),
                                Datasets = new[] {datasetId}
                            }
                        }
                    };
                    var embedToken = await client.Reports.GenerateTokenAsync(groupId, reportId, generateTokenRequestParameters);
                    return Json(new {
                        token = embedToken.Token, expiration = ((DateTimeOffset) embedToken.Expiration.GetValueOrDefault()).ToUnixTimeMilliseconds()
                    });
                }
                catch (Exception ex) {
                    Log.Error($"An error occurred getting token for reportId {reportId}:", ex);
                    throw;
                }
            }
        }

        [Authorize]
        [ChildActionOnly]
        public ActionResult Reports()
        {
            if (!GetUserRoles().Any()) {
                return null;
            }

            var reportsViewModels = new List<ReportsViewModel>();
            reportsViewModels.Add(new ReportsViewModel {
                GroupId = _groupId,
                Reports = GetReportsInGroupForUser(_groupId).ToList()
            });

            return PartialView(reportsViewModels);
        }

        private IEnumerable<Report> GetReportsInGroupForUser(string groupId)
        {
            try {
                using (var client = new PowerBIClient(new Uri(_apiUrl), GetTokenCredentials())) {
                    var reports = client.Reports.GetReportsInGroup(groupId).Value;
                    return reports.Where(x => x.Name.StartsWith("CAD - "))
                        .Where(x => User.IsInRole("AllReports") || User.IsInRole(x.Name.Replace(" ", "")))
                        .OrderBy(r => r.Name);
                }
            }
            catch (Exception ex) {
                Log.Error($"An exception occurred getting reports in group '{groupId}' for user: {User.Identity.Name}", ex);
                return new List<Report>();
            }
        }

        private async Task<string> GetKeyVaultSecretAsync(string keyVaultUrl, string secretName)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            var secret = await keyVaultClient.GetSecretAsync($"{keyVaultUrl}/secrets/{secretName}").ConfigureAwait(false);
            return secret.Value;
        }

        private IEnumerable<string> GetUserRoles()
        {
            return ((ClaimsIdentity) User.Identity).Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
        }

        private TokenCredentials GetTokenCredentials()
        {
            AuthenticationContext authContext = new AuthenticationContext(_authorityUri);

            var credential = new ClientCredential(_clientId, _clientSecret);

            var authenticationResult = authContext
                .AcquireTokenAsync(_resourceUri, credential)
                .GetAwaiter().GetResult();

            return authenticationResult == null ? null : new TokenCredentials(authenticationResult.AccessToken, "Bearer");
        }
    }
}
