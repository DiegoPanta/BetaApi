using Domain.Business;
using Domain.Entities;
using Interfaces.IExternalService;
using Interfaces.IRepositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;

namespace Aplication.LoanSimulation.Commands
{
    public class SimulateLoanHandler : IRequestHandler<SimulateLoanCommand, Unit>
    {

        private readonly LoanCalculator _loanCalculator;
        private readonly ILoanSimulationRepository _loanSimulationRepository;
        private readonly ISqsConsumerService _sqsConsumerService;
        private readonly ILogger<SimulateLoanHandler> _logger;

        public SimulateLoanHandler(LoanCalculator loanCalculator, 
            ILoanSimulationRepository loanSimulationRepository, 
            ISqsConsumerService sqsConsumerService, 
            ILogger<SimulateLoanHandler> logger)
        {
            _loanCalculator = loanCalculator;
            _loanSimulationRepository = loanSimulationRepository;
            _sqsConsumerService = sqsConsumerService;
            _logger = logger;
        }

        public async Task<Unit> Handle(SimulateLoanCommand request, CancellationToken cancellationToken)
        {
            var janeiro = 1;
            _logger.LogInformation("Processing loan simulation for Amount: {Amount}, Installments: {Installments}",
           request.LoanAmount, request.Installments);
            var data = await _sqsConsumerService.GetLatestLoanSimulationDataAsync();
            string yearMonth = $"{(DateTime.Now.Month == janeiro ? DateTime.Now.AddYears(-1).Year : DateTime.Now.Year)}-{DateTime.Now.AddMonths(-1).Month}";
            string yearMonthValid = $"2024-12";
            var monthlyRate = data.Value.FirstOrDefault(x => x.Cnpj8 == request.BankId && x.AnoMes == yearMonthValid);
            if (monthlyRate is null)
            {
                _logger.LogError("Monthly Rate not found for Loan Simulation.");
                throw new Exception(ErrorMessages.MonthlyRateNotFound);
            }

            _logger.LogInformation("Monthly Rate retrieved: {Rate}", monthlyRate);
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