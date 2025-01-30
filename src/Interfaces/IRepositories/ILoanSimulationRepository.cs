using Domain.Entities;

namespace Interfaces.IRepositories
{
    public interface ILoanSimulationRepository
    {
        Task AddAsync(LoanSimulationEntity loanSimulationEntity, CancellationToken cancellationToken);
        Task<LoanSimulationEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task DeleteAsync(LoanSimulationEntity loanSimulation, CancellationToken cancellationToken);
        Task<IEnumerable<LoanSimulationEntity>> GetAllAsync(CancellationToken cancellationToken);
    }
}
