import { AstronautDuty } from '../../core/api/models';
import { toTimeline } from './duty-timeline';

function duty(id: number, title: string, start: string, end: string | null): AstronautDuty {
  return { id, rank: 'COL', dutyTitle: title, dutyStartDate: `${start}T00:00:00`, dutyEndDate: end ? `${end}T00:00:00` : null };
}

// George Hammond's career from the seed data, newest first: retired, then returned to service.
const george = [
  duty(3, 'Commander', '2016-06-01', null),
  duty(2, 'RETIRED', '2013-01-01', '2016-05-31'),
  duty(1, 'Commander', '2005-01-01', '2012-12-31'),
];

describe('[UI-3] toTimeline', () => {
  const timeline = toTimeline(george, new Date(2026, 9, 26));

  it('marks only the duty with no end date as current', () => {
    expect(timeline.map((entry) => entry.current)).toEqual([true, false, false]);
  });

  it('marks RETIRED duties, ignoring case', () => {
    expect(timeline.map((entry) => entry.retired)).toEqual([false, true, false]);
    expect(toTimeline([duty(1, 'Retired', '2020-01-01', null)])[0].retired).toBe(true);
  });

  it('marks a duty that follows a RETIRED one as a return to service', () => {
    expect(timeline.map((entry) => entry.returnedToService)).toEqual([true, false, false]);
  });

  it("measures ended duties to their end date and the current one to today", () => {
    expect(timeline.map((entry) => entry.duration)).toEqual(['10 years, 4 months', '3 years, 5 months', '8 years']);
  });
});
