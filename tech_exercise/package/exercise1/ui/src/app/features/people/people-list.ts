import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { Router, RouterLink } from '@angular/router';
import { PersonAstronaut } from '../../core/api/models';
import { PeopleService } from '../../core/api/people.service';
import { AstronautStatus, astronautStatus, statusLabels } from '../../core/astronaut-status';
import { AddPersonDialog } from './add-person-dialog';

type StatusFilter = AstronautStatus | 'all';

/** Everyone tracked by ACTS, with search and a status filter (T31, UI-1). */
@Component({
  selector: 'app-people-list',
  imports: [
    DatePipe,
    RouterLink,
    MatButtonModule,
    MatChipsModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressBarModule,
    MatTableModule,
  ],
  templateUrl: './people-list.html',
  styleUrl: './people-list.scss',
})
export class PeopleList {
  private readonly peopleService = inject(PeopleService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);

  protected readonly people = rxResource({
    stream: () => this.peopleService.getPeople(),
    defaultValue: [],
  });
  protected readonly search = signal('');
  protected readonly statusFilter = signal<StatusFilter>('all');

  protected readonly columns = ['name', 'rank', 'title', 'career', 'status'];
  protected readonly filterOptions: { value: StatusFilter; label: string }[] = [
    { value: 'all', label: 'All' },
    { value: 'active', label: statusLabels.active },
    { value: 'retired', label: statusLabels.retired },
    { value: 'none', label: statusLabels.none },
  ];

  /** Everyone, sorted by name. Empty while loading or after an error. */
  private readonly everyone = computed(() =>
    this.people.hasValue()
      ? [...this.people.value()].sort((a, b) => a.name.localeCompare(b.name))
      : [],
  );

  protected readonly visible = computed(() => {
    const term = this.search().trim().toLowerCase();
    const filter = this.statusFilter();
    return this.everyone().filter(
      (person) =>
        (filter === 'all' || astronautStatus(person) === filter) &&
        (!term ||
          [person.name, person.currentRank, person.currentDutyTitle].some((text) =>
            text?.toLowerCase().includes(term),
          )),
    );
  });

  protected readonly total = computed(() => this.everyone().length);
  protected readonly errorMessage = computed(() => this.people.error()?.message);

  protected status(person: PersonAstronaut): AstronautStatus {
    return astronautStatus(person);
  }

  protected statusLabel(person: PersonAstronaut): string {
    return statusLabels[astronautStatus(person)];
  }

  protected setSearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }

  protected addPerson(): void {
    this.dialog
      .open<AddPersonDialog, void, string>(AddPersonDialog)
      .afterClosed()
      .subscribe((name) => {
        if (name) {
          this.people.reload();
        }
      });
  }

  protected open(person: PersonAstronaut): void {
    void this.router.navigate(['/people', person.name]);
  }
}
