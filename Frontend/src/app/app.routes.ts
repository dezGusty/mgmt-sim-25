import { Routes } from '@angular/router';
import { ManagerMainPage } from './components/manager/main-page';
import { Home } from './components/home/home';
import { Login } from './components/login/login';
import { User } from './components/user/user';
import { ResetPassword } from './components/reset-password/reset-password';
import { AdminMainPage } from './components/admin/main-page';
import { AuthGuard } from './guards/auth.guard';
import { LoginGuard } from './guards/login.guard';
import { RoleSelector } from './components/shared/role-selector/role-selector';

export const routes: Routes = [
  {
    path: '',
    component: Login,
    canActivate: [LoginGuard],
  },
  {
    path: 'login',
    component: Login,
    canActivate: [LoginGuard],
  },
  {
    path: 'role-selector',
    component: RoleSelector,
    canActivate: [AuthGuard],
  },
  {
    path: 'manager',
    component: ManagerMainPage,
    canActivate: [AuthGuard],
    data: { roles: ['Manager'] },
  },
  {
    path: 'admin',
    component: AdminMainPage,
    canActivate: [AuthGuard],
    data: { roles: ['Admin'] },
  },
  {
    path: 'user',
    component: User,
    canActivate: [AuthGuard],
    data: { roles: ['Employee'] },
  },
  { 
    path: 'reset-password', 
    component: ResetPassword 
  },
  { 
    path: '**', 
    redirectTo: '/' 
  },
];
