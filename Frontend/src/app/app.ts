import { Component, signal } from '@angular/core';
import { StatsCardsComponent } from './stats-cards.component';
import { AddRequestsComponent } from './add-requests.component';



@Component({
  selector: 'app-root',
  standalone: true,
  imports: [StatsCardsComponent, AddRequestsComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('Frontend');


}
