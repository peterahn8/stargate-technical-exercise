import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { AstronautDuty } from './api/models';
import { isRetiredTitle } from './astronaut-status';

/** Required, and not just spaces: the API trims input and rejects blank text (D13). */
export const notBlank: ValidatorFn = (control: AbstractControl<string>): ValidationErrors | null =>
  control.value?.trim() ? null : { required: true };

/**
 * Mirrors the API's duty rules (D1, D2) on an add-duty form, so the user sees a problem before
 * submitting. The API still checks them; these only save a round trip. Expects `dutyTitle` and
 * `dutyStartDate` controls, and reports each error with the API's own wording.
 */
export function dutyRules(currentDuty: AstronautDuty | null): ValidatorFn {
  return (form: AbstractControl): ValidationErrors | null => {
    const title: string = form.get('dutyTitle')?.value ?? '';
    const start: Date | null = form.get('dutyStartDate')?.value ?? null;
    const retiring = isRetiredTitle(title);

    if (retiring && !currentDuty) {
      return { dutyRule: "RETIRED can't be a person's first duty." };
    }
    if (retiring && isRetiredTitle(currentDuty?.dutyTitle)) {
      return { dutyRule: 'The person is already retired.' };
    }
    if (start && currentDuty && toIsoDate(start) <= currentDuty.dutyStartDate.slice(0, 10)) {
      return {
        dutyRule: `A new duty must start after the current duty's start date (${currentDuty.dutyStartDate.slice(0, 10)}).`,
      };
    }
    return null;
  };
}

/** A local date as the API's ISO date, e.g. 2024-01-31. */
export function toIsoDate(date: Date): string {
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}
