import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { apiErrorInterceptor } from './api-error';
import { AstronautDutyService } from './astronaut-duty.service';
import { GetAstronautDutiesResult } from './models';

describe('[UI-2] AstronautDutyService', () => {
  let service: AstronautDutyService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([apiErrorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(AstronautDutyService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it("retrieves an individual's astronaut duties from GET /api/AstronautDuty/{name}", async () => {
    const response: GetAstronautDutiesResult = {
      success: true,
      message: 'Successful',
      responseCode: 200,
      person: {
        personId: 2,
        name: 'John Doe',
        currentRank: 'CPT',
        currentDutyTitle: 'Commander',
        careerStartDate: '2018-01-15T00:00:00',
        careerEndDate: null,
      },
      astronautDuties: [
        {
          id: 3,
          rank: 'CPT',
          dutyTitle: 'Commander',
          dutyStartDate: '2023-01-01T00:00:00',
          dutyEndDate: null,
        },
      ],
    };
    const result = firstValueFrom(service.getDuties('John Doe'));

    http.expectOne({ method: 'GET', url: '/api/AstronautDuty/John%20Doe' }).flush(response);

    expect(await result).toEqual(response);
  });

  it('adds a duty with POST /api/AstronautDuty and returns its id', async () => {
    const duty = {
      name: 'John Doe',
      rank: 'MAJ',
      dutyTitle: 'Commander',
      dutyStartDate: '2025-01-01',
    };
    const id = firstValueFrom(service.addDuty(duty));

    const request = http.expectOne({ method: 'POST', url: '/api/AstronautDuty' });
    expect(request.request.body).toEqual(duty);
    request.flush({ id: 9, success: true });

    expect(await id).toBe(9);
  });
});
