import { Component, signal } from '@angular/core';
import { StatsCards } from './stats-cards/stats-cards';
import { CommonModule } from '@angular/common';
import { AddRequests } from './add-requests/add-requests';
import { AddRequestForm } from './add-request-form/add-request-form';
import { Router } from '@angular/router';
import { CustomNavbar } from '../shared/custom-navbar/custom-navbar';

@Component({
  selector: 'app-manager-main-page',
  imports: [CommonModule, StatsCards, AddRequests, AddRequestForm, CustomNavbar],
  templateUrl: './main-page.html',
  styleUrl: './main-page.css',
})
export class ManagerMainPage {
  showAddRequestForm = false;

  constructor(private router: Router) {}

  goBack() {
    this.router.navigate(['/']);
  }
}