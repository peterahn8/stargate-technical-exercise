using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Commands
{
    public class CreateAstronautDuty : IRequest<CreateAstronautDutyResult>
    {
        // Incoming text is trimmed as it's set, so every caller gets clean input (D4).
        private string _name = string.Empty;
        private string _rank = string.Empty;
        private string _dutyTitle = string.Empty;

        public required string Name { get => _name; set => _name = value?.Trim() ?? string.Empty; }

        public required string Rank { get => _rank; set => _rank = value?.Trim() ?? string.Empty; }

        public required string DutyTitle { get => _dutyTitle; set => _dutyTitle = value?.Trim() ?? string.Empty; }

        public required DateTime DutyStartDate { get; set; }
    }

    public class CreateAstronautDutyPreProcessor : IRequestPreProcessor<CreateAstronautDuty>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyPreProcessor(StargateContext context)
        {
            _context = context;
        }

        public async Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            // Input is already trimmed (D4), so blank text arrives here as empty.
            if (string.IsNullOrEmpty(request.Name)) throw new BadHttpRequestException("A name is required.");
            if (string.IsNullOrEmpty(request.Rank)) throw new BadHttpRequestException("A rank is required.");
            if (string.IsNullOrEmpty(request.DutyTitle)) throw new BadHttpRequestException("A duty title is required.");
            if (request.DutyStartDate == default) throw new BadHttpRequestException("A duty start date is required.");

            var person = await _context.People.AsNoTracking().FirstOrDefaultAsync(z => z.Name == request.Name, cancellationToken);

            if (person is null)
                throw new BadHttpRequestException($"Person '{request.Name}' not found.", StatusCodes.Status404NotFound);

            // Duties only append to the timeline: a new duty must start after the current one (D1).
            // This also rejects duplicates, which share a start date with an existing duty.
            var currentDuty = await _context.AstronautDuties.AsNoTracking()
                .Where(z => z.PersonId == person.Id)
                .OrderByDescending(z => z.DutyStartDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (currentDuty is not null && request.DutyStartDate.Date <= currentDuty.DutyStartDate)
                throw new BadHttpRequestException($"A new duty must start after the current duty's start date ({currentDuty.DutyStartDate:yyyy-MM-dd}).");

            // You can only retire from active service (D2).
            if (DutyTitles.IsRetired(request.DutyTitle))
            {
                if (currentDuty is null)
                    throw new BadHttpRequestException("RETIRED can't be a person's first duty.");

                if (DutyTitles.IsRetired(currentDuty.DutyTitle))
                    throw new BadHttpRequestException("The person is already retired.");
            }
        }
    }

    public class CreateAstronautDutyHandler : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResult>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<CreateAstronautDutyResult> Handle(CreateAstronautDuty request, CancellationToken cancellationToken)
        {

            var person = await _context.People.FirstAsync(z => z.Name == request.Name, cancellationToken);

            var astronautDetail = await _context.AstronautDetails.FirstOrDefaultAsync(z => z.PersonId == person.Id, cancellationToken);

            if (astronautDetail == null)
            {
                astronautDetail = new AstronautDetail();
                astronautDetail.PersonId = person.Id;
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;
                astronautDetail.CareerStartDate = request.DutyStartDate.Date;

                await _context.AstronautDetails.AddAsync(astronautDetail);

            }
            else
            {
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;
                if (DutyTitles.IsRetired(request.DutyTitle))
                {
                    astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
                }
                else
                {
                    // A non-RETIRED duty means active service, so a retired person returns to service (D2).
                    astronautDetail.CareerEndDate = null;
                }
            }

            var astronautDuty = await _context.AstronautDuties
                .Where(z => z.PersonId == person.Id)
                .OrderByDescending(z => z.DutyStartDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (astronautDuty != null)
            {
                astronautDuty.DutyEndDate = request.DutyStartDate.AddDays(-1).Date;
            }

            var newAstronautDuty = new AstronautDuty()
            {
                PersonId = person.Id,
                Rank = request.Rank,
                DutyTitle = request.DutyTitle,
                DutyStartDate = request.DutyStartDate.Date,
                DutyEndDate = null
            };

            await _context.AstronautDuties.AddAsync(newAstronautDuty);

            await _context.SaveChangesAsync();

            return new CreateAstronautDutyResult()
            {
                Id = newAstronautDuty.Id
            };
        }
    }

    public class CreateAstronautDutyResult : BaseResponse
    {
        public int? Id { get; set; }
    }
}
