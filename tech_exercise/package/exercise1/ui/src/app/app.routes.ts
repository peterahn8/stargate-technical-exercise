import { Routes } from '@angular/router';

// Pages are lazy-loaded, so their Material components stay out of the initial bundle.
export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'people' },
  {
    path: 'people',
    title: 'People · Stargate',
    loadComponent: () => import('./features/people/people-list').then((m) => m.PeopleList),
  },
  {
    // The page sets its own title once the person's name is known.
    path: 'people/:name',
    loadComponent: () => import('./features/people/person-detail').then((m) => m.PersonDetail),
  },
  { path: '**', redirectTo: 'people' },
];
