import { Routes } from '@angular/router';
import { ManagerMainPage } from './components/manager/main-page';
import { Home } from './components/home/home';
import { Login } from './components/login/login';
import { User } from './components/user/user';
import { ResetPassword } from './components/reset-password/reset-password';
import { AdminMainPage } from './components/admin/main-page';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    component: Home,
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
    path: 'login',
    component: Login,
  },
  { path: 'reset-password', component: ResetPassword },
  { path: '**', redirectTo: '/' },
];
