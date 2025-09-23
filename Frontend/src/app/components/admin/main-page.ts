import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { OnInit } from '@angular/core';
import {HttpClient, HttpClientModule} from '@angular/common/http';

import { AdminStatsCards } from './admin-stats-cards/admin-stats-cards'; 
import { AdminUsersList } from './admin-users-list/admin-users-list'; 
import { AdminDepartmentsList } from './admin-departments-list/admin-departments-list'; 
import { AdminJobTitlesList } from './admin-job-titles-list/admin-job-titles-list'; 
import { AdminUserRelationships } from './admin-user-relationships/admin-user-relationship-list/admin-user-relationships'; 
import { AdminLeaveTypesList } from './admin-leave-request-types-list/admin-leave-request-types-list';
import { AdminAuditLogsList } from './admin-audit-logs-list/admin-audit-logs-list';
import { AddForm } from './admin-add-form/form/admin-add-form';
import { CustomNavbar } from '../shared/custom-navbar/custom-navbar';
import { ChatbotComponent } from '../chatbot/chatbot.component';

@Component({
  selector: 'app-admin-main-page',
  imports: [CommonModule,
    HttpClientModule,
    CustomNavbar,
    AdminUsersList,
    AdminDepartmentsList,
    AdminJobTitlesList,
    AdminUserRelationships,
    AdminLeaveTypesList,
    AdminAuditLogsList,
    AddForm,
    ChatbotComponent
  ],
  templateUrl: './main-page.html',
})
export class AdminMainPage implements OnInit {
  activeTab: string = 'users';
  showAddForm: boolean = false;

  constructor() { }

  ngOnInit(): void {
    this.loadInitialData();
  }

  setActiveTab(tab: string): void {
    this.activeTab = tab;
  }

  private loadInitialData(): void {

  }
}
