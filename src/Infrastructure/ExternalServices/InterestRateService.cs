using Amazon.SQS;
using Amazon.SQS.Model;
using Domain.ExternalServicesModels;
using Interfaces.IExternalService;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Shared.Exceptions;

namespace Infrastructure.ExternalServices
{
    public class InterestRateService : IInterestRateService
    {
        private readonly HttpClient client;
        private static string? sqsQueueUrl;
        private static string? apiUrl;

        public InterestRateService(IConfiguration configuration)
        {
            client = new HttpClient();

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
                var response = await client.GetStringAsync(apiUrl);
                var data = JsonConvert.DeserializeObject<ApiResponse>(response);

                if (data == null)
                {
                    throw new InvalidOperationException(ErrorMessages.InvalidApiResponse);
                }

                await SendToSQS(data);

                return data;
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
