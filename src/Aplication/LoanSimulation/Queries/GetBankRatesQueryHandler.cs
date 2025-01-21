using Aplication.LoanSimulation.DTOs;
using Infrastructure.ExternalServices;
using MediatR;

namespace Aplication.LoanSimulation.Queries
{
    public class GetBankRatesQueryHandler : IRequestHandler<GetBankRatesQuery, List<BankRateResult>>
    {
        private readonly InterestRateService _interestRateService;

        public GetBankRatesQueryHandler(InterestRateService interestRateService)
        {
            _interestRateService = interestRateService;
        }

        public async Task<List<BankRateResult>> Handle(GetBankRatesQuery request, CancellationToken cancellationToken)
        {
            var bankRates = await _interestRateService.GetLoanSimulationDataAsync();

            return bankRates.Value.Select(rate => new BankRateResult
            {
                Cnpj = rate.Cnpj8,
                BankName = rate.InstituicaoFinanceira,
                MonthlyRate = rate.TaxaJurosAoMes,
                AnnualRate = rate.TaxaJurosAoAno
            }).ToList();
        }
    }

}
