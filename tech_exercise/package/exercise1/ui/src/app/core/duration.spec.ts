import { dayAfter, describeDuration } from './duration';

describe('[UI-3] describeDuration', () => {
  it('describes years and months', () => {
    expect(describeDuration(new Date(2018, 0, 15), new Date(2020, 6, 1))).toBe('2 years, 5 months');
  });

  it('uses singular units', () => {
    expect(describeDuration(new Date(2020, 0, 1), new Date(2021, 1, 1))).toBe('1 year, 1 month');
  });

  it('leaves out a zero part', () => {
    expect(describeDuration(new Date(2020, 0, 1), new Date(2023, 0, 1))).toBe('3 years');
    expect(describeDuration(new Date(2020, 0, 1), new Date(2020, 7, 1))).toBe('7 months');
  });

  it("doesn't count an incomplete month", () => {
    expect(describeDuration(new Date(2020, 0, 15), new Date(2020, 1, 14))).toBe('less than a month');
  });
});

describe('[UI-3] dayAfter', () => {
  it('turns an inclusive end date into an exclusive one, across a month end', () => {
    const next = dayAfter('2020-06-30T00:00:00');

    expect([next.getFullYear(), next.getMonth(), next.getDate()]).toEqual([2020, 6, 1]);
  });
});
