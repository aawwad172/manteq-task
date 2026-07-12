import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';
import { permissionGuard } from './core/guards/permission.guard';
import { Permissions } from './core/auth/permissions';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'requests' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    path: 'requests',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/requests/list/requests-list').then((m) => m.RequestsList),
      },
      {
        path: 'new',
        canActivate: [permissionGuard],
        data: { permission: Permissions.Requests.Create },
        loadComponent: () =>
          import('./features/requests/form/request-form').then((m) => m.RequestForm),
      },
      {
        path: ':id',
        loadComponent: () =>
          import('./features/requests/detail/request-detail').then((m) => m.RequestDetail),
      },
      {
        path: ':id/edit',
        canActivate: [permissionGuard],
        data: { permission: Permissions.Requests.Edit },
        loadComponent: () =>
          import('./features/requests/form/request-form').then((m) => m.RequestForm),
      },
    ],
  },
  { path: '**', redirectTo: 'requests' },
];
