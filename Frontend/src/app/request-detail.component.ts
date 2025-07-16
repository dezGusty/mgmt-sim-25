import { Component, Input, Output, EventEmitter } from '@angular/core';
import { NgFor, NgClass, NgIf } from '@angular/common';
interface LeaveRequest {
  id: string;
  employee: string;
  status: 'Pending' | 'Approved' | 'Rejected';
  from: string;
  to: string;
  days: number;
  reason: string;
  submitted: string;
  rejectionComment?: string;
}

@Component({
  selector: 'app-request-detail',
  standalone: true,
  imports: [NgClass, NgIf],
  template: `
    <div
      *ngIf="request"
      class="max-w-2xl mx-auto bg-white rounded-lg shadow p-6"
    >
      <h2 class="text-xl font-bold mb-4 flex items-center gap-2">
        <svg
          class="w-5 h-5 text-blue-600"
          fill="none"
          stroke="currentColor"
          stroke-width="2"
          viewBox="0 0 24 24"
        >
          <path d="M12 6v6l4 2"></path>
          <circle cx="12" cy="12" r="10"></circle>
        </svg>
        Leave Request Details
      </h2>

      <div class="space-y-6">
        <div class="flex justify-between items-center">
          <div class="flex items-center gap-2">
            <svg
              class="w-5 h-5 text-gray-600"
              fill="none"
              stroke="currentColor"
              stroke-width="2"
              viewBox="0 0 24 24"
            >
              <circle cx="12" cy="12" r="10"></circle>
            </svg>
            <span class="text-lg font-semibold text-gray-900">{{
              request.employee
            }}</span>
          </div>
          <span
            class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium border"
            [ngClass]="{
              'bg-yellow-100 text-yellow-800 border-yellow-200':
                request.status === 'Pending',
              'bg-green-100 text-green-800 border-green-200':
                request.status === 'Approved',
              'bg-red-100 text-red-800 border-red-200':
                request.status === 'Rejected'
            }"
          >
            {{
              request.status === 'Pending'
                ? '⏳'
                : request.status === 'Approved'
                ? '✅'
                : '❌'
            }}
            {{ request.status }}
          </span>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label
              class="text-sm font-medium text-gray-700 flex items-center gap-1 mb-2"
            >
              <svg
                class="w-4 h-4"
                fill="none"
                stroke="currentColor"
                stroke-width="2"
                viewBox="0 0 24 24"
              >
                <circle cx="12" cy="12" r="10"></circle>
              </svg>
              Start Date
            </label>
            <p class="text-gray-900 bg-gray-50 px-3 py-2 rounded-md">
              {{ request.from }}
            </p>
          </div>
          <div>
            <label
              class="text-sm font-medium text-gray-700 flex items-center gap-1 mb-2"
            >
              <svg
                class="w-4 h-4"
                fill="none"
                stroke="currentColor"
                stroke-width="2"
                viewBox="0 0 24 24"
              >
                <circle cx="12" cy="12" r="10"></circle>
              </svg>
              End Date
            </label>
            <p class="text-gray-900 bg-gray-50 px-3 py-2 rounded-md">
              {{ request.to }}
            </p>
          </div>
          <div>
            <label class="text-sm font-medium text-gray-700 mb-2 block"
              >Duration</label
            >
            <p
              class="text-gray-900 bg-blue-50 px-3 py-2 rounded-md font-medium"
            >
              {{ request.days }} days
            </p>
          </div>
          <div>
            <label
              class="text-sm font-medium text-gray-700 flex items-center gap-1 mb-2"
            >
              <svg
                class="w-4 h-4"
                fill="none"
                stroke="currentColor"
                stroke-width="2"
                viewBox="0 0 24 24"
              >
                <circle cx="12" cy="12" r="10"></circle>
              </svg>
              Submitted On
            </label>
            <p class="text-gray-900 bg-gray-50 px-3 py-2 rounded-md">
              {{ request.submitted }}
            </p>
          </div>
        </div>

        <div>
          <label
            class="text-sm font-medium text-gray-700 flex items-center gap-1 mb-2"
          >
            <svg
              class="w-4 h-4"
              fill="none"
              stroke="currentColor"
              stroke-width="2"
              viewBox="0 0 24 24"
            >
              <circle cx="12" cy="12" r="10"></circle>
            </svg>
            Reason for Leave
          </label>
          <div class="bg-gray-50 px-4 py-3 rounded-md">
            <p class="text-gray-900 whitespace-pre-wrap">
              {{ request.reason }}
            </p>
          </div>
        </div>

        <div *ngIf="request.rejectionComment">
          <label class="text-sm font-medium text-red-700 mb-2 block"
            >Rejection Reason</label
          >
          <div class="bg-red-50 border border-red-200 px-4 py-3 rounded-md">
            <p class="text-red-800">{{ request.rejectionComment }}</p>
          </div>
        </div>

        <div
          class="flex flex-col sm:flex-row gap-3 pt-4 border-t"
          *ngIf="request.status === 'Pending'"
        >
          <button
            class="flex-1 bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded"
            (click)="approve.emit(request.id)"
          >
            Approve Request
          </button>
          <button
            class="flex-1 bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded"
            (click)="reject.emit(request.id)"
          >
            Reject Request
          </button>
        </div>
      </div>
    </div>
  `,
})
export class RequestDetailComponent {
  @Input() request: LeaveRequest | null = null;
  @Output() approve = new EventEmitter<string>();
  @Output() reject = new EventEmitter<string>();
}
