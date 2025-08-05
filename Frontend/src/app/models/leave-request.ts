export interface ILeaveRequest {
  id: string;
  employeeName: string;
  status: 'Pending' | 'Approved' | 'Rejected';
  from: string;
  to: string;
  reason: string;
  createdAt: string;
  createdAtDate: Date;
  comment?: string;
  departmentName: string;
  leaveType?: {
    title: string;
    isPaid: boolean;
  };
}
