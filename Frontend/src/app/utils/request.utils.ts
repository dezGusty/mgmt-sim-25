import { ILeaveRequest } from '../models/leave-request';

export class RequestUtils {
  static filterRequests(
    requests: ILeaveRequest[],
    filter: 'All' | 'Pending' | 'Approved' | 'Rejected'
  ): ILeaveRequest[] {
    if (filter === 'All') {
      return requests;
    }
    return requests.filter((request) => request.status === filter);
  }

  static filterRequestsForCalendar(
    requests: ILeaveRequest[],
    filters: { pending: boolean; approved: boolean; rejected: boolean }
  ): ILeaveRequest[] {
    return requests.filter((request) => {
      if (request.status === 'Pending' && filters.pending) return true;
      if (request.status === 'Approved' && filters.approved) return true;
      if (request.status === 'Rejected' && filters.rejected) return true;
      return false;
    });
  }
}
