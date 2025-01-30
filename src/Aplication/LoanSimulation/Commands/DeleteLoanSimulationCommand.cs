using MediatR;

namespace Aplication.LoanSimulation.Commands
{
    public class DeleteLoanSimulationCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }

        public DeleteLoanSimulationCommand(Guid id)
        {
            Id = id;
        }
    }
}
