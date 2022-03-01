using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Maranics.AppStore.SDK;
using Maranics.AppStore.SDK.Models;

namespace AppStore.SDK.Demo
{
    class Program
    {
        private const string ClientId = "d0d396a2-70b7-46a8-9395-f73dea2afd4b";
        private const string ClientsSecret = "VMrlJwbbhHUA/VhvtXMgzPnJg/O2bH5j2nHN47B5";
        private const string AppStoreUrl = "https://store.central.nightlybuild.dev";
        private const string ApiEndpointUrl = "https://api.central.nightlybuild.dev";
        private const string RequestHost = "127.0.0.1"; //Currently we dont need it, just do not leave it empty for Validation method.
        
        static async Task Main(string[] args)
        {
            var storeClient = InitializeAppStoreClient();
            var token = await RetriveAccessTokenAsync(storeClient);
            var tenantList = await RetriveAllowedTenantsList(storeClient,token);
            string tenant = "";
            if (tenantList != null)
                tenant = tenantList.FirstOrDefault().Id;
            Log($"Retrived tenant: {tenant}");

            string responseContent = await SendRequestAndGetDataFromUserManagement(token, tenant);
            Log($"Response data{responseContent}");

            bool requestValidationResult = ValidateRequest(storeClient, token, tenant, RequestHost);
            Log($"Client authorized and has access to required tenant: {requestValidationResult}");

        }

        /// <summary>
        /// Initialize appStore client
        /// </summary>
        private static AppStoreClient InitializeAppStoreClient()
        {
            var appStoreSettings = new AppStoreSettings
            {
                ClientId = ClientId,
                ClientSecret = ClientsSecret,
                AppStoreUrl = AppStoreUrl
            };

            return new AppStoreClient(appStoreSettings);
        }

        /// <summary>
        /// Authorize and get access token
        /// </summary>
        /// <param name="appStoreClient">Appstore client object</param>
        private static async Task<string> RetriveAccessTokenAsync(AppStoreClient appStoreClient)
        {
            Log("Capturing token from AppStore");
            var token = await appStoreClient.AuthorizeAsync();
            Log(token);

            if (string.IsNullOrEmpty(token))
            {
                Log("Received empty token");
                return string.Empty;
            }

            Log("Token received successfully");
            return token;
        }

        /// <summary>
        /// Example of getting allowed tenants
        /// </summary>
        /// <param name="appStoreClient">Appstore client object</param>
        /// <param name="token">Access token</param>
        /// <returns>Response data field which contain list of allowed tenants and their permissions</returns>
        private static async Task<List<TenantAccess>> RetriveAllowedTenantsList(AppStoreClient appStoreClient, string token)
        {
            var tenantResponse = await appStoreClient.GetAllowedTenants(token);
            if (tenantResponse != null && tenantResponse.HasError == false)
                return tenantResponse.Data;
            return null;
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

        /// <summary>
        /// Example of incoming request validation
        /// </summary>
        /// <param name="appStoreClient">Appstore client object</param>
        /// <param name="token">Access token</param>
        /// <param name="tenant">Tenant name</param>
        /// <param name="requestHost">Client ipAddress (in ipv4 format) usualy taken from request.HttpContext.Connection.RemoteIpAddress</param>
        private static bool ValidateRequest(AppStoreClient appStoreClient, string token,string tenant,string requestHost) 
        {
            Log("Token validation starting");

            //IsTokenSignatureValid method can be used separately.
            bool isSignatureValid = appStoreClient.IsTokenSignatureValid(token);
            Log($"Signature is valid: {isSignatureValid}");
            ValidationResponse validationResult = appStoreClient.ValidateAccess($"Bearer {token}", tenant, requestHost);
            return validationResult.HasAccess;
        }

        private static void Log(string content) => Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] {content}");
    }
}