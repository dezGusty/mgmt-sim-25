import { Component } from '@angular/core';

@Component({
  selector: 'app-stats-cards',
  standalone: true,
  template: `
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
      <div class="bg-gradient-to-br from-blue-50 to-blue-100 border border-blue-200 rounded-lg p-4 flex items-center justify-between">
        <div>
          <p class="text-sm text-blue-600 font-medium">Total Requests</p>
          <p class="text-2xl font-bold text-blue-900">12</p>
        </div>
        <svg class="w-8 h-8 text-blue-600" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
          <path d="M3 17l6-6 4 4 8-8"></path>
        </svg>
      </div>
      <div class="bg-gradient-to-br from-yellow-50 to-yellow-100 border border-yellow-200 rounded-lg p-4 flex items-center justify-between">
        <div>
          <p class="text-sm text-yellow-600 font-medium">Pending</p>
          <p class="text-2xl font-bold text-yellow-900">3</p>
        </div>
        <svg class="w-8 h-8 text-yellow-600" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
          <circle cx="12" cy="12" r="10"></circle>
        </svg>
      </div>
      <div class="bg-gradient-to-br from-green-50 to-green-100 border border-green-200 rounded-lg p-4 flex items-center justify-between">
        <div>
          <p class="text-sm text-green-600 font-medium">Approved</p>
          <p class="text-2xl font-bold text-green-900">7</p>
        </div>
        <svg class="w-8 h-8 text-green-600" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
          <circle cx="12" cy="12" r="10"></circle>
        </svg>
      </div>
      <div class="bg-gradient-to-br from-red-50 to-red-100 border border-red-200 rounded-lg p-4 flex items-center justify-between">
        <div>
          <p class="text-sm text-red-600 font-medium">Rejected</p>
          <p class="text-2xl font-bold text-red-900">2</p>
        </div>
        <svg class="w-8 h-8 text-red-600" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
          <circle cx="12" cy="12" r="10"></circle>
        </svg>
      </div>
    </div>
  `
})
export class StatsCardsComponent {}
