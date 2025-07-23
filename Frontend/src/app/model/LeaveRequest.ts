
export interface LeaveRequest {
  id?: number;
  userId: number;
  reviewerId?: number; 
  leaveRequestTypeId: number; 
  startDate: string;
  endDate: string;
  reason: string;
  status?:  'pending' | 'approved' | 'rejected'; 
  reviewerComment?: string;

  duration?: number;
  selected?: boolean;
}