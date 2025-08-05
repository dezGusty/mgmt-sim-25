export interface CreateLeaveRequestResponse {
  id: number;
  createdAt: string;
  fullName: string;
  leaveRequestTypeName: string;
  leaveRequestTypeIsPaid: boolean;
  startDate: string;
  endDate: string;
  reason: string;
  requestStatus: number;
  reviewerComment: string;
  departmentName: string;
}
