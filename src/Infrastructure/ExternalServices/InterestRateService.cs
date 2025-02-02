using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Domain.ExternalServicesModels;
using Interfaces.IExternalService;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Shared.Exceptions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Infrastructure.ExternalServices
{
    public class InterestRateService : IInterestRateService
    {
        private readonly HttpClient client;
        private static string? sqsQueueUrl;
        private static string? apiUrl;

        public InterestRateService(HttpClient httpClient, IConfiguration configuration)
        {
            client = httpClient;

            if (sqsQueueUrl == null)
            {
                sqsQueueUrl = configuration.GetSection("SQS:QueueUrl").Value ?? 
                    throw new ArgumentNullException("SQS:QueueUrl", ErrorMessages.MissingSQSQueueUrl);
                apiUrl = configuration.GetSection("API:LoanSimulationUrl").Value ?? 
                    throw new ArgumentNullException("API:LoanSimulationUrl", ErrorMessages.MissingLoanSimulationApiUrl);
            }
        }

        public async Task<ApiResponse> GetLoanSimulationDataAsync()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro {response.StatusCode}: {response.ReasonPhrase}");
                }
                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                {
                    throw new InvalidOperationException(ErrorMessages.InvalidApiResponse);
                }

                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(content, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (apiResponse == null || apiResponse.Value == null || apiResponse.Value.Count == 0)
                {
                    throw new InvalidOperationException(ErrorMessages.InvalidApiResponse);
                }

                 await SendToSQS(apiResponse);

            return apiResponse;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ErrorMessages.GeneralError} {ex.Message}", ex);
            }
        }

        private static async Task SendToSQS(ApiResponse data)
        {
            var sqsClient = new AmazonSQSClient();

            var message = new SendMessageRequest
            {
                QueueUrl = sqsQueueUrl,
                MessageBody = JsonConvert.SerializeObject(data)
            };

            try
            {
                var sendMessageResponse = await sqsClient.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ErrorMessages.ErrorSendingToSQS} {ex.Message}");
            }
        }
    }
}
