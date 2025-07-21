import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule, NgClass, NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface LeaveRequest {
  id: string;
  employee: string;
  status: 'Pending' | 'Approved' | 'Rejected';
  from: string;
  to: string;
  days: number;
  reason: string;
  submitted: string;
  comment?: string;
}

@Component({
  selector: 'app-request-detail',
  imports: [NgClass, CommonModule, FormsModule],
  templateUrl: './request-detail.html',
  styleUrl: './request-detail.css',
})
export class RequestDetail {
  @Input() request: LeaveRequest | null = null;
  @Output() approve = new EventEmitter<{ id: string; comment?: string }>();
  @Output() reject = new EventEmitter<{ id: string; comment?: string }>();
  pendingComment: string = '';
}
