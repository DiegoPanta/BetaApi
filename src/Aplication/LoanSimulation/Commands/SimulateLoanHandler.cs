using Domain.Business;
using Domain.Entities;
using Infrastructure.ExternalServices;
using Interfaces.IRepositories;
using MediatR;
using Shared.Exceptions;

namespace Aplication.LoanSimulation.Commands
{
    public class SimulateLoanHandler : IRequestHandler<SimulateLoanCommand, Unit>
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

        public async Task<Unit> Handle(SimulateLoanCommand request, CancellationToken cancellationToken)
        {
            var janeiro = 1;
            var data = await _interestRateService.GetLoanSimulationDataAsync();
            string yearMonth = $"{(DateTime.Now.Month == janeiro ? DateTime.Now.AddYears(-1).Year : DateTime.Now.Year)}-{DateTime.Now.AddMonths(-1).Month}";
            var monthlyRate = data.Value.FirstOrDefault(x => x.Cnpj8 == request.BankId && x.AnoMes == yearMonth);
            if (monthlyRate is null)
            {
                throw new Exception(ErrorMessages.MonthlyRateNotFound);
            }

            double monthlyInterestRateDecimal = _loanCalculator.CalculateInterestRate(monthlyRate.TaxaJurosAoMes);
            _loanCalculator.CalculateMonthlySimulation(request.LoanAmount, request.Installments, monthlyInterestRateDecimal);
            double annualInterestRateDecimal = _loanCalculator.CalculateInterestRate(monthlyRate.TaxaJurosAoAno);
            _loanCalculator.CalculateAnnualSimulation(request.LoanAmount, annualInterestRateDecimal, request.Installments);

            var loanSimulationEntity = new LoanSimulationEntity
            {
                LoanAmount = Math.Round(request.LoanAmount, 2),
                Installments = request.Installments,
                MonthlyInstallment = Math.Round(_loanCalculator.MonthlyInstallmentAmount, 2),
                TotalCostMonth = Math.Round(_loanCalculator.TotalCostMonth, 2),
                TotalAnnualCost = Math.Round(_loanCalculator.AnnualAmount, 2),
                FinalCostYears = Math.Round(_loanCalculator.FinalAmountWithAnnualInterest, 2),
            };

            await _loanSimulationRepository.AddAsync(loanSimulationEntity, cancellationToken);

            return Unit.Value;
        }

    }
}