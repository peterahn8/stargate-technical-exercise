import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { API_BASE } from './api-base';
import { GetPeopleResult, GetPersonResult, IdResult, PersonAstronaut } from './models';

/** The API's /Person endpoints. */
@Injectable({ providedIn: 'root' })
export class PeopleService {
  private readonly http = inject(HttpClient);

  getPeople(): Observable<PersonAstronaut[]> {
    return this.http.get<GetPeopleResult>(`${API_BASE}/Person`).pipe(map((result) => result.people));
  }

  getPerson(name: string): Observable<PersonAstronaut> {
    return this.http
      .get<GetPersonResult>(`${API_BASE}/Person/${encodeURIComponent(name)}`)
      .pipe(map((result) => result.person));
  }

  /** Adds a person and returns their id. */
  createPerson(name: string): Observable<number> {
    return this.http
      .post<IdResult>(`${API_BASE}/Person`, jsonString(name), jsonOptions)
      .pipe(map((result) => result.id));
  }

  /** Renames a person (D16) and returns their id. */
  renamePerson(name: string, newName: string): Observable<number> {
    return this.http
      .put<IdResult>(`${API_BASE}/Person/${encodeURIComponent(name)}`, jsonString(newName), jsonOptions)
      .pipe(map((result) => result.id));
  }
}

// The API takes a bare JSON string as the body. HttpClient would send a plain string as text/plain,
// which ASP.NET rejects, so it's serialized and labelled as JSON explicitly.
const jsonOptions = { headers: new HttpHeaders({ 'Content-Type': 'application/json' }) };

function jsonString(value: string): string {
  return JSON.stringify(value);
}
