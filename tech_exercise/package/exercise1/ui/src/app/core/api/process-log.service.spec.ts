import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { apiErrorInterceptor } from './api-error';
import { ProcessLogEntry } from './models';
import { ProcessLogService } from './process-log.service';

describe('[UI-3] ProcessLogService', () => {
  let service: ProcessLogService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([apiErrorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(ProcessLogService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('gets the 20 most recent entries from GET /api/ProcessLog by default', async () => {
    const entry: ProcessLogEntry = {
      timestamp: '2026-10-26T14:34:22Z',
      requestType: 'CreatePerson',
      outcome: 'Success',
      level: 'Information',
      message: 'Successful',
      durationMs: 12,
    };
    const logs = firstValueFrom(service.getRecent());

    http
      .expectOne({ method: 'GET', url: '/api/ProcessLog?take=20' })
      .flush({ logs: [entry], success: true });

    expect(await logs).toEqual([entry]);
  });

  it('asks for a different number of entries', async () => {
    const logs = firstValueFrom(service.getRecent(5));

    http.expectOne('/api/ProcessLog?take=5').flush({ logs: [], success: true });

    expect(await logs).toEqual([]);
  });
});
