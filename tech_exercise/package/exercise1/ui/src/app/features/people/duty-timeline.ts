import { AstronautDuty } from '../../core/api/models';
import { isRetiredTitle } from '../../core/astronaut-status';
import { dayAfter, describeDuration } from '../../core/duration';

export interface TimelineEntry {
  duty: AstronautDuty;
  /** The person's current duty: the one with no end date (RULE-4). */
  current: boolean;
  retired: boolean;
  /** A duty that followed a RETIRED one: the person returned to service (D2). */
  returnedToService: boolean;
  /** How long the duty lasted, or has lasted so far. */
  duration: string;
}

/** Turns a person's duties (newest first, as the API returns them) into timeline entries. */
export function toTimeline(duties: AstronautDuty[], today = new Date()): TimelineEntry[] {
  return duties.map((duty, index) => {
    const previous = duties[index + 1]; // The one before it in time.
    const retired = isRetiredTitle(duty.dutyTitle);
    return {
      duty,
      current: duty.dutyEndDate === null,
      retired,
      returnedToService: !retired && isRetiredTitle(previous?.dutyTitle),
      duration: describeDuration(
        new Date(duty.dutyStartDate),
        duty.dutyEndDate ? dayAfter(duty.dutyEndDate) : today,
      ),
    };
  });
}
