using System;
using System.Net.Http;
using System.Threading.Tasks;
using Maranics.AppStore.SDK.Models;
using NetBox.Extensions;

namespace AppStore.SDK.Demo
{
    class Program
    {
        private const string ClientId = "d0d396a2-70b7-46a8-9395-f73dea2afd4b";
        private const string ClientsSecret = "VMrlJwbbhHUA/VhvtXMgzPnJg/O2bH5j2nHN47B5";
        private const string AppStoreUrl = "https://store.central.nightlybuild.dev";
        private const string ApiEndpointUrl = "https://api.central.nightlybuild.dev";
        private const string TenantName = "CentralNightlyInstance";
        private const string LocationId = "0b286d82-70b1-4e48-b7ce-544c3b922c38";
        
        static async Task Main(string[] args)
        {
            Log("Starting SDK demo");
            var appStoreSettings = new AppStoreSettings
            {
                ClientId = ClientId,
                ClientSecret = ClientsSecret,
                AppStoreUrl = AppStoreUrl
            };
            
            Log("Capturing token from AppStore");
            var appStoreClient = new Maranics.AppStore.SDK.AppStoreClient(appStoreSettings);
            var token = await appStoreClient.AuthorizeAsync();
            Log(token);
            if (string.IsNullOrEmpty(token))
            {
                Log("Received empty token");
                return;
            }
            
            Log("Token received successfully");

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "/app/flows/echo");
            
            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Headers.Add("Tenant", TenantName);
            request.Headers.Add("Location", LocationId);

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
                Log(responseString);
            }
            else
            {
                Log($"Invalid HTTP response, status code {response.StatusCode}");
            }
        }

        private static void Log(string content) => Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] {content}");
    }
}