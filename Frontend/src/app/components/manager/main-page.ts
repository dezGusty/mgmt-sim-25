import { Component, signal } from '@angular/core';
import { StatsCards } from './components/stats-cards/stats-cards';
import { CommonModule } from '@angular/common';
import { AddRequests } from './components/add-requests/add-requests';
import { AddRequestForm } from './components/add-request-form/add-request-form';
import { Router } from '@angular/router';
import { CustomNavbar } from '../shared/custom-navbar/custom-navbar';
import { IRequestStats } from '../../models/request-stats';

@Component({
  selector: 'app-manager-main-page',
  imports: [
    CommonModule,
    StatsCards,
    AddRequests,
    AddRequestForm,
    CustomNavbar,
  ],
  templateUrl: './main-page.html',
  styleUrl: './main-page.css',
})
export class ManagerMainPage {
  showAddRequestForm = false;

  constructor(private router: Router) {
    this.fetchMe();
  }
  async fetchMe() {
    try {
      const response = await fetch('https://localhost:7275/api/Auth/me', {
        credentials: 'include',
      });
      const data = await response.json();
      console.log('Auth/me:', data);
      return data;
    } catch (err) {
      console.error('Auth/me error:', err);
      return null;
    }
  }

  stats: IRequestStats = {
    total: 0,
    pending: 0,
    approved: 0,
    rejected: 0,
  };

  onStatsUpdated(newStats: IRequestStats) {
    this.stats = newStats;
  }

  goBack() {
    this.router.navigate(['/']);
  }
}
