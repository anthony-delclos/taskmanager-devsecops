import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./views/home/home.component').then(m => m.HomeComponent)
  },
  {
    path: 'users',
    loadComponent: () => import('./views/users/user-list/user-list.component').then(m => m.UserListComponent)
  },
  {
    path: 'users/new',
    loadComponent: () => import('./views/users/user-form/user-form.component').then(m => m.UserFormComponent)
  },
  {
    path: 'users/:id/edit',
    loadComponent: () => import('./views/users/user-form/user-form.component').then(m => m.UserFormComponent)
  },
  {
    path: '**',
    redirectTo: ''
  }
];
