import { Component, signal, ViewChild, ElementRef } from '@angular/core';
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
  @ViewChild('addRequestsRef') addRequestsComponent!: AddRequests;
  showAddRequestForm = false;
  currentFilter: 'All' | 'Pending' | 'Approved' | 'Rejected' = 'All';

  constructor(private router: Router) {}

  stats: IRequestStats = {
    total: 0,
    pending: 0,
    approved: 0,
    rejected: 0,
  };

  onStatsUpdated(newStats: IRequestStats) {
    this.stats = newStats;
  }

  onRequestAdded(createdRequest: any) {
    this.addRequestsComponent.addNewRequestToList(createdRequest);
  }

  goBack() {
    this.router.navigate(['/']);
  }

  setFilter(filter: 'All' | 'Pending' | 'Approved' | 'Rejected') {
    this.currentFilter = filter;
  }
}
