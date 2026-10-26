using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;

namespace StargateAPI.Controllers
{
    /// <summary>A person's astronaut duties: their assignments over time.</summary>
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
    public class AstronautDutyController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AstronautDutyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Retrieves a person's current astronaut details and all their duties, newest first.</summary>
        /// <param name="name">The person's name, ignoring case and surrounding spaces.</param>
        /// <response code="200">The person and their duties; the list is empty if they've never been an astronaut.</response>
        /// <response code="404">No person has that name.</response>
        [HttpGet("{name}")]
        [ProducesResponseType(typeof(GetAstronautDutiesByNameResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GetAstronautDutiesByNameResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAstronautDutiesByName(string name)
        {
            var result = await _mediator.Send(new GetAstronautDutiesByName()
            {
                Name = name
            });

            return this.GetResponse(result);            
        }

        /// <summary>
        /// Adds a duty for a person. It becomes their current duty and ends the previous one the day
        /// before it starts. A RETIRED duty retires the person; any other duty returns a retired person to service.
        /// </summary>
        /// <response code="200">The duty was added; returns its id.</response>
        /// <response code="400">Invalid input, or the duty breaks a rule (e.g. it doesn't start after the current duty).</response>
        /// <response code="404">No person has that name.</response>
        /// <response code="409">Another change to this person's duties happened at the same time; retry.</response>
        [HttpPost("")]
        [ProducesResponseType(typeof(CreateAstronautDutyResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateAstronautDuty([FromBody] CreateAstronautDuty request)
        {
                var result = await _mediator.Send(request);
                return this.GetResponse(result);           
        }
    }
}