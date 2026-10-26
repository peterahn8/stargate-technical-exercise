# Stargate UI

The Angular 22 front end for the Stargate API. For running the whole app, see the [root README](../../../../README.md).

## Commands

| What | Command |
|------|---------|
| Run (needs the API on port 5204) | `npm start`, then open http://localhost:4200 |
| Production build | `npx ng build` |
| Tests | `npx ng test --watch=false` |
| Tests for one criterion | `npx ng test --watch=false --filter "\[UI-2\]"` |
| Coverage | `npx ng test --watch=false --coverage` (report in `coverage/`) |

## Structure

```
src/app/
  core/
    api/                  typed services for each API area, the models, and the error interceptor
    astronaut-status.ts   Active / Retired / Not an astronaut, and the one RETIRED check
    duration.ts           "2 years, 5 months"
    validators.ts         form rules that mirror the API's, with its wording
  features/
    people/               the people list, person page, duty timeline and dialogs
    activity/             the recent-activity feed
```

- **Talking to the API:** every call goes to `/api/...`. `src/proxy.conf.json` forwards it to the API and removes the prefix. `apiErrorInterceptor` turns every failure into an `ApiError` carrying the API's own message, so pages can show it as-is.
- **State:** signals, with `rxResource` for loading data. Each page is lazy-loaded.
- **Design:** Angular Material with its Material 3 theme. The pages also work at phone width.

## Test conventions

- Vitest, through Angular's test runner. Each top-level `describe` starts with the rubric criterion it proves, e.g. `describe('[UI-2] PersonDetail', …)`, so `--filter` can select one criterion.
- HTTP calls go to Angular's fake backend (`HttpTestingController`). An unanswered request counts as pending work, so a test must answer it before waiting for the app to settle (`whenStable()`). Start loads with `TestBed.tick()`.
- Material components are driven through their test harnesses where possible. The dialog tests open each dialog for real, with Material's animations switched off. The page tests replace the dialog service and the activity feed's data with stand-ins, so they test only the page.
