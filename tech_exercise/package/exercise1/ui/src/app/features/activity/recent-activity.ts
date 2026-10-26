import { DatePipe } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ProcessLogEntry } from '../../core/api/models';
import { ProcessLogService } from '../../core/api/process-log.service';

/** Plain-English names for the API's operations, worded to fit both successes and failures. */
const operations: Record<string, string> = {
  CreatePerson: 'Add person',
  UpdatePerson: 'Rename person',
  CreateAstronautDuty: 'Add duty',
  GetPeople: 'List people',
  GetPersonByName: 'Look up person',
  GetAstronautDutiesByName: 'View duties',
};

/** The process log's most recent entries: what the API has done, how it ended, how long it took (T41, UI-3). */
@Component({
  selector: 'app-recent-activity',
  imports: [DatePipe, MatButtonModule, MatIconModule, MatProgressBarModule],
  templateUrl: './recent-activity.html',
  styleUrl: './recent-activity.scss',
})
export class RecentActivity {
  private readonly processLog = inject(ProcessLogService);

  protected readonly logs = rxResource({
    stream: () => this.processLog.getRecent(),
    defaultValue: [],
  });
  protected readonly entries = computed(() => (this.logs.hasValue() ? this.logs.value() : []));
  protected readonly errorMessage = computed(() => this.logs.error()?.message);
  protected readonly skeletonRows = [1, 2, 3];

  /** Fetches the latest entries, e.g. after the page has changed something. */
  reload(): void {
    this.logs.reload();
  }

  protected operation(entry: ProcessLogEntry): string {
    return operations[entry.requestType] ?? entry.requestType;
  }

  protected icon(entry: ProcessLogEntry): string {
    if (entry.outcome === 'Success') {
      return 'check_circle';
    }
    return entry.level === 'Error' ? 'error' : 'warning';
  }
}
