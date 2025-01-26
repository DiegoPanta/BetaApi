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

        [HttpGet]
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

        [HttpPost]
        public async Task<IActionResult> CreateSimulation([FromBody] SimulateLoanCommand command)
        
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
