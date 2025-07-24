import { Component } from '@angular/core';
import { OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-stats-cards',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-stats-cards.html',
  styleUrl: './admin-stats-cards.css'
})
export class AdminStatsCards implements OnInit {
  @Input() totalUsers?: number;
  @Input() activeManagers?: number;
  @Input() totalDepartments?: number;
  @Input() totalJobTitles?: number;
  @Input() pendingRequests?: number;

  constructor() { }

  ngOnInit(): void {
    // Initialize stats
  }
}