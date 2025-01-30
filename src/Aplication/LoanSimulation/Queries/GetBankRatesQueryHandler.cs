using Aplication.LoanSimulation.DTOs;
using Interfaces.IExternalService;
using MediatR;

namespace Aplication.LoanSimulation.Queries
{
    public class GetBankRatesQueryHandler : IRequestHandler<GetBankRatesQuery, List<BankRateResult>>
    {
        private readonly IInterestRateService _interestRateService;

        public GetBankRatesQueryHandler(IInterestRateService interestRateService)
        {
            _interestRateService = interestRateService;
        }

        public async Task<List<BankRateResult>> Handle(GetBankRatesQuery request, CancellationToken cancellationToken)
        {
            var bankRates = await _interestRateService.GetLoanSimulationDataAsync();

            return bankRates.Value
                .Select(rate => new BankRateResult
                {
                    Cnpj = rate.Cnpj8,
                    BankName = rate.InstituicaoFinanceira,
                    MonthlyRate = rate.TaxaJurosAoMes,
                    AnnualRate = rate.TaxaJurosAoAno,
                    YearMonth = rate.AnoMes,
                })
                .GroupBy(rate => rate.Cnpj)
                .Select(group => group.OrderBy(r => r.MonthlyRate)
                                    .First())
                .ToList();
        }
    }

}
