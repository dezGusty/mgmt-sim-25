import { ILeaveRequest } from '../models/leave-request';
import { IRequestStats } from '../models/request-stats';

export class RequestUtils {
  static calculateStats(requests: ILeaveRequest[]): IRequestStats {
    const total = requests.length;
    const pending = requests.filter((r) => r.status === 'Pending').length;
    const approved = requests.filter((r) => r.status === 'Approved').length;
    const rejected = requests.filter((r) => r.status === 'Rejected').length;
    return { total, pending, approved, rejected };
  }

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
