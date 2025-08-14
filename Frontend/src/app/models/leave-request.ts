export interface ILeaveRequest {
  id: string;
  employeeName: string;
  status: 'Pending' | 'Approved' | 'Rejected' | 'Invalid' | 'Expired' | 'Canceled' | 'Arrived' | undefined;
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
