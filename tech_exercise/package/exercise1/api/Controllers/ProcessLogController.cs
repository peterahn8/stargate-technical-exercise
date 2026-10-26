using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Queries;

namespace StargateAPI.Controllers
{
    /// <summary>The process log: what the API has done recently (SPEC-5).</summary>
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
    public class ProcessLogController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProcessLogController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Retrieves the most recent process log entries, newest first.</summary>
        /// <param name="take">How many entries to return: 20 by default, at most 100.</param>
        /// <response code="200">The entries, without exception details or request payloads.</response>
        [HttpGet("")]
        [ProducesResponseType(typeof(GetProcessLogsResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProcessLogs([FromQuery] int take = Business.Queries.GetProcessLogs.DefaultTake)
        {
            var result = await _mediator.Send(new GetProcessLogs()
            {
                Take = take
            });

            return this.GetResponse(result);
        }
    }
}
