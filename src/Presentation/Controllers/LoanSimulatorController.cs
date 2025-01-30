using Aplication.LoanSimulation.Commands;
using Aplication.LoanSimulation.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Exceptions;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class LoanSimulatorController : Controller
    {
        private readonly IMediator _mediator;

        public LoanSimulatorController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("bankRates")]
        public async Task<IActionResult> GetBankRates()
        {
            var query = new GetBankRatesQuery();
            var result = await _mediator.Send(query);

            if (result == null || !result.Any())
            {
                return NotFound(ErrorMessages.NoBankRatesFound);
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSimulations()
        {
            var result = await _mediator.Send(new GetAllSimulationsQuery());
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoanSimulation(Guid id)
        {
            try
            {
                await _mediator.Send(new DeleteLoanSimulationCommand(id));

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ErrorMessages.SimulationError, details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSimulation([FromBody] SimulateLoanCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
