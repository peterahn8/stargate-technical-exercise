import { FormControl, FormGroup } from '@angular/forms';
import { AstronautDuty } from './api/models';
import { dutyRules, notBlank, toIsoDate } from './validators';

function duty(title: string, start: string): AstronautDuty {
  return {
    id: 1,
    rank: 'CPT',
    dutyTitle: title,
    dutyStartDate: `${start}T00:00:00`,
    dutyEndDate: null,
  };
}

function form(current: AstronautDuty | null, title: string, start: Date | null): FormGroup {
  return new FormGroup(
    { dutyTitle: new FormControl(title), dutyStartDate: new FormControl<Date | null>(start) },
    { validators: [dutyRules(current)] },
  );
}

describe('[UI-1] notBlank', () => {
  it('rejects empty and whitespace-only text, like the API', () => {
    expect(['', '   '].map((value) => notBlank(new FormControl(value)))).toEqual([
      { required: true },
      { required: true },
    ]);
    expect(notBlank(new FormControl(' John Doe '))).toBeNull();
  });
});

describe('[UI-1] dutyRules', () => {
  const commander = duty('Commander', '2024-01-01');

  it('accepts a duty that starts after the current one', () => {
    expect(form(commander, 'Pilot', new Date(2024, 0, 2)).errors).toBeNull();
  });

  it("rejects a duty that doesn't start after the current one (D1)", () => {
    const message = "A new duty must start after the current duty's start date (2024-01-01).";
    expect(form(commander, 'Pilot', new Date(2024, 0, 1)).errors).toEqual({ dutyRule: message });
    expect(form(commander, 'Pilot', new Date(2023, 5, 1)).errors).toEqual({ dutyRule: message });
  });

  it('rejects RETIRED as a first duty, ignoring case (D2)', () => {
    expect(form(null, ' retired ', new Date(2024, 0, 1)).errors).toEqual({
      dutyRule: "RETIRED can't be a person's first duty.",
    });
  });

  it('rejects RETIRED for someone already retired (D2)', () => {
    expect(form(duty('RETIRED', '2024-01-01'), 'Retired', new Date(2025, 0, 1)).errors).toEqual({
      dutyRule: 'The person is already retired.',
    });
  });

  it('allows a first duty on any date', () => {
    expect(form(null, 'Pilot', new Date(1990, 0, 1)).errors).toBeNull();
  });
});

describe('[UI-1] toIsoDate', () => {
  it('formats a local date the way the API expects', () => {
    expect(toIsoDate(new Date(2025, 0, 5))).toBe('2025-01-05');
  });
});
