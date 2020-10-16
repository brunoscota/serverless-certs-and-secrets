using System;
// using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;

namespace AzureCertAuthClientConsole
{
    public class AzureCertAuthClientConsole
    {
        private readonly ILogger _log;

        public AzureCertAuthClientConsole(ILoggerFactory loggerFactory)
        {
            _log = loggerFactory.CreateLogger<AzureCertAuthClientConsole>();
        }

        /// <summary>
        /// Only the Thumbprint, NotBefore and NotAfter are checked, further validation of the client can/should be added
        /// Chained certificate do not work with Azure App services, X509Chain only loads the certificate, not the chain on Azure
        /// Maybe due to the chain being not trusted. (Works outside Azure)
        /// Certificate validation docs
        /// https://github.com/dotnet/aspnetcore/blob/master/src/Security/Authentication/Certificate/src/CertificateAuthenticationHandler.cs
        /// https://docs.microsoft.com/en-us/azure/app-service/app-service-web-configure-tls-mutual-auth#aspnet-sample
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [FunctionName("RandomStringCertAuthClient")]
        public IActionResult RandomStringCertAuthClient(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            _log.LogInformation("C# HTTP trigger RandomString processed a request.");

            Console.WriteLine("Let's try to get a random string from the Azure Function using a certificate!");
            Console.WriteLine("----");
            var result = CallApi().GetAwaiter().GetResult();
            Console.WriteLine($"{result}");
            Console.WriteLine("----");
            Console.WriteLine($"Success!");

            return new OkObjectResult($"{result}");

            // return new BadRequestObjectResult("A valid client certificate is not found");
        }

        private string GetEncodedRandomString()
        {
            var base64 = Convert.ToBase64String(GenerateRandomBytes(100));
            return HtmlEncoder.Default.Encode(base64);
        }

        private byte[] GenerateRandomBytes(int length)
        {
            using var randonNumberGen = new RNGCryptoServiceProvider();
            var byteArray = new byte[length];
            randonNumberGen.GetBytes(byteArray);
            return byteArray;
        }

        private static async Task<string> CallApi()
        {
            // var bytes = File.ReadAllBytes("/home/site/wwwroot/functionsCertAuth.pfx");
            // var cert = new X509Certificate2(bytes, "1234");
            var cert = new X509Certificate2("/home/site/wwwroot/functionsCertAuth.pfx", "1234");

            var azureRandomStringBasicUrl = "https://fappcertificate.azurewebsites.net/api/RandomStringCertAuth";
            return await CallAzureDeployedAPI(azureRandomStringBasicUrl, cert);

            //var localRandomStringBasicUrl = "http://localhost:7071/api/RandomStringCertAuth";
            //return await CallApiXARRClientCertHeader(localRandomStringBasicUrl, cert);

        }

        private static async Task<string> CallAzureDeployedAPI(string url, X509Certificate2 clientCertificate)
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(clientCertificate);
            var client = new HttpClient(handler);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
                return responseContent;
            }

            throw new ApplicationException($"Status code: {response.StatusCode}, Error: {response.ReasonPhrase}");
        }

        // Local dev
        private static async Task<string> CallApiXARRClientCertHeader(string url, X509Certificate2 clientCertificate)
        {
            try
            {
                var handler = new HttpClientHandler();
                handler.ClientCertificates.Add(clientCertificate);
                var client = new HttpClient(handler);

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Get,
                };

                request.Headers.Add("X-ARR-ClientCert", Convert.ToBase64String(clientCertificate.RawData));
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return responseContent;
                }

                throw new ApplicationException($"Status code: {response.StatusCode}, Error: {response.ReasonPhrase}");
            }
            catch (Exception e)
            {
                throw new ApplicationException($"Exception {e}");
            }
        }

    }
}
