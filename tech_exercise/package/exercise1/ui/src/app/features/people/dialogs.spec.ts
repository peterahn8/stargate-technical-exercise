import { HarnessLoader } from '@angular/cdk/testing';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { MatDatepickerInputHarness } from '@angular/material/datepicker/testing';
import { MatDialog } from '@angular/material/dialog';
import { MatInputHarness } from '@angular/material/input/testing';
import { apiErrorInterceptor } from '../../core/api/api-error';
import { AstronautDuty, NewDuty } from '../../core/api/models';
import { AddDutyData, AddDutyDialog } from './add-duty-dialog';
import { AddPersonDialog } from './add-person-dialog';

/** Something to attach the dialogs to; they render in an overlay at the document root. */
@Component({ template: '' })
class Host {}

let fixture: ComponentFixture<Host>;
let http: HttpTestingController;
let loader: HarnessLoader;

beforeEach(() => {
  TestBed.configureTestingModule({
    providers: [
      provideHttpClient(withInterceptors([apiErrorInterceptor])),
      provideHttpClientTesting(),
      // Dialogs report "closed" after their close animation, which never finishes in tests.
      { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
    ],
  });
  fixture = TestBed.createComponent(Host);
  http = TestBed.inject(HttpTestingController);
  loader = TestbedHarnessEnvironment.documentRootLoader(fixture);
});

afterEach(() => {
  http.verify();
  TestBed.inject(MatDialog).closeAll();
});

function dialogText(): string {
  return document.querySelector('mat-dialog-container')?.textContent ?? '';
}

/** Submits with a plain click: harnesses wait for pending requests, which the test answers afterwards. */
async function submit(): Promise<void> {
  document.querySelector<HTMLButtonElement>('mat-dialog-container button[type=submit]')!.click();
  TestBed.tick();
}

async function input(control: string): Promise<MatInputHarness> {
  return loader.getHarness(MatInputHarness.with({ selector: `[formControlName=${control}]` }));
}

describe('[UI-1] AddPersonDialog', () => {
  let result: string | undefined | null;

  beforeEach(async () => {
    result = null;
    TestBed.inject(MatDialog)
      .open<AddPersonDialog, void, string>(AddPersonDialog)
      .afterClosed()
      .subscribe((name) => (result = name));
    await fixture.whenStable();
  });

  it("doesn't call the API for a blank name", async () => {
    await (await loader.getHarness(MatInputHarness)).setValue('   ');

    await submit();
    await fixture.whenStable();

    expect(dialogText()).toContain('A name is required.');
    http.expectNone('/api/Person');
  });

  it('adds the trimmed name and closes with it', async () => {
    await (await loader.getHarness(MatInputHarness)).setValue('  Samantha Carter ');

    await submit();
    const request = http.expectOne({ method: 'POST', url: '/api/Person' });
    expect(request.request.body).toBe('"Samantha Carter"');
    request.flush({ id: 5, success: true });
    await fixture.whenStable();

    expect(result).toBe('Samantha Carter');
  });

  it("shows the API's message and stays open when the name is taken", async () => {
    await (await loader.getHarness(MatInputHarness)).setValue('john doe');

    await submit();
    http
      .expectOne('/api/Person')
      .flush(
        { success: false, message: "A person named 'john doe' already exists.", responseCode: 409 },
        { status: 409, statusText: 'Conflict' },
      );
    await fixture.whenStable();

    expect(dialogText()).toContain("A person named 'john doe' already exists.");
    expect(result).toBeNull();
  });
});

describe('[UI-1] AddDutyDialog', () => {
  const commander: AstronautDuty = {
    id: 3,
    rank: 'CPT',
    dutyTitle: 'Commander',
    dutyStartDate: '2023-01-01T00:00:00',
    dutyEndDate: null,
  };
  let result: NewDuty | undefined | null;

  async function open(currentDuty: AstronautDuty | null): Promise<void> {
    result = null;
    TestBed.inject(MatDialog)
      .open<AddDutyDialog, AddDutyData, NewDuty>(AddDutyDialog, {
        data: { name: 'John Doe', currentDuty },
      })
      .afterClosed()
      .subscribe((added) => (result = added));
    await fixture.whenStable();
  }

  async function fill(title: string, rank: string, start: string): Promise<void> {
    await (await input('dutyTitle')).setValue(title);
    await (await input('rank')).setValue(rank);
    await (await loader.getHarness(MatDatepickerInputHarness)).setValue(start);
  }

  it('shows the current duty, which the new one will end', async () => {
    await open(commander);

    expect(dialogText()).toContain('Current duty: Commander (CPT), since Jan 1, 2023');
  });

  it('adds the duty with an ISO start date and closes with it', async () => {
    await open(commander);
    await fill(' Pilot ', ' MAJ ', '6/1/2025');

    await submit();
    const request = http.expectOne({ method: 'POST', url: '/api/AstronautDuty' });
    expect(request.request.body).toEqual({
      name: 'John Doe',
      rank: 'MAJ',
      dutyTitle: 'Pilot',
      dutyStartDate: '2025-06-01',
    });
    request.flush({ id: 10, success: true });
    await fixture.whenStable();

    expect(result).toEqual({
      name: 'John Doe',
      rank: 'MAJ',
      dutyTitle: 'Pilot',
      dutyStartDate: '2025-06-01',
    });
  });

  it("catches a duty that doesn't start after the current one before calling the API", async () => {
    await open(commander);
    await fill('Pilot', 'MAJ', '1/1/2023');

    await submit();
    await fixture.whenStable();

    expect(dialogText()).toContain("It must start after the current duty's start date.");
    http.expectNone('/api/AstronautDuty');
  });

  it('catches RETIRED as a first duty before calling the API', async () => {
    await open(null);
    await fill('retired', 'CPT', '1/1/2024');

    await submit();
    await fixture.whenStable();

    expect(dialogText()).toContain("RETIRED can't be a person's first duty.");
    http.expectNone('/api/AstronautDuty');
  });

  it('requires every field', async () => {
    await open(null);

    await submit();
    await fixture.whenStable();

    expect(dialogText()).toContain('A duty title is required.');
    expect(dialogText()).toContain('A rank is required.');
    expect(dialogText()).toContain('A start date is required.');
  });

  it("shows the API's message when it still rejects the duty", async () => {
    await open(null);
    await fill('Pilot', 'CPT', '1/1/2024');

    await submit();
    http.expectOne('/api/AstronautDuty').flush(
      {
        success: false,
        message: 'The request conflicts with existing data. Please retry.',
        responseCode: 409,
      },
      { status: 409, statusText: 'Conflict' },
    );
    await fixture.whenStable();

    expect(dialogText()).toContain('The request conflicts with existing data. Please retry.');
    expect(result).toBeNull();
  });
});
