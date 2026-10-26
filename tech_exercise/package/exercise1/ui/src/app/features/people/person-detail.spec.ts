import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { MatDialog } from '@angular/material/dialog';
import { Title } from '@angular/platform-browser';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import { apiErrorInterceptor } from '../../core/api/api-error';
import { AstronautDuty, GetAstronautDutiesResult, NewDuty } from '../../core/api/models';
import { describeAddedDuty, PersonDetail } from './person-detail';

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
    {
      id: 9,
      rank: 'MG',
      dutyTitle: 'Commander',
      dutyStartDate: '2016-06-01T00:00:00',
      dutyEndDate: null,
    },
    {
      id: 8,
      rank: 'COL',
      dutyTitle: 'RETIRED',
      dutyStartDate: '2013-01-01T00:00:00',
      dutyEndDate: '2016-05-31T00:00:00',
    },
    {
      id: 7,
      rank: 'COL',
      dutyTitle: 'Commander',
      dutyStartDate: '2005-01-01T00:00:00',
      dutyEndDate: '2012-12-31T00:00:00',
    },
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
    return Array.from(
      element().querySelectorAll(selector),
      (node) => node.textContent?.trim() ?? '',
    );
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

  it("shows a retired person's career end date", async () => {
    const jack = {
      ...george,
      person: {
        ...george.person,
        name: "Jack O'Neill",
        currentRank: 'COL',
        currentDutyTitle: 'RETIRED',
        careerStartDate: '2010-03-01T00:00:00',
        careerEndDate: '2021-12-31T00:00:00',
      },
    };

    await open("Jack O'Neill", (url) => http.expectOne(url).flush(jack));

    expect(texts('.career')[0]).toContain('Mar 1, 2010 – Dec 31, 2021 · 11 years, 10 months');
    expect(texts('.status')).toEqual(['Retired']);
  });

  it('says when a person has never been an astronaut', async () => {
    const jane = {
      ...george,
      person: {
        ...george.person,
        name: 'Jane Doe',
        currentRank: null,
        currentDutyTitle: null,
        careerStartDate: null,
      },
      astronautDuties: [],
    };

    await open('Jane Doe', (url) => http.expectOne(url).flush(jane));

    expect(texts('.current')).toEqual(['Not an astronaut yet.']);
    expect(texts('.empty')).toEqual(['No astronaut duties yet.']);
    expect(texts('.status')).toEqual(['Not an astronaut']);
  });

  it("shows the API's message and a way back for an unknown person", async () => {
    await open('Nobody', (url) =>
      http.expectOne(url).flush(
        {
          person: null,
          success: false,
          message: "Person 'Nobody' not found.",
          responseCode: 404,
        },
        { status: 404, statusText: 'Not Found' },
      ),
    );

    expect(texts('.error .message')).toEqual(["Person 'Nobody' not found."]);
    expect(element().querySelector('.error a')?.getAttribute('href')).toBe('/people');
  });

  it('names the browser tab after the person', async () => {
    await open('George Hammond', (url) => http.expectOne(url).flush(george));

    expect(TestBed.inject(Title).getTitle()).toBe('George Hammond · Stargate');
  });
});

describe('[UI-3] PersonDetail progress and results', () => {
  let fixture: ComponentFixture<PersonDetail>;
  let http: HttpTestingController;
  let added: NewDuty;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [PersonDetail],
      providers: [
        provideHttpClient(withInterceptors([apiErrorInterceptor])),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
        // The add-duty dialog itself is tested in dialogs.spec.ts; here it returns `added`.
        { provide: MatDialog, useValue: { open: () => ({ afterClosed: () => of(added) }) } },
      ],
    });
    fixture = TestBed.createComponent(PersonDetail);
    http = TestBed.inject(HttpTestingController);
    fixture.componentRef.setInput('name', 'George Hammond');
    TestBed.tick();
  });

  afterEach(() => http.verify());

  function element(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  it('shows a placeholder page, and tells screen readers, until the duties first arrive', async () => {
    // No whenStable() yet: the request is still unanswered (see above). TestBed.tick() has rendered.
    expect(element().querySelectorAll('.skeleton.card')).toHaveLength(3);
    expect(element().querySelector('[role=status]')?.textContent).toContain('Loading duties');

    http.expectOne('/api/AstronautDuty/George%20Hammond').flush(george);
    await fixture.whenStable();

    expect(element().querySelector('.skeleton')).toBeNull();
  });

  it('confirms a new duty and reloads, keeping the page visible', async () => {
    http.expectOne('/api/AstronautDuty/George%20Hammond').flush(george);
    await fixture.whenStable();
    added = {
      name: 'George Hammond',
      rank: 'MG',
      dutyTitle: 'RETIRED',
      dutyStartDate: '2026-11-01',
    };

    element().querySelector<HTMLButtonElement>('.add-duty')!.click();
    TestBed.tick();

    expect(element().querySelector('h1')?.textContent).toBe('George Hammond');
    expect(element().querySelector('.skeleton')).toBeNull();
    expect(document.querySelector('mat-snack-bar-container')?.textContent).toContain(
      'George Hammond retired.',
    );

    http.expectOne('/api/AstronautDuty/George%20Hammond').flush(george);
    await fixture.whenStable();
  });

  it("doesn't reload or confirm anything when the dialog is cancelled", async () => {
    http.expectOne('/api/AstronautDuty/George%20Hammond').flush(george);
    await fixture.whenStable();
    added = undefined as unknown as NewDuty;

    element().querySelector<HTMLButtonElement>('.add-duty')!.click();
    TestBed.tick();

    http.expectNone('/api/AstronautDuty/George%20Hammond');
    expect(document.querySelector('mat-snack-bar-container')).toBeNull();
  });
});

describe('[UI-3] describeAddedDuty', () => {
  const commander: AstronautDuty = {
    id: 1,
    rank: 'CPT',
    dutyTitle: 'Commander',
    dutyStartDate: '2023-01-01T00:00:00',
    dutyEndDate: null,
  };
  const retired: AstronautDuty = { ...commander, dutyTitle: 'RETIRED' };

  it('names a retirement', () => {
    expect(describeAddedDuty({ ...duty('Retired') }, commander)).toBe('John Doe retired.');
  });

  it('names a return to service (D2)', () => {
    expect(describeAddedDuty(duty('Commander'), retired)).toBe(
      'John Doe returned to service as Commander.',
    );
  });

  it('names an ordinary new duty, including a first one', () => {
    expect(describeAddedDuty(duty('Pilot'), commander)).toBe('Pilot added for John Doe.');
    expect(describeAddedDuty(duty('Pilot'), null)).toBe('Pilot added for John Doe.');
  });

  function duty(title: string): NewDuty {
    return { name: 'John Doe', rank: 'MAJ', dutyTitle: title, dutyStartDate: '2025-01-01' };
  }
});

describe('[UI-1] PersonDetail rename', () => {
  let fixture: ComponentFixture<PersonDetail>;
  let http: HttpTestingController;
  let newName: string | undefined;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      imports: [PersonDetail],
      providers: [
        provideHttpClient(withInterceptors([apiErrorInterceptor])),
        provideHttpClientTesting(),
        provideRouter([{ path: 'people/:name', component: PersonDetail }]),
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
        // The rename dialog itself is tested in dialogs.spec.ts; here it returns `newName`.
        { provide: MatDialog, useValue: { open: () => ({ afterClosed: () => of(newName) }) } },
      ],
    });
    fixture = TestBed.createComponent(PersonDetail);
    http = TestBed.inject(HttpTestingController);
    fixture.componentRef.setInput('name', 'George Hammond');
    TestBed.tick();
    http.expectOne('/api/AstronautDuty/George%20Hammond').flush(george);
    await fixture.whenStable();
  });

  afterEach(() => http.verify());

  function clickRename(): void {
    (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('.rename')!.click();
    TestBed.tick();
  }

  it("moves to the new name's address and confirms the rename", async () => {
    newName = 'General Hammond';

    clickRename();
    await fixture.whenStable();

    expect(TestBed.inject(Router).url).toBe('/people/General%20Hammond');
    expect(document.querySelector('mat-snack-bar-container')?.textContent).toContain(
      'George Hammond renamed to General Hammond.',
    );
  });

  it('stays put when the name is unchanged or the dialog is cancelled', async () => {
    for (const result of ['George Hammond', undefined]) {
      newName = result;
      clickRename();
      await fixture.whenStable();
    }

    expect(TestBed.inject(Router).url).toBe('/');
    expect(document.querySelector('mat-snack-bar-container')).toBeNull();
  });
});
