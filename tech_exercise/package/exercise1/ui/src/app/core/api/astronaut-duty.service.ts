import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { API_BASE } from './api-base';
import { GetAstronautDutiesResult, IdResult, NewDuty } from './models';

/** The API's /AstronautDuty endpoints. */
@Injectable({ providedIn: 'root' })
export class AstronautDutyService {
  private readonly http = inject(HttpClient);

  /** A person's current details and all their duties, newest first. */
  getDuties(name: string): Observable<GetAstronautDutiesResult> {
    return this.http.get<GetAstronautDutiesResult>(
      `${API_BASE}/AstronautDuty/${encodeURIComponent(name)}`,
    );
  }

  /** Adds a duty and returns its id. */
  addDuty(duty: NewDuty): Observable<number> {
    return this.http
      .post<IdResult>(`${API_BASE}/AstronautDuty`, duty)
      .pipe(map((result) => result.id));
  }
}
