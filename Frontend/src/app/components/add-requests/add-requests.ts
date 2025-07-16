import { Component } from '@angular/core';
import { NgFor, NgClass, NgIf } from '@angular/common';
import { RequestDetailComponent } from '../../request-detail.component';

interface LeaveRequest {
  id: string;
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
  imports: [NgFor, NgClass, NgIf, RequestDetailComponent],
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
    },
  ];

  selectedRequest: LeaveRequest | null = null;

  openDetails(req: LeaveRequest) {
    this.selectedRequest = req;
  }

  closeDetails() {
    this.selectedRequest = null;
  }

  onApprove(id: string) {
    this.closeDetails();
  }

  onReject(id: string) {
    this.closeDetails();
  }
}
