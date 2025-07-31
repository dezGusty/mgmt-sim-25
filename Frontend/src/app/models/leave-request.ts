export interface ILeaveRequest {
  id: string;
  employeeName: string;
  status: 'Pending' | 'Approved' | 'Rejected';
  from: string;
  to: string;
  days: number;
  reason: string;
  createdAt: string;
  createdAtDate: Date;
  comment?: string;
  leaveTypeDescription: string;
  leaveType?: {
    id: number;
    title: string;
    description: string;
    maxDays: number;
    isPaid: boolean;
  };
}
