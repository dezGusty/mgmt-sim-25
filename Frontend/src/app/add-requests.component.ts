import { Component } from '@angular/core';
import { NgClass } from '@angular/common';

interface LeaveRequest {
  employee: string;
  status: 'Pending' | 'Approved' | 'Rejected';
  from: string;
  to: string;
  days: number;
  reason: string;
  submitted: string;
}
@Component({
  selector: 'app-add-requests',
  standalone: true,
  imports: [NgClass],
  template: `
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
      <div *ngFor="let req of requests" class="hover:shadow-md transition-shadow duration-200 border-l-4"
        [ngClass]="{
          'border-l-blue-500': req.status === 'Pending',
          'border-l-green-500': req.status === 'Approved',
          'border-l-red-500': req.status === 'Rejected'
        }"
        class="bg-white rounded-lg p-6"
      >
        <div class="flex justify-between items-start mb-4">
          <div class="flex items-center gap-2">
            <svg class="w-4 h-4 text-gray-600" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
              <circle cx="12" cy="12" r="10"></circle>
            </svg>
            <h3 class="text-lg font-semibold text-gray-900">{{ req.employee }}</h3>
          </div>
          <span
            class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium border"
            [ngClass]="{
              'bg-yellow-100 text-yellow-800 border-yellow-200': req.status === 'Pending',
              'bg-green-100 text-green-800 border-green-200': req.status === 'Approved',
              'bg-red-100 text-red-800 border-red-200': req.status === 'Rejected'
            }"
          >
            {{ req.status === 'Pending' ? '⏳' : req.status === 'Approved' ? '✅' : '❌' }}
            {{ req.status }}
          </span>
        </div>
        <div class="space-y-3">
          <div class="flex items-center gap-2 text-sm text-gray-600">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
              <circle cx="12" cy="12" r="10"></circle>
            </svg>
            <span>{{ req.from }} - {{ req.to }}</span>
            <span class="ml-2 px-2 py-1 bg-blue-100 text-blue-800 rounded-full text-xs">{{ req.days }} days</span>
          </div>
          <div class="flex items-start gap-2 text-sm text-gray-600">
            <svg class="w-4 h-4 mt-0.5 flex-shrink-0" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
              <circle cx="12" cy="12" r="10"></circle>
            </svg>
            <p>{{ req.reason }}</p>
          </div>
          <div class="flex items-center gap-2 text-xs text-gray-500">
            <svg class="w-3 h-3" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
              <circle cx="12" cy="12" r="10"></circle>
            </svg>
            <span>Submitted {{ req.submitted }}</span>
          </div>
        </div>
        <div class="mt-4 pt-4 border-t">
          <button class="w-full border border-gray-300 rounded py-2 text-sm font-medium hover:bg-gray-50">View Details</button>
        </div>
      </div>
    </div>
  `
})
export class AddRequestsComponent {
  requests: LeaveRequest[] = [
    {
      employee: 'Alice Popescu',
      status: 'Pending',
      from: 'Jun 10, 2024',
      to: 'Jun 14, 2024',
      days: 5,
      reason: 'Vacation',
      submitted: 'Jun 1, 2024'
    },
    {
      employee: 'Bogdan Ionescu',
      status: 'Approved',
      from: 'Jun 20, 2024',
      to: 'Jun 22, 2024',
      days: 3,
      reason: 'Medical leave',
      submitted: 'Jun 15, 2024'
    },
    {
      employee: 'Carmen Vasilescu',
      status: 'Rejected',
      from: 'Jul 01, 2024',
      to: 'Jul 05, 2024',
      days: 5,
      reason: 'Family event',
      submitted: 'Jun 25, 2024'
    },
    {
      employee: 'Dan Georgescu',
      status: 'Approved',
      from: 'May 10, 2024',
      to: 'May 12, 2024',
      days: 3,
      reason: 'Conference',
      submitted: 'May 1, 2024'
    }
  ];
}
