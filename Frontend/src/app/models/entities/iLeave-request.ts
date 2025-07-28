import { RequestStatus } from '../enums/RequestStatus'

export interface LeaveRequest {
  id: number;
  userId: number;
  reviewerId?: number;
  leaveRequestTypeId: number;
  startDate: Date;
  endDate: Date;
  reason?: string;
  isApproved?: boolean;
  requestStatus: RequestStatus;
  reviewerComment?: string;

  duration?: number;
  selected?: boolean;
}