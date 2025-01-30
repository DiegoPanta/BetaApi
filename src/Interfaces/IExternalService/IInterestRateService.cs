using Domain.ExternalServicesModels;

namespace Interfaces.IExternalService
{
    public interface IInterestRateService
    {
        Task<ApiResponse> GetLoanSimulationDataAsync();
    }
}
