using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Maranics.AppStore.SDK.Interfaces;
using Maranics.AppStore.SDK.Models;
using Maranics.AppStore.SDK.Services;

namespace AppStore.SDK.Demo
{
    public class AppStoreTest
    {
        private const string ApiEndpointUrl = "https://api.central.nightlybuild.dev";
        private const string RequestHost = "127.0.0.1"; //Currently we dont need it, just do not leave it empty for Validation method.

        private readonly IAccessValidationService _accessValidationService;
        private readonly IConnectivityService _connectivityService;
        private readonly IAccessTokenProvider _authorizationService;
        public AppStoreTest(IAccessValidationService validationService, IConnectivityService connectivityService, IAccessTokenProvider authorizationService)
        {
            _connectivityService = connectivityService;
            _authorizationService = authorizationService;
            _accessValidationService = validationService;
        }
        public async Task RunAsync()
        {
            var token = await _authorizationService.GetTokenAsync();
            var tenantList = await _connectivityService.GetAllowedTenants(token);
            string tenant = "";
            if (tenantList != null)
                tenant = tenantList.Data.FirstOrDefault().Id;
            Log($"Retrived tenant: {tenant}");

            string responseContent = await SendRequestAndGetDataFromUserManagement(token, tenant);
            Log($"Response data{responseContent}");

            ValidationResponse requestValidationResponse = await _accessValidationService.ValidateAccess(token, tenant, RequestHost);
            Log($"Client authorized and has access to required tenant: {requestValidationResponse.HasAccess}");
        }

        /// <summary>
        /// Example of request to Useremanagement system through api gateway
        /// </summary>
        /// <param name="token">Access token</param>
        /// <param name="tenant">Tenant name</param>
        private static async Task<string> SendRequestAndGetDataFromUserManagement(string token, string tenant)
        {
            var request = new HttpRequestMessage(
                  HttpMethod.Get,
                  "/app/usermanagement/categories");
            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Headers.Add("Tenant", tenant);
            //request.Headers.Add("Location", LocationId);

            using var client = new HttpClient
            {
                BaseAddress = new Uri(ApiEndpointUrl),
                Timeout = TimeSpan.FromSeconds(15)
            };

            Log("Sending Api request");
            var response = client.SendAsync(request).Result;

            if (response.IsSuccessStatusCode)
            {
                Log("Data received successfully");
                var responseString = await response.Content.ReadAsStringAsync();
                return responseString;
            }
            else
            {
                return response.StatusCode.ToString();
            }
        }

        private static void Log(string content) => Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] {content}");
    }
}
