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
- **Tasks:** ⬜ not started · ✅ done (with commit hash)

### Conventions

- **Commits:** `<type>(Tnn): <summary>`, e.g. `fix(T07): parameterize Dapper queries`. `git log --grep T07` finds a task's commits.
- **Tests:** tag each test with the criteria it proves, e.g. `[Trait("AC", "RULE-5")]`. `dotnet test --filter "AC=RULE-5"` runs the proof for one criterion. → D8
- **Decisions:** an ambiguous rule or design choice gets a `Dn` row before it's implemented. The user decides; the agent implements.

---

## Acceptance criteria (rubric)

The spec's **Rules**, **Requirements** and **Tasks** are the acceptance criteria. Each row quotes the spec verbatim, then states our testable interpretation. *Italics* mark anything we added beyond the spec's wording; where the addition is a judgement call, it links to a `Dn` row. Each row is traced to the tasks that address it and the tests that prove it. The solution is done when every row is ✅.

### Rules

| ID | Spec (verbatim) | Acceptance criterion (our interpretation) | Tasks / decisions | Tests | Status |
|----|-----------------|-------------------------------------------|-------------------|-------|--------|
| RULE-1 | A Person is uniquely identified by their Name. | A person is uniquely identified by name: *creating a person whose name already exists is rejected, and the database enforces uniqueness.* | T08, T11, T18, D4 | — | ❌ duplicates accepted |
| RULE-2 | A Person who has not had an astronaut assignment will not have Astronaut records. | A person with no astronaut assignment has no `AstronautDetail` and no `AstronautDuty` rows; *the API returns null astronaut fields for them.* | T10 | — | 🟡 observed, untested |
| RULE-3 | A Person will only ever hold one current Astronaut Duty Title, Start Date, and Rank at a time. | A person has at most one current duty (title, start date, rank), *and `AstronautDetail` always reflects it (spec Definitions #2).* | T10, T14, T20 | — | 🟡 a backdated duty becomes "current" |
| RULE-4 | A Person's Current Duty will not have a Duty End Date. | The current duty has no `DutyEndDate`; *every other duty has one (inferred from RULE-5).* | T10, T14 | — | 🟡 breaks with backdating |
| RULE-5 | A Person's Previous Duty End Date is set to the day before the New Astronaut Duty Start Date when a new Astronaut Duty is received for a Person. | When a new duty is received, the previous duty's `DutyEndDate` = new `DutyStartDate` − 1 day. *"Previous" means the current duty; a new duty must start strictly after it, otherwise it's rejected with 400 (D1).* | T10, T14, D1 | — | ❌ backdating yields end < start |
| RULE-6 | A Person is classified as 'Retired' when a Duty Title is 'RETIRED'. | A person is classified as retired when *their current* duty title is `RETIRED`. *A retired person can return to service; a new duty un-retires them (D2). The title is compared ignoring case (D4).* | T10, T17, T18, D2, D4 | — | ❌ case-sensitive; return to service leaves `CareerEndDate` set |
| RULE-7 | A Person's Career End Date is one day before the Retired Duty Start Date. | Career end date = retired duty start date − 1 day. *It's cleared when the person returns to service, and set again by any later retirement. RETIRED can't be a person's first duty, so a career never ends before it starts (D2).* | T10, T15, T17, D2 | — | ❌ wrong when RETIRED is the first duty |

### API requirements

| ID | Spec (verbatim) | Acceptance criterion (our interpretation) | Tasks / decisions | Tests | Status |
|----|-----------------|-------------------------------------------|-------------------|-------|--------|
| API-1 | Retrieve a person by name. | Retrieve a person by name, *safe for any name; an unknown name returns 404 (D3).* | T07, T11, T16, T21, D3 | — | 🟡 SQL injection; `O'Brien` → 500 |
| API-2 | Retrieve all people. | Retrieve all people. | T11 | — | 🟡 works, untested |
| API-3 | Add/update a person by name. | Add or update a person by name. | T08, T11, T25 | — | ❌ no update; duplicates accepted |
| API-4 | Retrieve Astronaut Duty by name. | Retrieve *all of* a person's astronaut duties by name. *An unknown name returns 404; a known person with no astronaut records returns 200 with null astronaut fields and an empty duties list (D3).* | T06, T07, T11, T16, T21 | — | ❌ returns the person, not duties |
| API-5 | Add an Astronaut Duty. | Add an astronaut duty, *applying RULE-3 to RULE-7.* | T07, T10, T13–T15, T19–T21 | — | 🟡 rule violations above |

### UI requirements

| ID | Spec (verbatim) | Acceptance criterion (our interpretation) | Tasks / decisions | Tests | Status |
|----|-----------------|-------------------------------------------|-------------------|-------|--------|
| UI-1 | Successfully implement a web application that demonstrates production level quality. Angular is preferred. | A production-quality web application, built in Angular (the preferred option). | T30–T35 | — | ⬜ |
| UI-2 | Implement call(s) to retrieve an individual's astronaut duties. | The UI calls the API to retrieve an individual's astronaut duties. | T26, T32 | — | ⬜ |
| UI-3 | Display the progress of the process and the results in a visually sophisticated and appealing manner. | The UI shows the progress of each request *(loading, empty and error states)* and its results in a visually sophisticated and appealing way. | T32, T34 | — | ⬜ |

### Spec tasks

| ID | Spec (verbatim) | Acceptance criterion (our interpretation) | Tasks / decisions | Tests | Status |
|----|-----------------|-------------------------------------------|-------------------|-------|--------|
| SPEC-1 | Generate the database — This is your source and storage location | The SQLite database is generated from the EF migrations, *automatically at startup in Development; the file itself isn't committed (D7).* | T02–T05, D7 | — | 🟡 works (verified manually), untested |
| SPEC-2 | Enforce the rules | Every `RULE-n` row is ✅. | see RULE-1…7 | — | ❌ |
| SPEC-3 | Improve defensive coding | *Untrusted input is validated or parameterized; failures return a consistent error contract (400/404/500) without leaking internals; the build has no nullable warnings.* | T07, T19, T21–T23 | — | ❌ |
| SPEC-4 | Add unit tests — identify the most impactful methods requiring tests; reach >50% code coverage | The business logic (duty handler, pre-processors, queries) is tested first; line coverage is >50%, measured with coverlet; *each test is tagged with the criteria it proves (D8).* | T09–T12, D8 | — | ⬜ |
| SPEC-5 | Implement process logging — Log exceptions; Log successes; Store the logs in the database | Every request's success or exception is written to a database table; *a failed request's log entry survives the rollback of its transaction.* | T27–T29 | — | ⬜ |

---

## Tasks

### Phase 0 — Setup

| ID | Task | Status |
|----|------|--------|
| T01 | Add `.gitignore` at the repo root (dotnet, node, coverage, SQLite, OS). Untrack the committed `bin/`, `obj/` and `tech_exercise_v.0.0.4.tgz`. Keep the shared `.idea/` files; Rider's own `.idea/.gitignore` already excludes `workspace.xml`. | ✅ `7819070` |
| T02 | Apply EF migrations at startup, in Development only (`Database.Migrate()`), so a fresh clone needs only `dotnet run`. *(Spec task 1)* → D7 | ✅ `aa69c89` |
| T03 | Dev-only seeder: runs after the migration, only when the database is empty, using fixed dates. The data covers a non-astronaut, an active astronaut with history and a retiree. Remove the commented-out `SeedData()` (`StargateContext.cs:22`). → D5 | ⬜ |
| T04 | Add a local tool manifest at the repo root (`.config/dotnet-tools.json`) pinning `dotnet-ef` 8.x; contributors run `dotnet tool restore`. → D9 | ✅ `0758c3e` |
| T05 | Make `AstronautDetail.CareerStartDate` NOT NULL in the database, via a new migration (don't edit `InitialCreate`). → D10 | ✅ |

### Phase 1 — Quick, high-impact bug fixes

| ID | Task | Status |
|----|------|--------|
| T06 | `GET /AstronautDuty/{name}` sends `GetPersonByName`; it should send `GetAstronautDutiesByName` (`Controllers/AstronautDutyController.cs:24`). | ⬜ |
| T07 | **Security:** remove the SQL built by string interpolation. Parameterize the Dapper queries in the query handlers (`GetPersonByName.cs:26`, `GetAstronautDutiesByName.cs:28,34`), and move `CreateAstronautDutyHandler`'s Dapper reads (`CreateAstronautDuty.cs:56,60,90`) to EF, so commands use only EF. → D6. Repro: `GET /Person/' OR 1=1 --` returns a person; `GET /Person/O'Brien` returns a 500. | ⬜ |
| T08 | Reject duplicate names: register `CreatePersonPreProcessor` (missing from `Program.cs:18`), and make `Person.Name` `COLLATE NOCASE` with a unique index (one migration). → D4 | ⬜ |

### Phase 2 — Tests (written before the rule fixes)

| ID | Task | Status |
|----|------|--------|
| T09 | Create an xUnit test project with in-memory SQLite (a real provider, so the Dapper SQL runs) and coverlet for coverage. → D8 | ⬜ |
| T10 | Tests for `CreateAstronautDutyHandler`: first duty, a subsequent duty, RETIRED as the first duty, RETIRED after other duties, backdated duty, unknown person. | ⬜ |
| T11 | Tests for the pre-processors (person and duty) and for the queries (found and not found). | ⬜ |
| T12 | Reach >50% coverage. Record the number here: `__%`. | ⬜ |

### Phase 3 — Enforce the rules

| ID | Task | Status |
|----|------|--------|
| T13 | Scope the duplicate-duty check to the person; today it matches title + start date across everyone (`CreateAstronautDuty.cs:37`). | ⬜ |
| T14 | Reject backdated and same-day duties: in `CreateAstronautDutyPreProcessor`, a new duty's start date must be strictly after the person's current duty's start date, otherwise 400. Today the "previous" duty is simply the newest one and the new start date is never validated (`CreateAstronautDuty.cs:90-98`). → D1 | ⬜ |
| T15 | In `CreateAstronautDutyPreProcessor`, reject with a 400: RETIRED as a person's first duty, and RETIRED for a person who is already retired. Today the code accepts it and sets `CareerEndDate` to the start date, breaking RULE-7 (`CreateAstronautDuty.cs:73`). → D2 | ⬜ |
| T16 | Not-found handling: both by-name query handlers return `Success = false`, `ResponseCode = 404` for an unknown name, through the existing `BaseResponse` wrapper. Today `GetPersonByName` returns 200 with `person: null`, and `GetAstronautDutiesByName` will throw a NullReferenceException once T06 routes to it (`GetAstronautDutiesByName.cs:34`). A known person with no astronaut records returns 200 with an empty duties list. → D3 | ⬜ |
| T17 | Return to service: when a non-RETIRED duty arrives for a retired person, end the RETIRED duty as usual (RULE-5), make the new duty current, and clear `CareerEndDate`. `CareerStartDate` is unchanged. A later RETIRED duty sets `CareerEndDate` again. Today `CareerEndDate` stays set alongside an active title. → D2 | ⬜ |
| T18 | Trim incoming names and duty titles. Compare the `RETIRED` title ignoring case (store it as received). Test that `john` finds `John` and that `Retired` retires. → D4 | ⬜ |
| T19 | Make `DutyStartDate` required and validate the inputs (empty names or titles; today a missing date silently defaults to `0001-01-01`). | ⬜ |
| T20 | Guard the duty write against two simultaneous requests for the same person: put the "find the previous duty" read and the write in one transaction. (After T07, `SaveChangesAsync` alone is already atomic.) → D6 | ⬜ |

### Phase 4 — Defensive coding and error contract

| ID | Task | Status |
|----|------|--------|
| T21 | Consistent error handling. `CreateAstronautDuty` has no try/catch and leaks stack traces; the other actions catch everything, return 500 and leak `ex.Message`. Replace with a global exception handler: validation → 400, anything unexpected → 500 with a generic message. (Not found is a result, not an exception; see D3.) | ⬜ |
| T22 | Make the pre-processors async (they currently call EF synchronously). | ⬜ |
| T23 | Resolve the 12 nullable warnings (the `Person` navigation properties, the possibly-null `person` dereferences). | ⬜ |
| T24 | Add `ProducesResponseType` attributes and XML comments for Swagger. | ⬜ |

### Phase 5 — Requirements gaps

| ID | Task | Status |
|----|------|--------|
| T25 | Add/**update** a person by name; no update or rename endpoint exists. | ⬜ |
| T26 | Add a CORS policy (or an `ng serve` proxy) for the Angular app. | ⬜ |

### Phase 6 — Process logging (stored in the database)

| ID | Task | Status |
|----|------|--------|
| T27 | Add a `ProcessLog` entity and migration (timestamp, level, request type, message, exception, payload?). | ⬜ |
| T28 | Add a MediatR `IPipelineBehavior` that logs the success or failure of each request to the database. | ⬜ |
| T29 | Make sure exceptions caught by the global handler (T21) are logged too, and that failed requests still log after a rollback (separate context or scope). | ⬜ |

### Phase 7 — Angular UI

| ID | Task | Status |
|----|------|--------|
| T30 | Scaffold the Angular app (standalone components, typed `HttpClient` services, environment-based API URL). | ⬜ |
| T31 | People list: search and filter, astronaut vs. non-astronaut, retired badge. | ⬜ |
| T32 | Person detail: duty history as a timeline (current duty highlighted, career start and end dates). | ⬜ |
| T33 | Forms: add a person, add a duty. Validation mirrors the API rules. | ⬜ |
| T34 | Progress and result states: loading skeletons, empty and error states, toasts. *(Spec: "display the progress of the process")* | ⬜ |
| T35 | Frontend tests for the services and key components. | ⬜ |

### Phase 8 — Wrap-up

| ID | Task | Status |
|----|------|--------|
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
| D4 | Are names and `RETIRED` case-sensitive? | **No, for both.** Names: `Person.Name` is `COLLATE NOCASE` with a unique index, so matching and uniqueness ignore case, while the name is stored as received. `RETIRED`: compared ignoring case, stored as received. Names and titles are trimmed. | One principle for both: *match identity and meaning ignoring case, but keep what the source sent.* The data comes from an external service ACTS doesn't control, and RULE-1 makes the name the identity, so `John` and `john` being two people would invite accidental duplicates. Setting the collation on the column means EF and Dapper queries both inherit it with no query changes. Accepted limitation: SQLite `NOCASE` only folds ASCII letters (`É` ≠ `é`); documented rather than fixed. Rejected: *exact match* (accidental duplicates; `Retired` would quietly not retire someone); *a normalized-name column* (handles Unicode, but adds an extra column to keep in sync, which this exercise doesn't need); *rewriting the stored title as `RETIRED`* (ACTS would be changing data it doesn't own). | T08, T18 |
| D5 | Seed data — migration `HasData`, or a dev-only seeder? | **Dev-only seeder**, run at startup after the migration, only when the database is empty, with fixed dates. | Keeps migrations schema-only. `HasData` would need a new migration for every data change, and doesn't suit date-dependent data. | T03 |
| D6 | Keep the Dapper-for-reads / EF-for-writes split, or consolidate? | **Split by side:** queries use Dapper with parameterized SQL; commands (handlers and pre-processors) use only EF. | The code already reads like a lightweight CQRS design, but applied it inconsistently: the duty handler read with Dapper and wrote with EF, calling `Update()` on objects EF never loaded. This option makes the existing pattern consistent rather than removing it. Rejected: *keep as is* (keeps the mixed handler and makes transactions awkward); *EF everywhere* (stronger on safety: injection-proof by design, compiler-checked mappings, one fewer dependency, and this app's reads don't need hand-written SQL; but BAM's email suggests MediatR + Dapper + EF is their own stack, and showing you can make it safe was judged more valuable than replacing it). The cost we accept: Dapper SQL isn't compiler-checked, so query tests (T11) must cover column mapping and names containing apostrophes. | T07, T20 |
| D7 | Commit `starbase.db`, or ignore it? | **Ignore** (`*.db` in `.gitignore`). Migrations are the only source of truth; the dev-only startup migration and seeder (T02, T03) keep a clone runnable. | A committed database would drift from the migrations and churn on every run, especially once process logging (T27–T29) writes rows on each request. Binary diffs can't be reviewed. | T01, T02 |
| D8 | How are tests traced to acceptance criteria? | **xUnit traits** with one key for every criterion type, e.g. `[Trait("AC", "RULE-5")]`, `[Trait("AC", "API-1")]`. | Lets you run the proof for a single criterion (`dotnet test --filter "AC=RULE-5"`) and group reports by criterion. Naming conventions alone can't be filtered as cleanly. | T09–T12 |
| D9 | How is `dotnet-ef` installed? | **Local tool manifest** at the repo root, pinned to 8.x. | Versioned in the repo; anyone gets it with `dotnet tool restore`. A global install only works on one machine. | T04 |
| D10 | `CareerStartDate`: the model says NOT NULL, the database says nullable — which wins? | **NOT NULL**, via a new migration. | Every astronaut has a career start date, so the database should enforce it. Making the property nullable would allow a state that should be impossible. `InitialCreate` isn't edited because it has already been applied. | T05 |

## Log

- 2026-10-25 — Plan created from the codebase review. T06–T08 and T13–T15 were reproduced against a scratch database; T16 was partly reproduced (the 200-with-null response).
- 2026-10-25 — Task IDs moved to `Tnn`. Only S1 → T01 and S2 → T02 appear in git history; earlier IDs exist only in this file's history.
