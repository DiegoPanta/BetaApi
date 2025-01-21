using Aplication.LoanSimulation.DTOs;
using MediatR;

namespace Aplication.LoanSimulation.Queries
{
    public class GetBankRatesQuery : IRequest<List<BankRateResult>>
    {
        
    }
}
