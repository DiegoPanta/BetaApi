using Aplication.LoanSimulation.DTOs;
using Domain.Business;
using Domain.Entities;
using Infrastructure.ExternalServices;
using Interfaces.IRepositories;
using MediatR;

namespace Aplication.LoanSimulation.Commands
{
    public class SimulateLoanHandler : IRequestHandler<SimulateLoanCommand, LoanSimulationResult>
    {

        private readonly LoanCalculator _loanCalculator;
        private readonly ILoanSimulationRepository _loanSimulationRepository;
        private readonly InterestRateService _interestRateService;

        public SimulateLoanHandler(LoanCalculator loanCalculator, ILoanSimulationRepository loanSimulationRepository, InterestRateService interestRateService)
        {
            _loanCalculator = loanCalculator;
            _loanSimulationRepository = loanSimulationRepository;
            _interestRateService = interestRateService;
        }

        public async Task<LoanSimulationResult> Handle(SimulateLoanCommand request, CancellationToken cancellationToken)
        {
            var data = await _interestRateService.GetLoanSimulationDataAsync();
            var monthlyRate = data.Value.FirstOrDefault().TaxaJurosAoMes;
            _loanCalculator.CalculateSimulation(request.LoanAmount, request.Installments, monthlyRate);

            var loanSimulationEntity = new LoanSimulationEntity
            {
                LoanAmount = request.LoanAmount,
                Installments = request.Installments,
                MonthlyInstallment = _loanCalculator.MonthlyInstallment,
                TotalCost = _loanCalculator.TotalCost
            };

            await _loanSimulationRepository.AddAsync(loanSimulationEntity, cancellationToken);

            return await Task.FromResult(new LoanSimulationResult
            {
                MonthlyInstallment = _loanCalculator.MonthlyInstallment,
                TotalCost = _loanCalculator.TotalCost
            });
        }
    }
}
