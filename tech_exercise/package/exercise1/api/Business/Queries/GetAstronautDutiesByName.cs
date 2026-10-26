using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Queries
{
    public class GetAstronautDutiesByName : IRequest<GetAstronautDutiesByNameResult>
    {
        // Incoming text is trimmed as it's set, so every caller gets clean input (D4).
        private string _name = string.Empty;

        public string Name { get => _name; set => _name = value?.Trim() ?? string.Empty; }
    }

    public class GetAstronautDutiesByNameHandler : IRequestHandler<GetAstronautDutiesByName, GetAstronautDutiesByNameResult>
    {
        private readonly StargateContext _context;

        public GetAstronautDutiesByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetAstronautDutiesByNameResult> Handle(GetAstronautDutiesByName request, CancellationToken cancellationToken)
        {

            var result = new GetAstronautDutiesByNameResult();

            var query = "SELECT a.Id as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate FROM [Person] a LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id WHERE @Name = a.Name";

            var person = await _context.Connection.QueryFirstOrDefaultAsync<PersonAstronaut>(query, new { request.Name });

            result.Person = person;

            // Not found is an expected outcome, returned as a result rather than thrown (D3).
            if (person is null)
            {
                result.Success = false;
                result.ResponseCode = (int)HttpStatusCode.NotFound;
                result.Message = $"Person '{request.Name}' not found.";
                return result;
            }

            query = "SELECT * FROM [AstronautDuty] WHERE @PersonId = PersonId Order By DutyStartDate Desc";

            var duties = await _context.Connection.QueryAsync<AstronautDuty>(query, new { person.PersonId });

            result.AstronautDuties = duties.ToList();

            return result;

        }
    }

    public class GetAstronautDutiesByNameResult : BaseResponse
    {
        public PersonAstronaut? Person { get; set; }
        public List<AstronautDuty> AstronautDuties { get; set; } = new List<AstronautDuty>();
    }
}
