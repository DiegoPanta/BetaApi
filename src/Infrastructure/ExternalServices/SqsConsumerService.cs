using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Domain.ExternalServicesModels;
using Interfaces.IExternalService;
using Microsoft.Extensions.Configuration;
using Shared.Exceptions;

namespace Infrastructure.ExternalServices
{
    public class SqsConsumerService : ISqsConsumerService
    {
        private readonly IAmazonSQS _sqsClient;
        private static string? sqsQueueUrl;

        public SqsConsumerService(IAmazonSQS sqsClient, IConfiguration configuration)
        {
            _sqsClient = sqsClient;
            if (sqsQueueUrl == null)
            {
                sqsQueueUrl = configuration.GetSection("SQS:QueueUrl").Value ??
                    throw new ArgumentNullException("SQS:QueueUrl", ErrorMessages.MissingSQSQueueUrl);
            }
        }

        public async Task<ApiResponse> GetLatestLoanSimulationDataAsync()
        {
            var request = new ReceiveMessageRequest
            {
                QueueUrl = sqsQueueUrl,
                MaxNumberOfMessages = 1, // Apenas uma mensagem por vez
                WaitTimeSeconds = 10 // Long polling para reduzir custo
            };

            var response = await _sqsClient.ReceiveMessageAsync(request);

            if (response.Messages.Count == 0)
            {
                throw new Exception("Nenhuma mensagem encontrada na fila SQS.");
            }

            var message = response.Messages[0];
            var data = JsonSerializer.Deserialize<ApiResponse>(message.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Remover a mensagem da fila após o processamento
            await _sqsClient.DeleteMessageAsync(sqsQueueUrl, message.ReceiptHandle);

            return data!;
        }
    }
}
