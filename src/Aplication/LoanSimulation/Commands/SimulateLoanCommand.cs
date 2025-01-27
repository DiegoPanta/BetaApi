using Aplication.LoanSimulation.DTOs;
using MediatR;

namespace Aplication.LoanSimulation.Commands
{
    public class SimulateLoanCommand : IRequest<LoanSimulationResult>
    {
        public required string Name { get; set; }

        public required string TaxId { get; set; }

        public required string Email { get; set; }

        public required string BankId { get; set; }

        public double LoanAmount { get; set; }

        public int Installments { get; set; }
    }
}
