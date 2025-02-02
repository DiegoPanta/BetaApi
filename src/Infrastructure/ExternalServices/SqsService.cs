using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Interfaces.IExternalService;

namespace Infrastructure.ExternalServices
{
    public class SqsService : ISqsService
    {
        private readonly IAmazonSQS _sqsClient;

        public SqsService(IAmazonSQS sqsClient)
        {
            _sqsClient = sqsClient;
        }

        public async Task SendMessageAsync<T>(string queueUrl, T message)
        {
            try
            {
                var messageBody = JsonSerializer.Serialize(message);
                var request = new SendMessageRequest
                {
                    QueueUrl = queueUrl,
                    MessageBody = messageBody
                };

                var response = await _sqsClient.SendMessageAsync(request);

                Console.WriteLine($"Mensagem enviada para SQS! ID: {response.MessageId}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro ao enviar mensagem para SQS: {ex.Message}", ex);
            }
        }
    }
}
