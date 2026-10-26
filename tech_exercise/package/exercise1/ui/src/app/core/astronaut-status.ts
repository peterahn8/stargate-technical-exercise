import { PersonAstronaut } from './api/models';

export type AstronautStatus = 'active' | 'retired' | 'none';

export const statusLabels: Record<AstronautStatus, string> = {
  active: 'Active',
  retired: 'Retired',
  none: 'Not an astronaut',
};

/**
 * Whether a duty title means retirement (RULE-6), ignoring case and surrounding spaces as the API does
 * (D4). The UI's single place for this, like DutyTitles.IsRetired in the API.
 */
export function isRetiredTitle(title: string | null | undefined): boolean {
  return title?.trim().toLowerCase() === 'retired';
}

/** A person's astronaut status, decided by their current duty (RULE-6, D2). */
export function astronautStatus(person: PersonAstronaut): AstronautStatus {
  if (!person.currentDutyTitle) {
    return 'none';
  }
  return isRetiredTitle(person.currentDutyTitle) ? 'retired' : 'active';
}
