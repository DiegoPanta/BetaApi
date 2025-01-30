using Aplication.LoanSimulation.DTOs;
using Interfaces.IRepositories;
using MediatR;

namespace Aplication.LoanSimulation.Queries
{
    public class GetAllSimulationsQueryHandler : IRequestHandler<GetAllSimulationsQuery, List<LoanSimulationResult>>
    {
        private readonly ILoanSimulationRepository _repository;

        public GetAllSimulationsQueryHandler(ILoanSimulationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<LoanSimulationResult>> Handle(GetAllSimulationsQuery request, CancellationToken cancellationToken)
        {
            var simulations = await _repository.GetAllAsync(cancellationToken);

            return simulations.Select(sim => new LoanSimulationResult
            {
                Id = sim.Id,
                Installments = sim.Installments,
                LoanAmount = sim.LoanAmount,
                MonthlyInstallment = sim.MonthlyInstallment,
                TotalCostMonth = sim.TotalCostMonth,
                TotalAnnualCost = sim.TotalAnnualCost,
                FinalCostYears = sim.FinalCostYears,
            }).ToList();
        }
    }

}
