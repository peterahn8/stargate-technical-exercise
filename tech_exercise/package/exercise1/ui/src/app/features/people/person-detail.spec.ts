import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Title } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { apiErrorInterceptor } from '../../core/api/api-error';
import { GetAstronautDutiesResult } from '../../core/api/models';
import { PersonDetail } from './person-detail';

// George Hammond from the seed data: retired in 2013, back in service since 2016.
const george: GetAstronautDutiesResult = {
  success: true,
  message: 'Successful',
  responseCode: 200,
  person: {
    personId: 4,
    name: 'George Hammond',
    currentRank: 'MG',
    currentDutyTitle: 'Commander',
    careerStartDate: '2005-01-01T00:00:00',
    careerEndDate: null,
  },
  astronautDuties: [
    { id: 9, rank: 'MG', dutyTitle: 'Commander', dutyStartDate: '2016-06-01T00:00:00', dutyEndDate: null },
    { id: 8, rank: 'COL', dutyTitle: 'RETIRED', dutyStartDate: '2013-01-01T00:00:00', dutyEndDate: '2016-05-31T00:00:00' },
    { id: 7, rank: 'COL', dutyTitle: 'Commander', dutyStartDate: '2005-01-01T00:00:00', dutyEndDate: '2012-12-31T00:00:00' },
  ],
};

describe('[UI-2] PersonDetail', () => {
  let fixture: ComponentFixture<PersonDetail>;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [PersonDetail],
      providers: [
        provideHttpClient(withInterceptors([apiErrorInterceptor])),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    });
    fixture = TestBed.createComponent(PersonDetail);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  async function open(name: string, respond: (url: string) => void): Promise<void> {
    fixture.componentRef.setInput('name', name);
    TestBed.tick(); // Starts the request; see people-list.spec.ts.
    respond(`/api/AstronautDuty/${encodeURIComponent(name)}`);
    await fixture.whenStable();
  }

  function element(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function texts(selector: string): string[] {
    return Array.from(element().querySelectorAll(selector), (node) => node.textContent?.trim() ?? '');
  }

  it("retrieves the person's duties from the API by name", async () => {
    await open('George Hammond', (url) => http.expectOne({ method: 'GET', url }).flush(george));

    expect(texts('h1')).toEqual(['George Hammond']);
    expect(texts('.current')[0]).toBe('MG · Commander');
    expect(texts('.career')[0]).toContain('Jan 1, 2005 – present');
  });

  it('shows every duty newest first, highlighting the current one', async () => {
    await open('George Hammond', (url) => http.expectOne(url).flush(george));

    expect(texts('.entry .duty-title')).toEqual(['Commander', 'RETIRED', 'Commander']);
    expect(element().querySelectorAll('.entry.current')).toHaveLength(1);
    expect(element().querySelector('.entry.current .duty-title')?.textContent).toBe('Commander');
    expect(texts('.entry .dates')[1]).toContain('Jan 1, 2013 – May 31, 2016');
  });

  it('marks retirement and the return to service', async () => {
    await open('George Hammond', (url) => http.expectOne(url).flush(george));

    expect(element().querySelector('.entry.retired .duty-title')?.textContent).toBe('RETIRED');
    expect(texts('.tag-returned')).toEqual(['Returned to service']);
  });

  it('says when a person has never been an astronaut', async () => {
    const jane = {
      ...george,
      person: { ...george.person, name: 'Jane Doe', currentRank: null, currentDutyTitle: null, careerStartDate: null },
      astronautDuties: [],
    };

    await open('Jane Doe', (url) => http.expectOne(url).flush(jane));

    expect(texts('.current')).toEqual(['Not an astronaut yet.']);
    expect(texts('.empty')).toEqual(['No astronaut duties yet.']);
    expect(texts('.status')).toEqual(['Not an astronaut']);
  });

  it("shows the API's message and a way back for an unknown person", async () => {
    await open('Nobody', (url) =>
      http
        .expectOne(url)
        .flush(
          { person: null, success: false, message: "Person 'Nobody' not found.", responseCode: 404 },
          { status: 404, statusText: 'Not Found' },
        ),
    );

    expect(texts('.error .message')).toEqual(["Person 'Nobody' not found."]);
    expect(element().querySelector('.error a')?.getAttribute('href')).toBe('/people');
  });

  it("names the browser tab after the person", async () => {
    await open('George Hammond', (url) => http.expectOne(url).flush(george));

    expect(TestBed.inject(Title).getTitle()).toBe('George Hammond · Stargate');
  });
});
