import { Component } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { RequestDetail } from '../request-detail/request-detail';

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
  selector: 'app-add-requests',
  standalone: true,
  imports: [CommonModule, NgClass, RequestDetail],
  templateUrl: './add-requests.html',
  styleUrls: ['./add-requests.css'],
})
export class AddRequests {
  requests: LeaveRequest[] = [
    {
      id: '1',
      employee: 'Alice Popescu',
      status: 'Pending',
      from: 'Jun 10, 2024',
      to: 'Jun 14, 2024',
      days: 5,
      reason: 'Vacation',
      submitted: 'Jun 1, 2024',
      comment: '',
    },
    {
      id: '2',
      employee: 'Bogdan Ionescu',
      status: 'Approved',
      from: 'Jun 20, 2024',
      to: 'Jun 22, 2024',
      days: 3,
      reason: 'Medical leave',
      submitted: 'Jun 15, 2024',
      comment: 'Get well soon!',
    },
    {
      id: '3',
      employee: 'Carmen Vasilescu',
      status: 'Rejected',
      from: 'Jul 01, 2024',
      to: 'Jul 05, 2024',
      days: 5,
      reason: 'Family event',
      submitted: 'Jun 25, 2024',
      comment: 'Not enough leave days.',
    },
    {
      id: '4',
      employee: 'Dan Georgescu',
      status: 'Approved',
      from: 'May 10, 2024',
      to: 'May 12, 2024',
      days: 3,
      reason: 'Conference',
      submitted: 'May 1, 2024',
      comment: 'Enjoy the conference!',
    },
  ];

  selectedRequest: LeaveRequest | null = null;

  openDetails(req: LeaveRequest) {
    this.selectedRequest = req;
  }

  closeDetails() {
    this.selectedRequest = null;
  }

  onApprove(data: { id: string; comment?: string }) {
    const req = this.requests.find((r) => r.id === data.id);
    if (req) {
      req.status = 'Approved';
      req.comment = data.comment;
    }
    this.closeDetails();
  }

  onReject(data: { id: string; comment?: string }) {
    const req = this.requests.find((r) => r.id === data.id);
    if (req) {
      req.status = 'Rejected';
      req.comment = data.comment;
    }
    this.closeDetails();
  }
}
