using Interfaces.IRepositories;
using MediatR;
using Shared.Exceptions;

namespace Aplication.LoanSimulation.Commands
{
    public class DeleteLoanSimulationCommandHandler : IRequestHandler<DeleteLoanSimulationCommand, Unit>
    {
        private readonly ILoanSimulationRepository _loanSimulationRepository;

        public DeleteLoanSimulationCommandHandler(ILoanSimulationRepository loanSimulationRepository)
        {
            _loanSimulationRepository = loanSimulationRepository;
        }

        public async Task<Unit> Handle(DeleteLoanSimulationCommand request, CancellationToken cancellationToken)
        {
            var loanSimulation = await _loanSimulationRepository.GetByIdAsync(request.Id, cancellationToken);
            if (loanSimulation == null)
            {
                throw new Exception(ErrorMessages.SimulationNotFound);
            }

            await _loanSimulationRepository.DeleteAsync(loanSimulation, cancellationToken);

            return Unit.Value;
        }
    }

}
