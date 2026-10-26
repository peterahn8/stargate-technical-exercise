import { PersonAstronaut } from './api/models';
import { astronautStatus } from './astronaut-status';

function withTitle(title: string | null): PersonAstronaut {
  return { personId: 1, name: 'John Doe', currentRank: null, currentDutyTitle: title, careerStartDate: null, careerEndDate: null };
}

describe('[UI-1] astronautStatus', () => {
  it('is "none" for someone who has never been an astronaut', () => {
    expect(astronautStatus(withTitle(null))).toBe('none');
  });

  it('is "active" for any duty other than RETIRED', () => {
    expect(astronautStatus(withTitle('Commander'))).toBe('active');
  });

  it('is "retired" for RETIRED, ignoring case like the API', () => {
    expect(['RETIRED', 'Retired', 'retired'].map((title) => astronautStatus(withTitle(title)))).toEqual([
      'retired',
      'retired',
      'retired',
    ]);
  });
});
