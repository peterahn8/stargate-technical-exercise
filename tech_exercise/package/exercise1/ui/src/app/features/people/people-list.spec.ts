import { HarnessLoader } from '@angular/cdk/testing';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatButtonHarness } from '@angular/material/button/testing';
import { MatChipListboxHarness } from '@angular/material/chips/testing';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { MatDialog } from '@angular/material/dialog';
import { MatInputHarness } from '@angular/material/input/testing';
import { MatProgressBarHarness } from '@angular/material/progress-bar/testing';
import { MatTableHarness } from '@angular/material/table/testing';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import { apiErrorInterceptor } from '../../core/api/api-error';
import { PersonAstronaut } from '../../core/api/models';
import { PeopleList } from './people-list';

function person(
  name: string,
  rank: string | null,
  title: string | null,
  start: string | null,
  end: string | null = null,
): PersonAstronaut {
  return {
    personId: 0,
    name,
    currentRank: rank,
    currentDutyTitle: title,
    careerStartDate: start,
    careerEndDate: end,
  };
}

/** Stands in for the person page, so navigation can be checked through the router's URL. */
@Component({ template: '' })
class PersonPage {}

const routes = [{ path: 'people/:name', component: PersonPage }];

const people: PersonAstronaut[] = [
  person('John Doe', 'CPT', 'Commander', '2018-01-15T00:00:00'),
  person('Jane Doe', null, null, null),
  person("Jack O'Neill", 'COL', 'RETIRED', '2010-03-01T00:00:00', '2021-12-31T00:00:00'),
  person('George Hammond', 'MG', 'Commander', '2005-01-01T00:00:00'),
];

describe('[UI-1] PeopleList', () => {
  let fixture: ComponentFixture<PeopleList>;
  let http: HttpTestingController;
  let loader: HarnessLoader;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      imports: [PeopleList],
      providers: [
        provideHttpClient(withInterceptors([apiErrorInterceptor])),
        provideHttpClientTesting(),
        provideRouter(routes),
      ],
    });
    fixture = TestBed.createComponent(PeopleList);
    http = TestBed.inject(HttpTestingController);
    loader = TestbedHarnessEnvironment.loader(fixture);
    // Starts the load without waiting for it: an unanswered request counts as pending work, so
    // whenStable() (which harnesses also use) only settles once a test has answered it.
    TestBed.tick();
  });

  afterEach(() => http.verify());

  async function respondWith(body: object, status = 200): Promise<void> {
    http
      .expectOne('/api/Person')
      .flush(body, { status, statusText: status === 200 ? 'OK' : 'Error' });
    await fixture.whenStable();
  }

  async function column(name: string): Promise<string[]> {
    const rows = await (await loader.getHarness(MatTableHarness)).getRows();
    return Promise.all(rows.map(async (row) => (await row.getCellTextByColumnName())[name]));
  }

  function element(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  function text(): string {
    return element().textContent ?? '';
  }

  it('lists everyone, sorted by name, with their astronaut status', async () => {
    await respondWith({ people, success: true });

    expect(await column('name')).toEqual([
      'George Hammond',
      "Jack O'Neill",
      'Jane Doe',
      'John Doe',
    ]);
    expect(await column('status')).toEqual(['Active', 'Retired', 'Not an astronaut', 'Active']);
    expect(text()).toContain('Showing 4 of 4');
  });

  it("shows '—' for a non-astronaut's rank, duty and career", async () => {
    await respondWith({ people: [people[1]], success: true });

    const cells = await (
      await (await loader.getHarness(MatTableHarness)).getRows()
    )[0].getCellTextByColumnName();
    expect([cells['rank'], cells['title'], cells['career']]).toEqual(['—', '—', '—']);
  });

  it('filters as you type, by name, rank or title, ignoring case', async () => {
    await respondWith({ people, success: true });
    const search = await loader.getHarness(MatInputHarness);

    await search.setValue('commander');
    expect(await column('name')).toEqual(['George Hammond', 'John Doe']);

    await search.setValue("o'NEILL");
    expect(await column('name')).toEqual(["Jack O'Neill"]);
    expect(text()).toContain('Showing 1 of 4');
  });

  it('filters by status', async () => {
    await respondWith({ people, success: true });
    const chips = await loader.getHarness(MatChipListboxHarness);

    await chips.selectChips({ text: 'Retired' });
    expect(await column('name')).toEqual(["Jack O'Neill"]);

    await chips.selectChips({ text: 'Not an astronaut' });
    expect(await column('name')).toEqual(['Jane Doe']);
  });

  it('says so when nothing matches', async () => {
    await respondWith({ people, success: true });

    await (await loader.getHarness(MatInputHarness)).setValue('zzz');

    expect(text()).toContain('No people match your search.');
  });

  it('says so when no one has been added yet', async () => {
    await respondWith({ people: [], success: true });

    expect(text()).toContain('No one has been added yet.');
  });

  it('shows a progress bar until the people arrive', async () => {
    expect(element().querySelector('mat-progress-bar')).not.toBeNull();

    await respondWith({ people, success: true });

    expect(await loader.getAllHarnesses(MatProgressBarHarness)).toHaveLength(0);
  });

  it("opens a person's page when their row is clicked", async () => {
    await respondWith({ people, success: true });

    element().querySelector<HTMLElement>('tr.row')!.click();
    await fixture.whenStable();

    expect(TestBed.inject(Router).url).toBe('/people/George%20Hammond');
  });

  it("shows the API's message and retries when asked", async () => {
    await respondWith(
      { success: false, message: 'An unexpected error occurred.', responseCode: 500 },
      500,
    );
    expect(text()).toContain('An unexpected error occurred.');

    expect(await loader.getAllHarnesses(MatButtonHarness.with({ text: 'Retry' }))).toHaveLength(1);
    element().querySelector<HTMLButtonElement>('.error button')!.click();
    TestBed.tick();
    await respondWith({ people, success: true });

    expect(await column('name')).toHaveLength(4);
  });
});

describe('[UI-3] PeopleList progress and results', () => {
  let fixture: ComponentFixture<PeopleList>;
  let http: HttpTestingController;
  let dialogResult: string | undefined;

  beforeEach(async () => {
    dialogResult = 'Samantha Carter';
    TestBed.configureTestingModule({
      imports: [PeopleList],
      providers: [
        provideHttpClient(withInterceptors([apiErrorInterceptor])),
        provideHttpClientTesting(),
        provideRouter(routes),
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
        // The add-person dialog itself is tested in dialogs.spec.ts; here it just adds Samantha.
        {
          provide: MatDialog,
          useValue: { open: () => ({ afterClosed: () => of(dialogResult) }) },
        },
      ],
    });
    fixture = TestBed.createComponent(PeopleList);
    http = TestBed.inject(HttpTestingController);
    TestBed.tick(); // Starts the first load; see above.
  });

  afterEach(() => http.verify());

  function element(): HTMLElement {
    return fixture.nativeElement as HTMLElement;
  }

  it('shows placeholder rows, and tells screen readers, until the people first arrive', async () => {
    // No whenStable() yet: the request is still unanswered (see above). TestBed.tick() has rendered.
    expect(element().querySelectorAll('.skeleton-row')).toHaveLength(4);
    expect(element().querySelector('[role=status]')?.textContent).toContain('Loading people');

    http.expectOne('/api/Person').flush({ people, success: true });
    await fixture.whenStable();

    expect(element().querySelector('.skeleton-row')).toBeNull();
  });

  it('confirms a new person, keeping the table visible while it reloads', async () => {
    http.expectOne('/api/Person').flush({ people, success: true });
    await fixture.whenStable();

    element().querySelector<HTMLButtonElement>('.page-header button')!.click();
    TestBed.tick();

    // Reloading: the table stays, with the progress bar above it, and no placeholders.
    expect(element().querySelector('table')).not.toBeNull();
    expect(element().querySelector('.skeleton-row')).toBeNull();
    expect(element().querySelector('mat-progress-bar')).not.toBeNull();
    expect(document.querySelector('mat-snack-bar-container')?.textContent).toContain(
      'Samantha Carter added.',
    );

    http.expectOne('/api/Person').flush({ people, success: true });
    await fixture.whenStable();
  });

  it("opens the new person from the snackbar's View button", async () => {
    http.expectOne('/api/Person').flush({ people, success: true });
    await fixture.whenStable();
    element().querySelector<HTMLButtonElement>('.page-header button')!.click();
    TestBed.tick();
    http.expectOne('/api/Person').flush({ people, success: true });
    await fixture.whenStable();

    document.querySelector<HTMLButtonElement>('mat-snack-bar-container button')!.click();
    await fixture.whenStable();

    expect(TestBed.inject(Router).url).toBe('/people/Samantha%20Carter');
  });

  it('changes nothing when the dialog is cancelled', async () => {
    http.expectOne('/api/Person').flush({ people, success: true });
    await fixture.whenStable();
    dialogResult = undefined;

    element().querySelector<HTMLButtonElement>('.page-header button')!.click();
    TestBed.tick();

    http.expectNone('/api/Person');
    expect(document.querySelector('mat-snack-bar-container')).toBeNull();
  });
});
