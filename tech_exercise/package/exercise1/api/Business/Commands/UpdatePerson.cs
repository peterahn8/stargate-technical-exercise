using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands
{
    /// <summary>Renames a person (D16). Their duties and astronaut details stay linked by id.</summary>
    public class UpdatePerson : IRequest<UpdatePersonResult>
    {
        // Incoming text is trimmed as it's set, so every caller gets clean input (D4).
        private string _name = string.Empty;
        private string _newName = string.Empty;

        public required string Name { get => _name; set => _name = value?.Trim() ?? string.Empty; }

        public required string NewName { get => _newName; set => _newName = value?.Trim() ?? string.Empty; }
    }

    public class UpdatePersonPreProcessor : IRequestPreProcessor<UpdatePerson>
    {
        private readonly StargateContext _context;
        public UpdatePersonPreProcessor(StargateContext context)
        {
            _context = context;
        }
        public async Task Process(UpdatePerson request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.NewName)) throw new BadHttpRequestException("A new name is required.");

            var person = await _context.People.AsNoTracking().FirstOrDefaultAsync(z => z.Name == request.Name, cancellationToken);

            if (person is null)
                throw new BadHttpRequestException($"Person '{request.Name}' not found.", StatusCodes.Status404NotFound);

            // Names ignore case (D4), so renaming "jane doe" to "Jane Doe" matches only this person and is allowed.
            var nameTaken = await _context.People.AnyAsync(z => z.Name == request.NewName && z.Id != person.Id, cancellationToken);

            if (nameTaken)
                throw new BadHttpRequestException($"A person named '{request.NewName}' already exists.", StatusCodes.Status409Conflict);
        }
    }

    public class UpdatePersonHandler : IRequestHandler<UpdatePerson, UpdatePersonResult>
    {
        private readonly StargateContext _context;

        public UpdatePersonHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<UpdatePersonResult> Handle(UpdatePerson request, CancellationToken cancellationToken)
        {
            var person = await _context.People.FirstAsync(z => z.Name == request.Name, cancellationToken);

            person.Name = request.NewName;

            await _context.SaveChangesAsync(cancellationToken);

            return new UpdatePersonResult()
            {
                Id = person.Id
            };
        }
    }

    public class UpdatePersonResult : BaseResponse
    {
        public int Id { get; set; }
    }
}
