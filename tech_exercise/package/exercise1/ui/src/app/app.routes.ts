import { Routes } from '@angular/router';

// Pages are lazy-loaded, so their Material components stay out of the initial bundle.
export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'people' },
  {
    path: 'people',
    title: 'People · Stargate',
    loadComponent: () => import('./features/people/people-list').then((m) => m.PeopleList),
  },
  // Until the person detail page exists (T32), and for any unknown address, show the list.
  { path: '**', redirectTo: 'people' },
];
