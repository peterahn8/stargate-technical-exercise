import { PersonAstronaut } from './api/models';

export type AstronautStatus = 'active' | 'retired' | 'none';

export const statusLabels: Record<AstronautStatus, string> = {
  active: 'Active',
  retired: 'Retired',
  none: 'Not an astronaut',
};

/**
 * A person's astronaut status, decided by their current duty (RULE-6, D2). RETIRED is compared
 * ignoring case, as the API does (D4).
 */
export function astronautStatus(person: PersonAstronaut): AstronautStatus {
  if (!person.currentDutyTitle) {
    return 'none';
  }
  return person.currentDutyTitle.toLowerCase() === 'retired' ? 'retired' : 'active';
}
