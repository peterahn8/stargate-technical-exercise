# Stargate (ACTS) — Work Plan

Living plan for the BAM technical exercise. Spec: [exercise1/README.md](../tech_exercise/package/exercise1/README.md).

Code paths below are relative to `tech_exercise/package/exercise1/api/`. Repo-root files (`.gitignore`, `.config/`, `docs/`) are named as such.

## Legend

### ID prefixes

| Prefix | Meaning | Numbered by |
|--------|---------|-------------|
| `RULE-n` | Acceptance criterion: business rule | Position in the spec's **Rules** list |
| `API-n` | Acceptance criterion: API requirement | Position in the spec's **Enhance the Stargate API** list |
| `UI-n` | Acceptance criterion: UI requirement | Position in the spec's **Implement a user interface** list |
| `SPEC-n` | Acceptance criterion: spec task | Position in the spec's **Tasks** list |
| `Tnn` | Task | Next free number; never reused or renumbered |
| `Dn` | Design decision | Next free number |

### Status

- **Acceptance criteria:** ✅ met and proven by a passing test · 🟡 partially met or untested · ❌ fails or missing · ⬜ not started
  A criterion is ✅ only when its tests pass, none of its tagged tests is skipped, and none of its listed tasks is still open.
- **Tasks:** ⬜ not started · ✅ done (with commit hash)

### Conventions

- **Commits:** `<type>(Tnn): <summary>`, e.g. `fix(T07): parameterize Dapper queries`. `git log --grep T07` finds a task's commits.
- **Tests:** tag each test with the criteria it proves, e.g. `[Trait("AC", "RULE-5")]`. `dotnet test --filter "AC=RULE-5"` runs the proof for one criterion. The rubric's Tests column shows the filter and the number of passing tests, plus any skipped ones, e.g. `AC=RULE-5` · 1 (+2 skipped).
- **UI tests (Vitest):** each `describe` name starts with the criterion, e.g. `describe('[UI-2] …')`. `ng test --filter "\[UI-2\]"` (in `ui/`) runs them; the Tests column shows `[UI-2]` · n. → D18 → D8
- **Decisions:** an ambiguous rule or design choice gets a `Dn` row before it's implemented. The user decides; the agent implements.

---

## Acceptance criteria (rubric)

The spec's **Rules**, **Requirements** and **Tasks** are the acceptance criteria. Each row quotes the spec verbatim, then states our testable interpretation. *Italics* mark anything we added beyond the spec's wording; where the addition is a judgement call, it links to a `Dn` row. Each row is traced to the tasks that address it and the tests that prove it. The solution is done when every row is ✅.

### Rules

| ID | Spec (verbatim) | Acceptance criterion (our interpretation) | Tasks / decisions | Tests | Status |
|----|-----------------|-------------------------------------------|-------------------|-------|--------|
| RULE-1 | A Person is uniquely identified by their Name. | A person is uniquely identified by name: *creating a person whose name already exists is rejected, and the database enforces uniqueness.* | T08, T11, T18, T21, D4 | `AC=RULE-1` · 10 | ✅ |
| RULE-2 | A Person who has not had an astronaut assignment will not have Astronaut records. | A person with no astronaut assignment has no `AstronautDetail` and no `AstronautDuty` rows; *the API returns null astronaut fields for them.* | T10, T23 | `AC=RULE-2` · 4 | ✅ |
| RULE-3 | A Person will only ever hold one current Astronaut Duty Title, Start Date, and Rank at a time. | A person has at most one current duty (title, start date, rank), *and `AstronautDetail` always reflects it (spec Definitions #2).* | T10, T14, T20 | `AC=RULE-3` · 4 | ✅ |
| RULE-4 | A Person's Current Duty will not have a Duty End Date. | The current duty has no `DutyEndDate`; *every other duty has one (inferred from RULE-5).* | T10, T14, T20 | `AC=RULE-4` · 5 | ✅ |
| RULE-5 | A Person's Previous Duty End Date is set to the day before the New Astronaut Duty Start Date when a new Astronaut Duty is received for a Person. | When a new duty is received, the previous duty's `DutyEndDate` = new `DutyStartDate` − 1 day. *"Previous" means the current duty; a new duty must start strictly after it, otherwise it's rejected with 400 (D1).* | T10, T14, T20, D1 | `AC=RULE-5` · 4 | ✅ |
| RULE-6 | A Person is classified as 'Retired' when a Duty Title is 'RETIRED'. | A person is classified as retired when *their current* duty title is `RETIRED`. *A retired person can return to service; a new duty un-retires them (D2). The title is compared ignoring case (D4).* | T10, T17, T18, D2, D4 | `AC=RULE-6` · 5 | ✅ |
| RULE-7 | A Person's Career End Date is one day before the Retired Duty Start Date. | Career end date = retired duty start date − 1 day. *It's cleared when the person returns to service, and set again by any later retirement. RETIRED can't be a person's first duty, so a career never ends before it starts (D2).* | T10, T15, T17, D2 | `AC=RULE-7` · 5 | ✅ |

### API requirements

| ID | Spec (verbatim) | Acceptance criterion (our interpretation) | Tasks / decisions | Tests | Status |
|----|-----------------|-------------------------------------------|-------------------|-------|--------|
| API-1 | Retrieve a person by name. | Retrieve a person by name, *safe for any name; an unknown name returns 404 (D3).* | T07, T11, T16, T21, D3 | `AC=API-1` · 9 | ✅ |
| API-2 | Retrieve all people. | Retrieve all people. | T11 | `AC=API-2` · 2 | ✅ |
| API-3 | Add/update a person by name. | Add or update a person by name. | T08, T11, T25 | `AC=API-3` · 19 | ✅ |
| API-4 | Retrieve Astronaut Duty by name. | Retrieve *all of* a person's astronaut duties by name. *An unknown name returns 404; a known person with no astronaut records returns 200 with null astronaut fields and an empty duties list (D3).* | T06, T07, T11, T16, T21, T38 | `AC=API-4` · 10 | ✅ |
| API-5 | Add an Astronaut Duty. | Add an astronaut duty, *applying RULE-3 to RULE-7.* | T07, T10, T13–T15, T19–T21 | `AC=API-5` · 16 | ✅ |

### UI requirements

| ID | Spec (verbatim) | Acceptance criterion (our interpretation) | Tasks / decisions | Tests | Status |
|----|-----------------|-------------------------------------------|-------------------|-------|--------|
| UI-1 | Successfully implement a web application that demonstrates production level quality. Angular is preferred. | A production-quality web application, built in Angular (the preferred option). | T30–T35, T40, T41 | `[UI-1]` · 6 | 🟡 scaffold, app shell and API layer done; the screens are still to come (T31–T35, T40, T41) |
| UI-2 | Implement call(s) to retrieve an individual's astronaut duties. | The UI calls the API to retrieve an individual's astronaut duties. | T26, T30, T32 | `[UI-2]` · 2 | 🟡 the API call is in place; the person detail screen is still to come (T32) |
| UI-3 | Display the progress of the process and the results in a visually sophisticated and appealing manner. | The UI shows the progress of each request *(loading, empty and error states)* and its results in a visually sophisticated and appealing way. | T30, T32, T34, T41 | `[UI-3]` · 3 | 🟡 API error messages are surfaced; progress states and the activity feed are still to come (T34, T41) |

### Spec tasks

| ID | Spec (verbatim) | Acceptance criterion (our interpretation) | Tasks / decisions | Tests | Status |
|----|-----------------|-------------------------------------------|-------------------|-------|--------|
| SPEC-1 | Generate the database — This is your source and storage location | The SQLite database is generated from the EF migrations, *automatically at startup in Development; the file itself isn't committed (D7).* | T02–T05, T39, D7 | — | 🟡 works (verified manually), untested |
| SPEC-2 | Enforce the rules | Every `RULE-n` row is ✅. | see RULE-1…7 | — | ✅ |
| SPEC-3 | Improve defensive coding | *Untrusted input is validated or parameterized; failures return a consistent error contract (400/404/500) without leaking internals; the build has no nullable warnings.* | T07, T19, T21–T23 | `AC=SPEC-3` · 11 | ✅ |
| SPEC-4 | Add unit tests — identify the most impactful methods requiring tests; reach >50% code coverage | The business logic (duty handler, pre-processors, queries) is tested first; line coverage is >50%, measured with coverlet; *each test is tagged with the criteria it proves (D8).* | T09–T12, D8 | coverage 63.1% line (T12); all 34 tests tagged | ✅ |
| SPEC-5 | Implement process logging — Log exceptions; Log successes; Store the logs in the database | Every request's success or exception is written to a database table; *a failed request's log entry survives the rollback of its transaction.* | T27–T29 | `AC=SPEC-5` · 10 | ✅ |

---

## Tasks

### Phase 0 — Setup

| ID | Task | Status |
|----|------|--------|
| T01 | Add `.gitignore` at the repo root (dotnet, node, coverage, SQLite, OS). Untrack the committed `bin/`, `obj/` and `tech_exercise_v.0.0.4.tgz`. Keep the shared `.idea/` files; Rider's own `.idea/.gitignore` already excludes `workspace.xml`. | ✅ `7819070` |
| T02 | Apply EF migrations at startup, in Development only (`Database.Migrate()`), so a fresh clone needs only `dotnet run`. *(Spec task 1)* → D7 | ✅ `aa69c89` |
| T03 | Dev-only seeder: runs after the migration, only when the database is empty, using fixed dates. The data covers a non-astronaut, an active astronaut with history, a retiree and (added with T17) an astronaut who returned to service. Remove the commented-out `SeedData()` (`StargateContext.cs:22`). → D5 | ✅ `1ba7350` |
| T04 | Add a local tool manifest at the repo root (`.config/dotnet-tools.json`) pinning `dotnet-ef` 8.x; contributors run `dotnet tool restore`. → D9 | ✅ `0758c3e` |
| T05 | Make `AstronautDetail.CareerStartDate` NOT NULL in the database, via a new migration (don't edit `InitialCreate`). → D10 | ✅ `ab5f7e9` |

### Phase 1 — Quick, high-impact bug fixes

| ID | Task | Status |
|----|------|--------|
| T06 | `GET /AstronautDuty/{name}` sends `GetPersonByName`; it should send `GetAstronautDutiesByName` (`Controllers/AstronautDutyController.cs:24`). | ✅ `7f7a960` |
| T07 | **Security:** remove the SQL built by string interpolation. Parameterize the Dapper queries in the query handlers (`GetPersonByName.cs:26`, `GetAstronautDutiesByName.cs:28,34`), and move `CreateAstronautDutyHandler`'s Dapper reads (`CreateAstronautDuty.cs:56,60,90`) to EF, so commands use only EF. → D6. Repro: `GET /Person/' OR 1=1 --` returns a person; `GET /Person/O'Brien` returns a 500. | ✅ `9d16290` |
| T08 | Reject duplicate names: register `CreatePersonPreProcessor` (missing from `Program.cs:18`), and make `Person.Name` `COLLATE NOCASE` with a unique index (one migration). → D4 | ✅ `bb13ea6` |

### Phase 2 — Tests (written before the rule fixes)

| ID | Task | Status |
|----|------|--------|
| T09 | Create an xUnit test project with in-memory SQLite (a real provider, so the Dapper SQL runs) and coverlet for coverage. Run from `tech_exercise/package/exercise1/`: `dotnet test Stargate.sln`; one criterion: `--filter "AC=RULE-1"`; coverage: `--collect:"XPlat Code Coverage"`, then `dotnet reportgenerator -reports:"tests/StargateAPI.Tests/TestResults/*/coverage.cobertura.xml" -targetdir:coverage -reporttypes:Html`. → D8, D11 | ✅ `19aac51` |
| T10 | Tests for `CreateAstronautDutyHandler`: first duty, a subsequent duty, RETIRED as the first duty, RETIRED after other duties, backdated duty, unknown person. Tests for behaviour not built yet are skipped with `Skip = "Pending Tnn …"`; the fixing task removes the skip first. → D12 | ✅ `868d15c` |
| T11 | Tests for the pre-processors (person and duty) and for the queries (found and not found). | ✅ `5206e2a` |
| T12 | Reach >50% coverage. Record the number here: **63.1% line, 75% branch** (2026-10-25, after T11; generated migrations excluded). Every handler and pre-processor is at 100%; the controllers are at 0% until the HTTP-level tests (T16, T21). Re-measure at wrap-up (T36). | ✅ `fe00dd6` |

### Phase 3 — Enforce the rules

| ID | Task | Status |
|----|------|--------|
| T13 | Scope the duplicate-duty check to the person; today it matches title + start date across everyone (`CreateAstronautDuty.cs:37`). *Superseded by T14: the check was removed, since T14's rule also rejects duplicates.* | ✅ `aeb7377` |
| T14 | Reject backdated and same-day duties: in `CreateAstronautDutyPreProcessor`, a new duty's start date must be strictly after the person's current duty's start date, otherwise 400. Today the "previous" duty is simply the newest one and the new start date is never validated (`CreateAstronautDuty.cs:90-98`). → D1 | ✅ `8c325a8` |
| T15 | In `CreateAstronautDutyPreProcessor`, reject with a 400: RETIRED as a person's first duty, and RETIRED for a person who is already retired. Today the code accepts it and sets `CareerEndDate` to the start date, breaking RULE-7 (`CreateAstronautDuty.cs:73`). → D2 | ✅ `703fe5a` |
| T16 | Not-found handling: both by-name query handlers return `Success = false`, `ResponseCode = 404` for an unknown name, through the existing `BaseResponse` wrapper. Today `GetPersonByName` returns 200 with `person: null`, and `GetAstronautDutiesByName` will throw a NullReferenceException once T06 routes to it (`GetAstronautDutiesByName.cs:34`). A known person with no astronaut records returns 200 with an empty duties list. → D3 | ✅ `0278afb` |
| T17 | Return to service: when a non-RETIRED duty arrives for a retired person, end the RETIRED duty as usual (RULE-5), make the new duty current, and clear `CareerEndDate`. `CareerStartDate` is unchanged. A later RETIRED duty sets `CareerEndDate` again. Today `CareerEndDate` stays set alongside an active title. → D2 | ✅ `17d86f1` |
| T18 | Trim incoming names and duty titles. Compare the `RETIRED` title ignoring case (store it as received), by changing `DutyTitles.IsRetired`, the single place it's decided. Test that `john` finds `John` and that `Retired` retires. → D4 | ✅ `65d6ac8` |
| T19 | Make `DutyStartDate` required and validate the inputs (empty names or titles; today a missing date silently defaults to `0001-01-01`). → D13 | ✅ `e855470` |
| T20 | Guard against two simultaneous duty requests for the same person: a unique index on `AstronautDuty(PersonId)` filtered to `DutyEndDate IS NULL`, so the database can never hold two current duties for one person. The losing request's save fails and is rolled back completely (a 500 until T21 maps it to 409). → D6, D14 | ✅ `b163d22` |

### Phase 4 — Defensive coding and error contract

| ID | Task | Status |
|----|------|--------|
| T21 | Consistent error handling. `CreateAstronautDuty` has no try/catch and leaks stack traces; the other actions catch everything, return 500 and leak `ex.Message`. Replace with a global exception handler: validation → 400, a database unique-constraint conflict (such as T20's race, or two simultaneous identical person creations) → 409 Conflict, anything unexpected → 500 with a generic message. (Not found is a result, not an exception; see D3.) Done with an `IExceptionHandler` that writes the `BaseResponse` shape for every error, including ASP.NET's automatic 400s; the controllers' catch-all blocks are gone. A duplicate person is a 409 and a duty for an unknown person a 404. → D15 | ✅ `fa0ce37` |
| T22 | Make the pre-processors async (they currently call EF synchronously). | ✅ `2f72d52` |
| T23 | Resolve the nullable warnings: 6 distinct at the start, 2 left after T16 (the `Person` navigation properties on `AstronautDetail` and `AstronautDuty`). Make `PersonAstronaut.CurrentRank` and `CurrentDutyTitle` `string?`, so a non-astronaut gets `null` rather than `""` (RULE-2). The navigation properties use `= null!`, as EF Core's documentation recommends; the build now has no warnings. | ✅ `0c80488` |
| T24 | Add `ProducesResponseType` attributes and XML comments for Swagger. Every endpoint lists its 200, error (400/404/409) and 500 responses with their shapes; the build emits an XML documentation file, with CS1591 ("missing comment") switched off so only the documented members need comments. | ✅ `1c9bec5` |
| T38 | Return duties as an `AstronautDutyDto` (matching how `PersonAstronaut` works for people) rather than the `AstronautDuty` EF entity, which leaks its `Person` link as `"person": null` and ties the API's response shape to the database entity (`GetAstronautDutiesByName.cs:48`). Each duty now returns `id`, `rank`, `dutyTitle`, `dutyStartDate` and `dutyEndDate`; `personId` is dropped as redundant with `person.personId`. | ✅ `525f34e` |

### Phase 5 — Requirements gaps

| ID | Task | Status |
|----|------|--------|
| T25 | Add/**update** a person by name: `PUT /Person/{name}` renames them, with the new name as a JSON string body. Unknown person → 404, blank new name → 400, name taken by someone else → 409; a case-only change is allowed. → D16 | ✅ `1cf0853` |
| T26 | Let the Angular app reach the API: `ng serve`'s proxy forwards `/api/*` to the API and strips the prefix, so the browser sees one origin and the API needs no CORS policy. A separately hosted UI would need one. To run both: `dotnet run --launch-profile http` in `api/` (port 5204), then `npm start` in `ui/` (port 4200). → D18 | ✅ |

### Phase 6 — Process logging (stored in the database)

| ID | Task | Status |
|----|------|--------|
| T27 | Add a `ProcessLog` entity and migration: timestamp (UTC), request type, outcome, level, message, exception (unexpected errors only), duration and payload (the request as JSON), indexed by timestamp. → D17 | ✅ `77584f4` |
| T28 | Add a MediatR `IPipelineBehavior` that logs the success or failure of each request to the database. It's registered before `AddMediatR`, because MediatR adds its own pre-processor step first and would otherwise run validation outside the logging. The unique-conflict check is shared with `ApiExceptionHandler` (`DatabaseErrors`). → D17 | ✅ `d9f2e86` |
| T29 | Make sure failed requests still log after a rollback: `ProcessLogWriter` writes through a fresh context in its own scope, so a failed save's leftover changes can't take the log entry with them. (Exceptions inside MediatR are already logged by T28; errors outside it, such as malformed JSON, are deliberately not logged, per D17.) → D17 | ✅ `3f42aad` |

### Phase 7 — Angular UI

| ID | Task | Status |
|----|------|--------|
| T30 | Scaffold the Angular 22 app in `exercise1/ui` (standalone components, signals, no zone.js, strict mode, SCSS, Vitest) with Angular Material 22; add the app shell, typed API services (`PeopleService`, `AstronautDutyService`, all calls under `/api`) and an interceptor that turns every failure into an `ApiError` carrying the API's own message. → D18 | ✅ `665cf95` |
| T31 | People list: search, astronaut vs. non-astronaut, retired badge. | ⬜ |
| T32 | Person detail: current rank and title, career dates, and the duty history as a timeline (current duty highlighted; RETIRED and return to service visible). | ⬜ |
| T33 | Forms: add a person, add a duty. Validation mirrors the API rules; rejections show the API's message. | ⬜ |
| T34 | Progress and result states: progress bar and loading skeletons, buttons disabled while saving, empty and error states, and snackbars with the API's messages. *(Spec: "display the progress of the process")* → D18 | ⬜ |
| T35 | Frontend tests for the services and key components (Vitest; `describe` names carry the requirement ID). → D18 | ⬜ |
| T40 | Rename a person from the UI, using `PUT /Person/{name}` (T25). → D18 | ⬜ |
| T41 | Activity feed: a read-only `GET /ProcessLog` endpoint (most recent first) and a UI panel showing recent operations with outcome and duration. → D17, D18 | ⬜ |

### Phase 8 — Wrap-up

| ID | Task | Status |
|----|------|--------|
| T39 | Test `DevelopmentSeeder`: the seeded data follows the rules (one current duty per astronaut, each previous duty ends the day before the next starts, career end the day before RETIRED), and a second run adds nothing. Tag `AC=SPEC-1`. Expected to lift line coverage to about 91%. | ⬜ |
| T36 | Update the README with run instructions (API, database, UI, tests) and a summary of the changes. | ⬜ |
| T37 | Make the repo public and share the link (do **not** fork the original). | ⬜ |

---

## Decision log

Each entry records a choice, why it was made, and what was rejected. The exercise leaves these open on purpose.

| ID | Question | Decision | Rationale | Tasks |
|----|----------|----------|-----------|-------|
| D1 | A new duty starts before the current duty — reject it, or insert it into the history? | **Reject**, as a validation error (400). A new duty's start date must be **strictly after** the current duty's start date, so a same-day duty is rejected too. | Duties are only added at the end of the timeline. This matches the plain reading of RULE-5 ("previous" = the current duty), keeps the logic simple, and RULE-3 to RULE-5 always hold. Same-day duties would otherwise give the current duty an end date before its start. Trade-off we accept: the external service must send duties in date order, and backfill would be a separate feature. Rejected: *insert into the history and recalculate end dates* (handles out-of-order feeds, but rewrites existing records and ties D1 to D2); *store as history only* (most of that complexity, with an unclear meaning); *treat a same-day duty as a correction* (an update operation the spec doesn't ask for). | T14 |
| D2 | A duty arrives after RETIRED — reject it, or un-retire the person? | **Un-retire.** The RETIRED duty is ended as usual (RULE-5), the new duty becomes current, and `CareerEndDate` is cleared; `CareerStartDate` keeps the original start. "Retired" means the *current* duty is RETIRED, and `CareerEndDate` reflects the most recent retirement. **RETIRED is rejected** (400) as a person's first duty, and for a person who is already retired. | A real HR system has to handle people returning to service (for example, a retiree recalled), so accepting that is worth the extra states (retire → return → retire again). It also settles RULE-6's "a Duty Title" ambiguity: retirement is decided by the current duty. Rejecting RETIRED as a first duty means RULE-7 never produces a career that ends before it starts. Rejecting a second RETIRED keeps `CareerEndDate` stable: you can only retire from active service. Rejected: *retirement is final* (simpler and consistent with D1's strictness, but can't represent a return to service); *accept but stay retired* (today's contradiction: an active title with a career end date); *apply RULE-7 literally to a first-duty RETIRED* (stores a career ending before it begins); *keep today's end date = start date* (breaks RULE-7); *accept a second RETIRED as a correction that moves `CareerEndDate`* (an update operation, which D1 also turned down for same-day duties). | T15, T17 |
| D3 | Unknown name on GET — 404, or 200 with null? | **404, returned through the existing `BaseResponse` wrapper** (`Success = false`, `ResponseCode = 404`, message naming the person); no exception is thrown. A **known person with no astronaut records** returns 200 with null astronaut fields and an empty duties list. | Uses the codebase's own mechanism (`GetResponse()` already maps `ResponseCode` to the HTTP status), keeps "doesn't exist" distinct from "exists, not an astronaut" (RULE-2), and gives the UI a clear not-found state. Sets the error rule for T21: *expected outcomes are results* (not found → 404); *invalid input is a validation error* (400); *anything unexpected is an exception* (500). Process logging (T28) therefore records a not-found as a handled failure, not an exception. Rejected: *200 with null* (`success: true` for something not found; the two empty cases look identical); *throwing a `NotFoundException`* (uses exceptions for an ordinary situation, logs every miss as an exception, and depends on T21). | T16, T21 |
| D4 | Are names and `RETIRED` case-sensitive? | **No, for both.** Names: `Person.Name` is `COLLATE NOCASE` with a unique index, so matching and uniqueness ignore case, while the name is stored as received. `RETIRED`: compared ignoring case, stored as received. Names and titles (and ranks) are trimmed, in the request classes' property setters, so every caller gets clean input (T18). | One principle for both: *match identity and meaning ignoring case, but keep what the source sent.* The data comes from an external service ACTS doesn't control, and RULE-1 makes the name the identity, so `John` and `john` being two people would invite accidental duplicates. Setting the collation on the column means EF and Dapper queries both inherit it with no query changes. Accepted limitation: SQLite `NOCASE` only folds ASCII letters (`É` ≠ `é`); documented rather than fixed. Rejected: *exact match* (accidental duplicates; `Retired` would quietly not retire someone); *a normalized-name column* (handles Unicode, but adds an extra column to keep in sync, which this exercise doesn't need); *rewriting the stored title as `RETIRED`* (ACTS would be changing data it doesn't own). | T08, T18 |
| D5 | Seed data — migration `HasData`, or a dev-only seeder? | **Dev-only seeder**, run at startup after the migration, only when the database is empty, with fixed dates. | Keeps migrations schema-only. `HasData` would need a new migration for every data change, and doesn't suit date-dependent data. | T03 |
| D6 | Keep the Dapper-for-reads / EF-for-writes split, or consolidate? | **Split by side:** queries use Dapper with parameterized SQL; commands (handlers and pre-processors) use only EF. | The code already reads like a lightweight CQRS design, but applied it inconsistently: the duty handler read with Dapper and wrote with EF, calling `Update()` on objects EF never loaded. This option makes the existing pattern consistent rather than removing it. Rejected: *keep as is* (keeps the mixed handler and makes transactions awkward); *EF everywhere* (stronger on safety: injection-proof by design, compiler-checked mappings, one fewer dependency, and this app's reads don't need hand-written SQL; but BAM's email suggests MediatR + Dapper + EF is their own stack, and showing you can make it safe was judged more valuable than replacing it). The cost we accept: Dapper SQL isn't compiler-checked, so query tests (T11) must cover column mapping and names containing apostrophes. | T07, T20 |
| D7 | Commit `starbase.db`, or ignore it? | **Ignore** (`*.db` in `.gitignore`). Migrations are the only source of truth; the dev-only startup migration and seeder (T02, T03) keep a clone runnable. | A committed database would drift from the migrations and churn on every run, especially once process logging (T27–T29) writes rows on each request. Binary diffs can't be reviewed. | T01, T02 |
| D8 | How are tests traced to acceptance criteria? | **xUnit traits** with one key for every criterion type, e.g. `[Trait("AC", "RULE-5")]`, `[Trait("AC", "API-1")]`. | Lets you run the proof for a single criterion (`dotnet test --filter "AC=RULE-5"`) and group reports by criterion. Naming conventions alone can't be filtered as cleanly. | T09–T12 |
| D9 | How is `dotnet-ef` installed? | **Local tool manifest** at the repo root, pinned to 8.x. | Versioned in the repo; anyone gets it with `dotnet tool restore`. A global install only works on one machine. | T04 |
| D10 | `CareerStartDate`: the model says NOT NULL, the database says nullable — which wins? | **NOT NULL**, via a new migration. | Every astronaut has a career start date, so the database should enforce it. Making the property nullable would allow a state that should be impossible. `InitialCreate` isn't edited because it has already been applied. | T05 |
| D11 | How is the test project set up? | **`tests/StargateAPI.Tests`** next to `api/`, plus `Stargate.sln`. Tests send requests **through the real MediatR pipeline** (pre-processors included), registered by the same `AddStargateMediatR()` the API uses, against an **in-memory SQLite database built by the real migrations**, one per test. xUnit's built-in `Assert`. Coverage with coverlet plus **ReportGenerator** (local tool), excluding generated migrations. Names: `Method_Scenario_ExpectedResult`. HTTP-level tests (`WebApplicationFactory`) come later, for status codes (T16, T21). | Testing through the pipeline checks a rule's validation and its handler together, and catches a missing registration (the T08 bug), which calling handlers directly would miss. Sharing one registration method stops tests and API drifting apart. Running the real migrations tests the actual schema (`NOCASE`, unique index, NOT NULL). Rejected: *calling handlers directly* (two tests per rule; misses registration bugs); *only HTTP tests* (slower; more integration than unit testing); *`EnsureCreated()`* (skips the migrations); *FluentAssertions* (v8+ needs a commercial licence); *coverage without a report* (nothing to show for SPEC-4). | T09–T12 |
| D12 | Tests written before their fix fail. Commit them failing, or skip them? | **Skip with a reason naming the fixing task**, e.g. `[Fact(Skip = "Pending T14 (D1): …")]`. The fixing task first removes the skip, sees the test fail, then makes it pass. | Keeps `dotnet test` green, so a new, unexpected failure stands out, and every run lists the remaining work as "Skipped" with reasons. Rejected: *commit them failing* (the suite stays red, so "all tests pass" stops meaning anything during review); *custom "pending" traits and filters* (a new mechanism; tracking is kept minimal). | T10, T14, T15, T17 |
| D13 | How is input validated, and how strict? | **In the command pre-processors**, throwing `BadHttpRequestException` like the existing rules. Name, rank and title must be **non-empty after trimming**; `DutyStartDate` is a C# `required` property (missing JSON → ASP.NET's automatic 400), and the pre-processor also rejects the default `0001-01-01`. **No length limits, character rules or date-range checks.** | Follows the codebase's existing pattern, keeps validation in one place, and is covered by the pipeline tests. Rejected: *validation attributes* (wouldn't cover `POST /Person`'s bare string; skipped by pipeline tests); *FluentValidation* (a new dependency and mechanism for a handful of rules); *a nullable `DutyStartDate`* (a bigger change; missing rank would still use ASP.NET's error format); *length limits* (not in the spec; the database doesn't need them); *character rules* (risk rejecting real names like O'Brien); *rejecting future dates* (the external feed may send scheduled assignments). Missing or malformed JSON returns ASP.NET's ProblemDetails, while pre-processor errors use `BaseResponse`; unifying them is T21's job. | T19, T21 |
| D14 | How are two simultaneous duty requests for the same person kept from creating two current duties? | **The database enforces it:** a unique index on `AstronautDuty(PersonId)` filtered to `DutyEndDate IS NULL`. The losing request's save fails and rolls back completely; T21 maps that conflict to **409 Conflict**. The plain `PersonId` index is kept for duty-history lookups. | Makes the invalid state impossible to store, whatever code path writes the data (this endpoint, the seeder, a future import, a manual fix), and states RULE-3 directly in the schema. Simultaneous first duties are already covered by `AstronautDetail`'s unique `PersonId` index. EF was checked to update the previous duty's end date before inserting the new one, so ordinary requests don't trip the index; the existing duty tests would catch it if that ever changed. Rejected: *a transaction spanning pre-processor and handler* (more moving parts; relies on SQLite locking; protects only this code path); *an optimistic concurrency version on `AstronautDetail`* (an extra column and code; EF-only; less direct); *accepting the risk* (RULE-3 says "only ever"). | T20, T21 |
| D15 | What do error responses look like, and what produces them? | **The existing `BaseResponse` shape for every error** (`success: false`, `message`, `responseCode`), including ASP.NET's automatic 400s for missing or malformed JSON. **One .NET 8 `IExceptionHandler`** replaces the controllers' catch-all blocks. Status codes: validation → 400; duplicate person → **409**; duty for an unknown person → **404** (matching D3); database unique conflict → 409; anything else → 500. Clients see only our own messages; 409/500 conflicts and malformed JSON get generic text, and exception details never leave the server. Validation keeps throwing `BadHttpRequestException`, using its status code for 404/409. | One response shape, matching successes and D3's 404, so the Angular app always reads `success` and `message`. Built-in mechanism, registered once. Rejected: *ProblemDetails for errors* (standard, but D3's 404 would change format and success/error bodies would differ); *custom middleware* (hand-written equivalent); *a MediatR exception step* (can't see errors outside MediatR, like JSON binding); *400 for duplicates and unknown people* (409 and 404 say more precisely what went wrong); *exception details in Development* (more paths, risk of a leak; T27–T29 log them instead); *our own exception types* (decouples the business layer from ASP.NET, but touches every throw; a possible later clean-up). | T21 |
| D16 | What does "update a person by name" (API-3) mean, when a name is all a person has? | **Rename:** `PUT /Person/{name}` with the new name as a JSON string body, like `POST`. The person keeps their id, duties and astronaut details. Unknown person → 404; blank new name → 400; a name belonging to *someone else* → 409; a case-only change, or the same name, is allowed and returns 200. Returns the person's id, like `POST`. The new name is trimmed (D4). | The only reading in which "update" changes anything. Duties link to the id, so history survives a rename. Follows the existing command + pre-processor + handler pattern. Rejected: *"add or leave as is" for the external feed* (with only a name, "update" would change nothing); *both rename and a replay-safe `POST`* (covers both readings, but reverses D15's 409 for duplicates). | T25 |
| D17 | What is process logging (SPEC-5): what's logged, what's recorded, and how does it survive failures? | **Every MediatR request** (commands and queries) is logged by one pipeline step. Each `ProcessLog` row records timestamp (UTC), request type, outcome (Success/Failure), level (Information / Warning for expected failures such as validation, 404 and 409 / Error for unexpected ones), message, exception details (Error only; never sent to clients), duration and **the request payload as JSON**. Returned failures (D3's 404s) count as failures. Entries are written through a **separate, fresh context**, so a failed business save can't take its log entry down with it. If writing the log fails, the request is unaffected and the failure goes to the console logger. Logs are stored only; there's no read endpoint for now (revisited in D18: T41 adds one for the UI's activity feed). | One place covers every business operation, including future ones, with its business meaning rather than just a URL. The payload gives an audit trail ("which duty was added, with what values?"); this domain holds no secrets, but a real HR system would need a personal-data policy before storing payloads. Rejected: *commands only* (reads are processes too, and the spec says to log successes); *HTTP middleware* (sees URLs, not business operations); *writing through the request's own context* (a failed save leaves it holding broken changes); *EF's context factory* (needs fiddly option-lifetime configuration alongside the existing registration); *failing the request when logging fails* (a logging problem would take down the API); *a `GET /ProcessLog` endpoint* (not required; can be revisited for UI-3). Malformed-JSON 400s aren't logged, since no business operation starts; genuine 500s are also logged to the console by `ApiExceptionHandler`. | T27–T29 |
| D18 | How is the Angular app built? | **Angular 22** (the current release) in `exercise1/ui`, with its defaults: standalone components, signals, no zone.js, strict mode, Vitest. **Angular Material 22** for components and theming. In development, **`ng serve`'s proxy** forwards `/api/*` to the API, so no CORS policy is needed. **Screens:** people list, person detail with a duty timeline, add-person and add-duty forms, and rename (T40). **UI-3** is met by per-request progress (progress bar, skeletons, disabled buttons, snackbars with the API's messages), the duty timeline as the result, and an **activity feed** from the process log (T41, which adds a read-only `GET /ProcessLog`). **State:** signals and small typed services, plus one HTTP interceptor; no NgRx. **Tests:** Vitest, with the requirement ID in each `describe` name. | Current defaults avoid a migration later. Material is the fastest route to accessible, consistent, production-quality UI. The proxy keeps the API free of CORS in development and the UI free of environment-specific URLs. The activity feed is a literal reading of "the progress of the process" and shows SPEC-5 working. Rejected: *an older LTS version* (no benefit for a new app); *a development-only CORS policy* (more configuration, and an API URL per environment); *plain CSS/Tailwind* (more work for the same quality); *PrimeNG* (an extra dependency to justify); *NgRx* (overkill for five endpoints); *Playwright end-to-end tests* (possible at wrap-up). | T26, T30–T35, T40, T41 |

## Log

- 2026-10-25 — Plan created from the codebase review. T06–T08 and T13–T15 were reproduced against a scratch database; T16 was partly reproduced (the 200-with-null response).
- 2026-10-25 — Task IDs moved to `Tnn`. Only S1 → T01 and S2 → T02 appear in git history; earlier IDs exist only in this file's history.
