import { Component, signal } from '@angular/core';
import { StatsCards } from './components/stats-cards/stats-cards';
import { CommonModule } from '@angular/common';

import { AddRequests } from './components/add-requests/add-requests';
import { AddRequestForm } from './components/add-request-form/add-request-form';

@Component({
  selector: 'app-root',
  imports: [CommonModule, StatsCards, AddRequests, AddRequestForm],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly title = signal('Frontend');
  showAddRequestForm = false;
}
