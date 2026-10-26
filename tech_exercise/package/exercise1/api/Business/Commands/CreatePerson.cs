using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands
{
    public class CreatePerson : IRequest<CreatePersonResult>
    {
        // Incoming text is trimmed as it's set, so every caller gets clean input (D4).
        private string _name = string.Empty;

        public required string Name { get => _name; set => _name = value?.Trim() ?? string.Empty; }
    }

    public class CreatePersonPreProcessor : IRequestPreProcessor<CreatePerson>
    {
        private readonly StargateContext _context;
        public CreatePersonPreProcessor(StargateContext context)
        {
            _context = context;
        }
        public Task Process(CreatePerson request, CancellationToken cancellationToken)
        {
            // Input is already trimmed (D4), so blank names arrive here as empty.
            if (string.IsNullOrEmpty(request.Name)) throw new BadHttpRequestException("A name is required.");

            var person = _context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.Name);

            if (person is not null) throw new BadHttpRequestException("Bad Request");

            return Task.CompletedTask;
        }
    }

    public class CreatePersonHandler : IRequestHandler<CreatePerson, CreatePersonResult>
    {
        private readonly StargateContext _context;

        public CreatePersonHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<CreatePersonResult> Handle(CreatePerson request, CancellationToken cancellationToken)
        {

                var newPerson = new Person()
                {
                   Name = request.Name
                };

                await _context.People.AddAsync(newPerson);

                await _context.SaveChangesAsync();

                return new CreatePersonResult()
                {
                    Id = newPerson.Id
                };
          
        }
    }

    public class CreatePersonResult : BaseResponse
    {
        public int Id { get; set; }
    }
}
