import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { apiErrorInterceptor } from './api-error';
import { PersonAstronaut } from './models';
import { PeopleService } from './people.service';

const john: PersonAstronaut = {
  personId: 2,
  name: 'John Doe',
  currentRank: 'CPT',
  currentDutyTitle: 'Commander',
  careerStartDate: '2018-01-15T00:00:00',
  careerEndDate: null,
};

describe('[UI-1] PeopleService', () => {
  let service: PeopleService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([apiErrorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(PeopleService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('gets everyone from GET /api/Person', async () => {
    const people = firstValueFrom(service.getPeople());

    http.expectOne({ method: 'GET', url: '/api/Person' }).flush({ people: [john], success: true });

    expect(await people).toEqual([john]);
  });

  it("gets one person by name, encoding names like O'Neill safely", async () => {
    const person = firstValueFrom(service.getPerson('Jack O Neill'));

    http
      .expectOne({ method: 'GET', url: '/api/Person/Jack%20O%20Neill' })
      .flush({ person: john, success: true });

    expect(await person).toEqual(john);
  });

  it('creates a person by posting the name as a JSON string', async () => {
    const id = firstValueFrom(service.createPerson('John Doe'));

    const request = http.expectOne({ method: 'POST', url: '/api/Person' });
    expect(request.request.body).toBe('"John Doe"');
    expect(request.request.headers.get('Content-Type')).toBe('application/json');
    request.flush({ id: 5, success: true });

    expect(await id).toBe(5);
  });

  it('renames a person with PUT /api/Person/{name}', async () => {
    const id = firstValueFrom(service.renamePerson('John Doe', 'Jonathan Doe'));

    const request = http.expectOne({ method: 'PUT', url: '/api/Person/John%20Doe' });
    expect(request.request.body).toBe('"Jonathan Doe"');
    request.flush({ id: 2, success: true });

    expect(await id).toBe(2);
  });
});
