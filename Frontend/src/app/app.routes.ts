import { Routes } from '@angular/router';
import { AdminMainPage } from './components/admin/main-page/main-page';
import { Home } from './components/home/home';
import { Login } from './components/login/login';
import { User } from './components/user/user';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'admin', component: AdminMainPage },
  { path: 'login', component: Login },
  { path: 'user', component: User },
  { path: '**', redirectTo: '/' }
];
