import { Component, signal } from '@angular/core';
import { StatsCards } from './components/stats-cards/stats-cards';
import { AddRequests } from './components/add-requests/add-requests';

@Component({
  selector: 'app-root',
  imports: [StatsCards, AddRequests],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly title = signal('Frontend');
}
