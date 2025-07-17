import { Component, signal } from '@angular/core';
import { StatsCards } from '../stats-cards/stats-cards';
import { CommonModule } from '@angular/common';
import { AddRequests } from '../add-requests/add-requests';
import { AddRequestForm } from '../add-request-form/add-request-form';
import { Router } from '@angular/router';

@Component({
  selector: 'app-admin-main-page',
  imports: [CommonModule, StatsCards, AddRequests, AddRequestForm],
  templateUrl: './main-page.html',
  styleUrl: './main-page.css',
})
export class AdminMainPage {
  showAddRequestForm = false;

  constructor(private router: Router) {}

  goBack() {
    this.router.navigate(['/']);
  }
}