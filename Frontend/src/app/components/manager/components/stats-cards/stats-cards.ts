import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-stats-cards',
  templateUrl: './stats-cards.html',
  styleUrls: ['./stats-cards.css'],
})
export class StatsCards {
  @Input() stats!: {
    total: number;
    pending: number;
    approved: number;
    rejected: number;
  };
}
