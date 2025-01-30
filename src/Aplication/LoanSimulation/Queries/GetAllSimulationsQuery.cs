using Aplication.LoanSimulation.DTOs;
using MediatR;

namespace Aplication.LoanSimulation.Queries
{
    public class GetAllSimulationsQuery : IRequest<List<LoanSimulationResult>>
    {
    }
}
