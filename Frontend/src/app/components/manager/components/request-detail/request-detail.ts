import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule, NgClass, NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ILeaveRequest } from '../../../../models/leave-request';

@Component({
  selector: 'app-request-detail',
  imports: [NgClass, CommonModule, FormsModule],
  templateUrl: './request-detail.html',
  styleUrl: './request-detail.css',
})
export class RequestDetail {
  @Input() request: ILeaveRequest | null = null;
  @Output() approve = new EventEmitter<{ id: string; comment?: string }>();
  @Output() reject = new EventEmitter<{ id: string; comment?: string }>();
  @Output() actionCompleted = new EventEmitter<void>();
  pendingComment: string = '';
}
