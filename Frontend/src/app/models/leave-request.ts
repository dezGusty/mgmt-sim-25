export interface ILeaveRequest {
  id: string;
  employeeName: string;
  status: 'Pending' | 'Approved' | 'Rejected';
  from: string;
  to: string;
  days: number;
  reason: string;
  createdAt: string;
  comment?: string;
}
