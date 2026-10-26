import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { apiErrorInterceptor } from '../../core/api/api-error';
import { ProcessLogEntry } from '../../core/api/models';
import { RecentActivity } from './recent-activity';

function entry(
  requestType: string,
  outcome: 'Success' | 'Failure',
  level: ProcessLogEntry['level'],
  message: string,
): ProcessLogEntry {
  return {
    timestamp: '2026-10-26T14:34:22Z',
    requestType,
    outcome,
    level,
    message,
    durationMs: 12,
  };
}

const logs: ProcessLogEntry[] = [
  entry('CreateAstronautDuty', 'Failure', 'Warning', "RETIRED can't be a person's first duty."),
  entry('CreatePerson', 'Success', 'Information', 'Successful'),
  entry('GetPeople', 'Failure', 'Error', 'An unexpected error occurred.'),
];

describe('[UI-3] RecentActivity', () => {
  let fixture: ComponentFixture<RecentActivity>;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [RecentActivity],
      providers: [
        provideHttpClient(withInterceptors([apiErrorInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    fixture = TestBed.createComponent(RecentActivity);
    http = TestBed.inject(HttpTestingController);
    TestBed.tick(); // Starts the load; see people-list.spec.ts.
  });

  afterEach(() => http.verify());

  async function respond(body: object, status = 200): Promise<void> {
    http
      .expectOne('/api/ProcessLog?take=20')
      .flush(body, { status, statusText: status === 200 ? 'OK' : 'Error' });
    await fixture.whenStable();
  }

  function element(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function texts(selector: string): string[] {
    return Array.from(
      element().querySelectorAll(selector),
      (node) => node.textContent?.trim() ?? '',
    );
  }

  it('lists recent operations in plain English, newest first', async () => {
    await respond({ logs, success: true });

    expect(texts('.operation')).toEqual(['Add duty', 'Add person', 'List people']);
    expect(texts('.meta')[0]).toContain('12 ms');
  });

  it('shows how each ended, with the message only for failures', async () => {
    await respond({ logs, success: true });

    expect(texts('.icon')).toEqual(['warning', 'check_circle', 'error']);
    expect(texts('.message')).toEqual([
      "RETIRED can't be a person's first duty.",
      'An unexpected error occurred.',
    ]);
    expect(texts('.visually-hidden')).toEqual(['Failed:', 'Succeeded:', 'Failed:']);
  });

  it('shows placeholders until the first entries arrive, then says when there are none', async () => {
    expect(element().querySelectorAll('.skeleton.row')).toHaveLength(3);

    await respond({ logs: [], success: true });

    expect(texts('.empty')).toEqual(['No activity yet.']);
  });

  it('fetches again when refreshed', async () => {
    await respond({ logs: [], success: true });

    element().querySelector<HTMLButtonElement>('button[aria-label="Refresh activity"]')!.click();
    TestBed.tick();
    await respond({ logs, success: true });

    expect(texts('.operation')).toHaveLength(3);
  });

  it("shows the API's message when the feed can't be loaded", async () => {
    await respond({ success: false, message: "The API couldn't be reached. Is it running?" }, 502);

    expect(texts('.error')).toEqual(["The API couldn't be reached. Is it running?"]);
  });
});
