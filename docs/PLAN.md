# Stargate (ACTS) — Work Plan

Living plan for the BAM technical exercise. Spec: [exercise1/README.md](../tech_exercise/package/exercise1/README.md).

**How this file works**

- Each work item has a stable ID. Reference it in commits: `fix(B2): parameterize Dapper queries`.
  `git log --grep B2` traces an item to its changes.
- Check the box when the item is done and note the commit(s).
- Any ambiguous rule gets an entry in the [Decision log](#decision-log) before it's implemented.

Paths below are relative to `tech_exercise/package/exercise1/api/`.

---

## Acceptance criteria (rubric)

The spec's **Rules** and **Requirements** are treated as acceptance criteria. Each row is worded as a testable statement and is traced to the plan items that implement it and the tests that prove it. The solution is done when every row is ✅ with at least one passing test.

Status: ✅ met and tested · 🟡 works on the happy path / partially met · ❌ fails or missing · ⬜ not started

### Rules

| # | ID | Acceptance criterion | Plan items | Tests | Status |
|---|----|----------------------|------------|-------|--------|
| 1 | RULE-1 | A person is uniquely identified by name: creating a person whose name already exists is rejected, and the database enforces uniqueness. | B3, D4 | — | ❌ duplicates accepted |
| 2 | RULE-2 | A person with no astronaut assignment has no `AstronautDetail` and no `AstronautDuty` rows; the API returns null astronaut fields for them. | — | — | 🟡 observed, untested |
| 3 | RULE-3 | A person has at most one current duty (title, start date, rank), and `AstronautDetail` always reflects it. | B5, B12 | — | 🟡 a backdated duty becomes "current" |
| 4 | RULE-4 | The current duty has no `DutyEndDate`; every other duty has one. | B5 | — | 🟡 breaks with backdating |
| 5 | RULE-5 | When a new duty is received, the previous duty's `DutyEndDate` = new `DutyStartDate` − 1 day. | B5, D1 | — | ❌ backdating yields end < start |
| 6 | RULE-6 | A person is classified as retired when their duty title is `RETIRED`. | B9, B10, D2, D4 | — | 🟡 case-sensitive; post-retirement duty undefined |
| 7 | RULE-7 | Career end date = retired duty start date − 1 day. | B6 | — | ❌ wrong when RETIRED is the first duty |

### API requirements

| # | ID | Acceptance criterion | Plan items | Tests | Status |
|---|----|----------------------|------------|-------|--------|
| 8 | REQ-API-1 | Retrieve a person by name (404 or defined behaviour when not found; safe for any name). | B2, B7, D3 | — | 🟡 SQL injection; `O'Brien` → 500 |
| 9 | REQ-API-2 | Retrieve all people. | — | — | 🟡 works, untested |
| 10 | REQ-API-3 | Add or update a person by name. | B3, R1 | — | ❌ no update; duplicates accepted |
| 11 | REQ-API-4 | Retrieve a person's astronaut duties by name. | B1, B7 | — | ❌ returns the person, not duties |
| 12 | REQ-API-5 | Add an astronaut duty, applying RULE-3 to RULE-7. | B4–B6, B11, B12 | — | 🟡 rule violations above |

### UI requirements

| # | ID | Acceptance criterion | Plan items | Tests | Status |
|---|----|----------------------|------------|-------|--------|
| 13 | REQ-UI-1 | A production-quality web app (Angular). | UI1–UI6 | — | ⬜ |
| 14 | REQ-UI-2 | The UI calls the API to retrieve an individual's astronaut duties. | UI3 | — | ⬜ |
| 15 | REQ-UI-3 | The UI displays the progress of the process and its results in a visually polished way. | UI3, UI5 | — | ⬜ |

---

## Phase 0 — Setup

- [x] **S1** Add `.gitignore` (dotnet, node, coverage, SQLite, OS). Untrack the committed `bin/`, `obj/` and `tech_exercise_v.0.0.4.tgz`. Keep the shared `.idea/` files; Rider's own `.idea/.gitignore` already excludes `workspace.xml`.
- [x] **S2** Generate the database. *(Spec task 1)* → D7
  - Apply migrations at startup, in Development only (`db.Database.Migrate()`), so a fresh clone needs only `dotnet run`. Add a comment explaining why: migrating at startup is avoided in production because multiple instances can race.
  - `dotnet-ef` (`dotnet tool install -g dotnet-ef --version 8.*`) is needed only to *author* new migrations.
- [ ] **S3** Decide on seed data: re-enable it with fixed dates or add a dev-only seeder (`StargateContext.cs:22`, which currently uses `DateTime.Now`). → D5
- [ ] **S4** Fix the model/migration drift: `AstronautDetail.CareerStartDate` is non-nullable in the model but nullable in the migration and snapshot.

## Phase 1 — Quick, high-impact bug fixes

- [ ] **B1** `GET /AstronautDuty/{name}` sends `GetPersonByName`; it should send `GetAstronautDutiesByName` (`Controllers/AstronautDutyController.cs:24`).
- [ ] **B2** SQL injection: the Dapper queries build SQL by string interpolation. Parameterize them (`GetPersonByName.cs:26`, `GetAstronautDutiesByName.cs:28,34`, `CreateAstronautDuty.cs:56,60,90`).
  Repro: `GET /Person/' OR 1=1 --` returns a person; `GET /Person/O'Brien` returns a 500.
- [ ] **B3** Duplicate names are allowed. `CreatePersonPreProcessor` is never registered (`Program.cs:18`), and `Person.Name` has no unique index (needs a migration). *(Rule 1)*

## Phase 2 — Tests (write before the rule fixes)

- [ ] **T1** Create an xUnit test project with in-memory SQLite (a real provider, so the Dapper SQL runs) and coverlet for coverage.
- [ ] **T2** Tests for `CreateAstronautDutyHandler`: first duty, a subsequent duty, RETIRED as the first duty, RETIRED after other duties, backdated duty, unknown person.
- [ ] **T3** Tests for the pre-processors (person and duty) and for the queries (found and not found).
- [ ] **T4** Reach >50% coverage. Record the number here: `__%`.

## Phase 3 — Enforce the rules

- [ ] **B4** The duplicate-duty check ignores the person; it matches title + start date across everyone (`CreateAstronautDuty.cs:37`).
- [ ] **B5** Backdated duties corrupt the history: the "previous" duty is simply the newest one, and the new start date is never validated (`CreateAstronautDuty.cs:90-98`). → D1 *(Rules 3–5)*
- [ ] **B6** RETIRED as a first duty sets `CareerEndDate` to the start date instead of the day before (`CreateAstronautDuty.cs:73`). *(Rule 7)*
- [ ] **B7** Not-found handling: `GetAstronautDutiesByName` throws a NullReferenceException for an unknown name, and `GetPersonByName` returns 200 with `person: null` (`GetAstronautDutiesByName.cs:34`). → D3
- [ ] **B9** Decide what happens to a duty after RETIRED (rejected, or the person un-retires and `CareerEndDate` is cleared). → D2
- [ ] **B10** Case sensitivity of `"RETIRED"` and of names. → D4
- [ ] **B11** Make `DutyStartDate` required and validate the inputs (empty names or titles; today it silently defaults to `0001-01-01`).
- [ ] **B12** Wrap the duty write in an explicit transaction. Keep Dapper reads on the EF connection, or move the write path fully to EF. → D6

## Phase 4 — Defensive coding and error contract

- [ ] **B8** Consistent error handling. `CreateAstronautDuty` has no try/catch and leaks stack traces; the other actions catch everything, return 500 and leak `ex.Message`. Replace this with a global exception handler that maps validation → 400, not found → 404, anything else → 500 with a generic message.
- [ ] **E1** Make the pre-processors async (they currently call EF synchronously).
- [ ] **E2** Resolve the 12 nullable warnings (the `Person` navigation properties, the possibly-null `person` dereferences).
- [ ] **E3** Add `ProducesResponseType` attributes and XML comments for Swagger.

## Phase 5 — Requirements gaps

- [ ] **R1** Add/**update** a person by name. No update or rename endpoint exists. *(Requirement 3)*
- [ ] **R2** Add a CORS policy (or an `ng serve` proxy) for the Angular app.

## Phase 6 — Process logging (stored in the database)

- [ ] **L1** Add a `ProcessLog` entity and migration (timestamp, level, request type, message, exception, payload?).
- [ ] **L2** Add a MediatR `IPipelineBehavior` that logs the success or failure of each request to the database.
- [ ] **L3** Make sure exceptions caught by the global handler (B8) are logged too, and that failed requests still log after a rollback (use a separate context or scope).

## Phase 7 — Angular UI

- [ ] **UI1** Scaffold the Angular app (standalone components, typed `HttpClient` services, environment-based API URL).
- [ ] **UI2** People list: search and filter, astronaut vs. non-astronaut, retired badge.
- [ ] **UI3** Person detail: duty history as a timeline (current duty highlighted, career start and end dates).
- [ ] **UI4** Forms: add a person, add a duty. Validation mirrors the API rules.
- [ ] **UI5** Progress and result states: loading skeletons, empty and error states, toasts. *(Spec: "display the progress of the process")*
- [ ] **UI6** Frontend tests for the services and key components.

## Phase 8 — Wrap-up

- [ ] **W1** Update the README with run instructions (API, database, UI, tests) and a summary of the changes.
- [ ] **W2** Make the repo public and share the link (do **not** fork the original).

---

## Decision log

Each entry records a choice, why it was made, and what was rejected. The exercise leaves these open on purpose.

| ID | Question | Decision | Rationale |
|----|----------|----------|-----------|
| D1 | A new duty starts before the current duty — reject it, or insert it into the history? | _TBD_ | |
| D2 | A duty arrives after RETIRED — reject it, or un-retire the person? | _TBD_ | |
| D3 | Unknown name on GET — 404, or 200 with null? | _TBD_ | |
| D4 | Are names and `RETIRED` case-sensitive? | _TBD_ | |
| D5 | Seed data — migration `HasData`, or a dev-only seeder? | _TBD_ | |
| D6 | Keep the Dapper-for-reads / EF-for-writes split, or consolidate? | _TBD_ | |
| D7 | Commit `starbase.db`, or ignore it? | **Ignore** (`*.db` in `.gitignore`). Migrations are the only source of truth; a dev-only startup migration plus a seeder (S2, S3) keeps a clone runnable. | A committed database would drift from the migrations and churn on every run, especially once process logging (L1–L3) writes rows on each request. Binary diffs can't be reviewed. |

## Log

- 2026-10-25 — Plan created from the codebase review. Bugs B1–B7 were reproduced against a scratch database.
