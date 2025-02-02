using Domain.ExternalServicesModels;

namespace Interfaces.IExternalService
{
    public interface ISqsConsumerService
    {
        Task<ApiResponse> GetLatestLoanSimulationDataAsync();
    }
}
