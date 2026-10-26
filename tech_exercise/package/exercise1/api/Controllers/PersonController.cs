using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;

namespace StargateAPI.Controllers
{
    /// <summary>People, whether or not they have served as astronauts.</summary>
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
    public class PersonController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PersonController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Retrieves all people, with their current astronaut details if they have any.</summary>
        [HttpGet("")]
        [ProducesResponseType(typeof(GetPeopleResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPeople()
        {
            var result = await _mediator.Send(new GetPeople()
            {

            });

            return this.GetResponse(result);
        }

        /// <summary>Retrieves a person by name, ignoring case and surrounding spaces.</summary>
        /// <param name="name">The person's name.</param>
        /// <response code="200">The person, with null astronaut fields if they've never been an astronaut.</response>
        /// <response code="404">No person has that name.</response>
        [HttpGet("{name}")]
        [ProducesResponseType(typeof(GetPersonByNameResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GetPersonByNameResult), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPersonByName(string name)
        {
            var result = await _mediator.Send(new GetPersonByName()
            {
                Name = name
            });

            return this.GetResponse(result);
        }

        /// <summary>Adds a person. Names are unique, ignoring case.</summary>
        /// <param name="name">The person's name, as a JSON string, e.g. <c>"John Doe"</c>.</param>
        /// <response code="200">The person was added; returns their id.</response>
        /// <response code="400">The name is blank.</response>
        /// <response code="409">A person with that name already exists.</response>
        [HttpPost("")]
        [ProducesResponseType(typeof(CreatePersonResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreatePerson([FromBody] string name)
        {
            var result = await _mediator.Send(new CreatePerson()
            {
                Name = name
            });

            return this.GetResponse(result);

        }

        /// <summary>Renames a person. Their duties and astronaut details are kept.</summary>
        /// <param name="name">The person's current name, ignoring case and surrounding spaces.</param>
        /// <param name="newName">The new name, as a JSON string, e.g. <c>"Jonathan Doe"</c>. A change of case only is allowed.</param>
        /// <response code="200">The person was renamed (or already had that name); returns their id.</response>
        /// <response code="400">The new name is blank.</response>
        /// <response code="404">No person has that name.</response>
        /// <response code="409">Another person already has the new name.</response>
        [HttpPut("{name}")]
        [ProducesResponseType(typeof(UpdatePersonResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdatePerson(string name, [FromBody] string newName)
        {
            var result = await _mediator.Send(new UpdatePerson()
            {
                Name = name,
                NewName = newName
            });

            return this.GetResponse(result);
        }
    }
}