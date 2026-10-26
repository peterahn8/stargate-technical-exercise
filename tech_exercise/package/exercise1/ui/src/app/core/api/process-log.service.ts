import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { API_BASE } from './api-base';
import { GetProcessLogsResult, ProcessLogEntry } from './models';

/** The API's /ProcessLog endpoint (T41). */
@Injectable({ providedIn: 'root' })
export class ProcessLogService {
  private readonly http = inject(HttpClient);

  /** The most recent entries, newest first. The API keeps `take` between 1 and 100. */
  getRecent(take = 20): Observable<ProcessLogEntry[]> {
    return this.http
      .get<GetProcessLogsResult>(`${API_BASE}/ProcessLog`, { params: { take } })
      .pipe(map((result) => result.logs));
  }
}
