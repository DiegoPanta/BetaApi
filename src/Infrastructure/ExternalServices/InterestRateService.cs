﻿using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Domain.ExternalServicesModels;
using Interfaces.IExternalService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Infrastructure.ExternalServices
{
    public class InterestRateService : IInterestRateService
    {
        private readonly HttpClient client;
        private readonly IAmazonSQS _sqsClient;
        private readonly ISqsService _sqsService;
        private static string? sqsQueueUrl;
        private static string? apiUrl;
        private readonly ILogger<InterestRateService> _logger;

        public InterestRateService(HttpClient httpClient, IAmazonSQS sqsClient, IConfiguration configuration, ISqsService sqsService, ILogger<InterestRateService> logger)
        {
            _logger = logger;
            client = httpClient;
            _sqsClient = sqsClient;
            _sqsService = sqsService;
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
            _logger.LogInformation("Fetching Monthly Rate from database/API.");
            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("No Monthly Rate found in the database/API.");
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
                await _sqsService.SendMessageAsync(sqsQueueUrl, apiResponse);

            return apiResponse;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ErrorMessages.GeneralError} {ex.Message}", ex);
            }
        }
    }
}
