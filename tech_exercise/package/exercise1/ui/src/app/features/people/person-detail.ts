import { DatePipe } from '@angular/common';
import { Component, computed, effect, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Title } from '@angular/platform-browser';
import { RouterLink } from '@angular/router';
import { AstronautDutyService } from '../../core/api/astronaut-duty.service';
import { AstronautDuty, NewDuty } from '../../core/api/models';
import { astronautStatus, isRetiredTitle, statusLabels } from '../../core/astronaut-status';
import { dayAfter, describeDuration } from '../../core/duration';
import { AddDutyData, AddDutyDialog } from './add-duty-dialog';
import { toTimeline } from './duty-timeline';

/** A person's current astronaut details and their duty history as a timeline (T32, UI-2, UI-3). */
@Component({
  selector: 'app-person-detail',
  imports: [DatePipe, RouterLink, MatButtonModule, MatIconModule, MatProgressBarModule],
  templateUrl: './person-detail.html',
  styleUrl: './person-detail.scss',
})
export class PersonDetail {
  /** From the route: /people/:name. */
  readonly name = input.required<string>();

  private readonly dutyService = inject(AstronautDutyService);
  private readonly title = inject(Title);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly details = rxResource({
    params: () => this.name(),
    stream: ({ params }) => this.dutyService.getDuties(params),
  });

  protected readonly result = computed(() =>
    this.details.hasValue() ? this.details.value() : undefined,
  );
  protected readonly errorMessage = computed(() => this.details.error()?.message);
  protected readonly timeline = computed(() => toTimeline(this.result()?.astronautDuties ?? []));
  protected readonly skeletonCards = [1, 2, 3];

  protected readonly status = computed(() => {
    const person = this.result()?.person;
    return person ? astronautStatus(person) : 'none';
  });
  protected readonly statusLabel = computed(() => statusLabels[this.status()]);

  protected readonly careerLength = computed(() => {
    const person = this.result()?.person;
    if (!person?.careerStartDate) {
      return null;
    }
    const end = person.careerEndDate ? dayAfter(person.careerEndDate) : new Date();
    return describeDuration(new Date(person.careerStartDate), end);
  });

  constructor() {
    effect(() => this.title.setTitle(`${this.result()?.person.name ?? this.name()} · Stargate`));
  }

  protected addDuty(): void {
    const result = this.result();
    if (!result) {
      return;
    }

    // Duties arrive newest first, so the first is the current one.
    const currentDuty = result.astronautDuties[0] ?? null;
    this.dialog
      .open<AddDutyDialog, AddDutyData, NewDuty>(AddDutyDialog, {
        data: { name: result.person.name, currentDuty },
      })
      .afterClosed()
      .subscribe((added) => {
        if (!added) {
          return;
        }
        this.details.reload();
        this.snackBar.open(describeAddedDuty(added, currentDuty), undefined, { duration: 6000 });
      });
  }
}

/** A confirmation that names what happened to the person's career (RULE-6, D2). */
export function describeAddedDuty(added: NewDuty, previous: AstronautDuty | null): string {
  if (isRetiredTitle(added.dutyTitle)) {
    return `${added.name} retired.`;
  }
  if (isRetiredTitle(previous?.dutyTitle)) {
    return `${added.name} returned to service as ${added.dutyTitle}.`;
  }
  return `${added.dutyTitle} added for ${added.name}.`;
}
