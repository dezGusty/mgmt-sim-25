import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CustomNavbar } from '../shared/custom-navbar/custom-navbar';
import { OnInit } from '@angular/core';

import { AdminStatsCards } from './admin-stats-cards/admin-stats-cards'; 
import { AdminManagersList } from './admin-managers-list/admin-managers-list'; 
import { AdminDepartmentsList } from './admin-departments-list/admin-departments-list'; 
import { AdminJobTitlesList } from './admin-job-titles-list/admin-job-titles-list'; 
import { AdminUserRelationships } from './admin-user-relationships/admin-user-relationships'; 
import { AdminLeaveTypesList } from './admin-leave-request-types-list/admin-leave-request-types-list';
import { AddForm } from './admin-add-form/admin-add-form';

@Component({
  selector: 'app-admin-main-page',
  imports: [CommonModule,
    CustomNavbar,
    AdminStatsCards,
    AdminManagersList,
    AdminDepartmentsList, 
    AdminJobTitlesList,
    AdminUserRelationships,
    AdminLeaveTypesList, 
    AddForm
  ],
  templateUrl: './main-page.html',
})
export class AdminMainPage implements OnInit {
  activeTab: string = 'managers';
  showAddForm: boolean = false;

  // Stats data
  totalUsers: number = 247;
  activeManagers: number = 23;
  totalDepartments: number = 12;
  totalJobTitles: number = 35;
  pendingRequests: number = 18;

  constructor() { }

  ngOnInit(): void {
    this.loadInitialData();
  }

  setActiveTab(tab: string): void {
    this.activeTab = tab;
  }

  private loadInitialData(): void {
    // Load initial dashboard data
    // This would typically come from services
  }
}
