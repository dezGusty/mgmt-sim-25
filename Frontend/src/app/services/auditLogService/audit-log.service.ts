import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AuditLog {
  id: number;
  action: string;
  entityType: string;
  entityId?: number;
  entityName: string;
  userId: number;
  userEmail: string;
  userRoles: string;
  isImpersonating: boolean;
  originalUserId?: number;
  originalUserEmail?: string;
  httpMethod: string;
  endpoint: string;
  ipAddress?: string;
  userAgent?: string;
  oldValues?: string;
  newValues?: string;
  additionalData?: string;
  timestamp: string;
  success: boolean;
  errorMessage?: string;
  description?: string;
  user?: {
    id: number;
    email: string;
  };
  originalUser?: {
    id: number;
    email: string;
  };
}

export interface AuditLogResponse {
  message: string;
  data: {
    data: AuditLog[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
  };
  success: boolean;
  timestamp: string;
}

export interface AuditLogSummaryResponse {
  message: string;
  data: {
    totalActions: number;
    periodDays: number;
    startDate: string;
    endDate: string;
    actionBreakdown: Array<{ action: string; count: number }>;
    entityBreakdown: Array<{ entityType: string; count: number }>;
    topUsers: Array<{
      userId: number;
      userEmail: string;
      actionCount: number;
    }>;
    failureRate: number;
    recentFailures: Array<{
      action: string;
      entityType: string;
      userEmail: string;
      errorMessage: string;
      timestamp: string;
    }>;
  };
  success: boolean;
  timestamp: string;
}

export interface AuditLogFiltersResponse {
  message: string;
  data: {
    actions: string[];
    entityTypes: string[];
  };
  success: boolean;
  timestamp: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuditLogService {
  private apiUrl = `${environment.apiUrl}/auditlogs`;

  constructor(private http: HttpClient) {}

  getAuditLogs(
    page: number = 1,
    pageSize: number = 50,
    filters?: {
      userId?: string;
      action?: string;
      entityType?: string;
      startDate?: string;
      endDate?: string;
      search?: string;
    }
  ): Observable<AuditLogResponse> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (filters) {
      if (filters.userId) {
        params = params.set('userId', filters.userId);
      }
      if (filters.action) {
        params = params.set('action', filters.action);
      }
      if (filters.entityType) {
        params = params.set('entityType', filters.entityType);
      }
      if (filters.startDate) {
        params = params.set('startDate', filters.startDate);
      }
      if (filters.endDate) {
        params = params.set('endDate', filters.endDate);
      }
      if (filters.search) {
        params = params.set('search', filters.search);
      }
    }

    return this.http.get<AuditLogResponse>(this.apiUrl, { params });
  }

  getEntityAuditTrail(entityType: string, entityId: number): Observable<{
    message: string;
    data: AuditLog[];
    success: boolean;
    timestamp: string;
  }> {
    return this.http.get<{
      message: string;
      data: AuditLog[];
      success: boolean;
      timestamp: string;
    }>(`${this.apiUrl}/entity/${entityType}/${entityId}`);
  }

  getUserAuditTrail(
    userId: number,
    startDate?: string,
    endDate?: string
  ): Observable<{
    message: string;
    data: AuditLog[];
    success: boolean;
    timestamp: string;
  }> {
    let params = new HttpParams();
    if (startDate) {
      params = params.set('startDate', startDate);
    }
    if (endDate) {
      params = params.set('endDate', endDate);
    }

    return this.http.get<{
      message: string;
      data: AuditLog[];
      success: boolean;
      timestamp: string;
    }>(`${this.apiUrl}/user/${userId}`, { params });
  }

  getAuditSummary(days: number = 7): Observable<AuditLogSummaryResponse> {
    const params = new HttpParams().set('days', days.toString());
    return this.http.get<AuditLogSummaryResponse>(`${this.apiUrl}/summary`, { params });
  }

  getAvailableFilters(): Observable<AuditLogFiltersResponse> {
    return this.http.get<AuditLogFiltersResponse>(`${this.apiUrl}/actions`);
  }

  exportAuditLogs(
    format: 'json' | 'csv' = 'json',
    filters?: {
      userId?: string;
      action?: string;
      entityType?: string;
      startDate?: string;
      endDate?: string;
    }
  ): Observable<Blob> {
    let params = new HttpParams().set('format', format);

    if (filters) {
      if (filters.userId) {
        params = params.set('userId', filters.userId);
      }
      if (filters.action) {
        params = params.set('action', filters.action);
      }
      if (filters.entityType) {
        params = params.set('entityType', filters.entityType);
      }
      if (filters.startDate) {
        params = params.set('startDate', filters.startDate);
      }
      if (filters.endDate) {
        params = params.set('endDate', filters.endDate);
      }
    }

    return this.http.post(`${this.apiUrl}/export`, {}, {
      params,
      responseType: 'blob'
    });
  }

  formatTimestamp(timestamp: string): string {
    const date = new Date(timestamp);
    return date.toLocaleString();
  }

  getActionIcon(action: string): string {
    switch (action.toUpperCase()) {
      case 'LOGIN_SUCCESS':
      case 'LOGIN':
        return '🔑';
      case 'LOGIN_FAILED':
        return '❌';
      case 'LOGOUT':
        return '🚪';
      case 'IMPERSONATION_STARTED':
      case 'IMPERSONATE':
        return '👤';
      case 'IMPERSONATION_STOPPED':
      case 'STOP_IMPERSONATION':
        return '🔄';
      case 'CREATE':
        return '➕';
      case 'UPDATE':
        return '✏️';
      case 'DELETE':
        return '🗑️';
      case 'READ':
        return '👁️';
      default:
        return '📝';
    }
  }

  getActionDisplayName(action: string): string {
    switch (action.toUpperCase()) {
      case 'LOGIN_SUCCESS':
      case 'LOGIN':
        return 'User Login';
      case 'LOGIN_FAILED':
        return 'Failed Login';
      case 'LOGOUT':
        return 'User Logout';
      case 'IMPERSONATION_STARTED':
      case 'IMPERSONATE':
        return 'Admin Impersonation Started';
      case 'IMPERSONATION_STOPPED':
      case 'STOP_IMPERSONATION':
        return 'Admin Impersonation Ended';
      case 'CREATE':
        return 'Created';
      case 'UPDATE':
        return 'Modified';
      case 'DELETE':
        return 'Deleted';
      case 'READ':
        return 'Viewed';
      case 'LOGIN_ATTEMPT':
        return 'Login Attempt';
      default:
        return action.charAt(0).toUpperCase() + action.slice(1).toLowerCase();
    }
  }

  getEntityDisplayName(entityType: string): string {
    switch (entityType.toLowerCase()) {
      case 'user':
        return 'User Account';
      case 'department':
        return 'Department';
      case 'jobtitle':
        return 'Job Title';
      case 'leaverequest':
        return 'Leave Request';
      case 'leaverequesttype':
        return 'Leave Type';
      case 'project':
        return 'Project';
      case 'employeemanager':
        return 'Manager Assignment';
      case 'userproject':
        return 'Project Assignment';
      case 'employeerole':
        return 'User Role';
      case 'publicroliday':
        return 'Public Holiday';
      default:
        return entityType.charAt(0).toUpperCase() + entityType.slice(1);
    }
  }

  getActionDescription(auditLog: AuditLog): string {
    const action = this.getActionDisplayName(auditLog.action);
    const entity = this.getEntityDisplayName(auditLog.entityType);
    const entityName = auditLog.entityName || '';

    // Handle authentication events differently
    if (auditLog.action.toUpperCase().includes('LOGIN') || auditLog.action.toUpperCase().includes('LOGOUT')) {
      return `${action} by ${auditLog.userEmail}`;
    }

    if (auditLog.action.toUpperCase().includes('IMPERSON')) {
      const target = auditLog.originalUserEmail || 'another user';
      return `${action} - Admin ${auditLog.userEmail} ${auditLog.action.includes('STARTED') ? 'began impersonating' : 'stopped impersonating'} ${target}`;
    }

    // Try to extract meaningful information from additional data or endpoint
    const meaningfulDescription = this.extractMeaningfulDescription(auditLog);
    if (meaningfulDescription) {
      return meaningfulDescription;
    }

    // If we have old/new values for updates, show a concise inline field-level summary
    if (auditLog.action.toUpperCase() === 'UPDATE') {
      const changeSummary = this.computeChangeSummary(auditLog.oldValues, auditLog.newValues, auditLog.additionalData);
      if (changeSummary) {
        const target = entityName ? `${entity.toLowerCase()} "${entityName}"` : entity.toLowerCase();
        return `${action} ${target}: ${changeSummary}`;
      }
    }

    // Handle CRUD operations with entity names
    if (entityName && entityName !== 'HttpRequest' && !entityName.startsWith('HttpRequest')) {
      switch (auditLog.action.toUpperCase()) {
        case 'CREATE':
          // For creates, try to include key fields from newValues/additionalData
          const createDetails = this.summarizeNewValues(auditLog.newValues, auditLog.additionalData);
          return `Created new ${entity.toLowerCase()}: "${entityName}"${createDetails ? ' — ' + createDetails : ''}`;
        case 'UPDATE':
          return `Modified ${entity.toLowerCase()}: "${entityName}"`;
        case 'DELETE':
          return `Deleted ${entity.toLowerCase()}: "${entityName}"`;
        default:
          return `${action} ${entity.toLowerCase()}: "${entityName}"`;
      }
    }

    // Handle generic HTTP requests with better descriptions
    return this.describeHttpAction(auditLog);
  }

  private extractMeaningfulDescription(auditLog: AuditLog): string | null {
    try {
      // Try to parse additional data for more context
      if (auditLog.additionalData) {
        const additionalData = JSON.parse(auditLog.additionalData);

        // Handle role changes specifically
        if (auditLog.endpoint?.includes('/roles') || auditLog.endpoint?.includes('/employeeroles')) {
          return this.describeRoleChange(auditLog, additionalData);
        }

        // Handle user management
        if (auditLog.endpoint?.includes('/users') || auditLog.endpoint?.includes('/User')) {
          return this.describeUserAction(auditLog, additionalData);
        }

        // Handle project assignments
        if (auditLog.endpoint?.includes('/projects') || auditLog.endpoint?.includes('/Project')) {
          return this.describeProjectAction(auditLog, additionalData);
        }

        // Handle leave requests
        if (auditLog.endpoint?.includes('/leaverequest') || auditLog.endpoint?.includes('/LeaveRequest')) {
          return this.describeLeaveAction(auditLog, additionalData);
        }
      }
    } catch (e) {
      // If parsing fails, fall back to default behavior
    }

    return null;
  }

  private describeRoleChange(auditLog: AuditLog, additionalData: any): string {
    const action = auditLog.action.toUpperCase();

    if (action === 'UPDATE' || action === 'CREATE') {
      return `Updated user roles and permissions`;
    } else if (action === 'DELETE') {
      return `Removed user role assignment`;
    }

    return `Modified user roles`;
  }

  private describeUserAction(auditLog: AuditLog, additionalData: any): string {
    const action = auditLog.action.toUpperCase();

    // Try to get user information from request body or response
    if (additionalData.RequestBody) {
      try {
        const requestBody = JSON.parse(additionalData.RequestBody);
        if (requestBody.email || requestBody.Email) {
          const userEmail = requestBody.email || requestBody.Email;
          switch (action) {
            case 'CREATE': return `Created new user account for ${userEmail}`;
            case 'UPDATE': {
              const summary = this.computeChangeSummary(auditLog.oldValues, auditLog.newValues, JSON.stringify(additionalData));
              return summary ? `Updated user account for ${userEmail}: ${summary}` : `Updated user account for ${userEmail}`;
            }
            case 'DELETE': return `Deleted user account for ${userEmail}`;
          }
        }
      } catch (e) {}
    }

    switch (action) {
      case 'CREATE': {
        const details = this.summarizeNewValues(auditLog.newValues, JSON.stringify(additionalData));
        return details ? `Created a new user account — ${details}` : `Created a new user account`;
      }
      case 'UPDATE': {
        const summary = this.computeChangeSummary(auditLog.oldValues, auditLog.newValues, JSON.stringify(additionalData));
        return summary ? `Updated user account: ${summary}` : `Updated user account information`;
      }
      case 'DELETE': return `Deleted a user account`;
      default: return `Modified user information`;
    }
  }

  private describeProjectAction(auditLog: AuditLog, additionalData: any): string {
    const action = auditLog.action.toUpperCase();

    switch (action) {
      case 'CREATE': return `Created a new project`;
      case 'UPDATE': return `Updated project details or assignments`;
      case 'DELETE': return `Removed project or project assignment`;
      default: return `Modified project information`;
    }
  }

  private describeLeaveAction(auditLog: AuditLog, additionalData: any): string {
    const action = auditLog.action.toUpperCase();

    switch (action) {
      case 'CREATE': return `Submitted a new leave request`;
      case 'UPDATE': return `Updated leave request status or details`;
      case 'DELETE': return `Cancelled a leave request`;
      default: return `Modified leave request`;
    }
  }

  private describeHttpAction(auditLog: AuditLog): string {
    const endpoint = auditLog.endpoint || '';
    const method = auditLog.httpMethod || '';
    const action = auditLog.action.toUpperCase();

    // Analyze endpoint patterns to provide better descriptions
    if (endpoint.includes('/auth/')) {
      return `Authentication action`;
    }

    if (endpoint.includes('/roles') || endpoint.includes('/employeeroles')) {
      switch (action) {
        case 'CREATE': return `Added new user role`;
        case 'UPDATE': return `Changed user roles or permissions`;
        case 'DELETE': return `Removed user role`;
        default: return `Modified user roles`;
      }
    }

    if (endpoint.includes('/users')) {
      switch (action) {
        case 'CREATE': return `Created user account`;
        case 'UPDATE': return `Updated user information`;
        case 'DELETE': return `Deleted user account`;
        default: return `Modified user data`;
      }
    }

    if (endpoint.includes('/projects')) {
      switch (action) {
        case 'CREATE': return `Created new project`;
        case 'UPDATE': return `Updated project information`;
        case 'DELETE': return `Deleted project`;
        default: return `Modified project data`;
      }
    }

    if (endpoint.includes('/departments')) {
      switch (action) {
        case 'CREATE': return `Created new department`;
        case 'UPDATE': return `Updated department information`;
        case 'DELETE': return `Deleted department`;
        default: return `Modified department data`;
      }
    }

    if (endpoint.includes('/leaverequest')) {
      switch (action) {
        case 'CREATE': return `Submitted leave request`;
        case 'UPDATE': return `Updated leave request`;
        case 'DELETE': return `Cancelled leave request`;
        default: return `Modified leave request`;
      }
    }

    // Generic fallback with better wording
    switch (action) {
      case 'CREATE': return `Created new item in system`;
      case 'UPDATE': return `Updated system information`;
      case 'DELETE': return `Deleted item from system`;
      default: return `Performed ${this.getActionDisplayName(auditLog.action).toLowerCase()} action`;
    }
  }

  getActionColor(action: string): string {
    switch (action.toUpperCase()) {
      case 'LOGIN_SUCCESS':
      case 'LOGIN':
        return 'text-green-600';
      case 'LOGIN_FAILED':
        return 'text-red-600';
      case 'LOGOUT':
        return 'text-gray-600';
      case 'IMPERSONATION_STARTED':
      case 'IMPERSONATE':
        return 'text-purple-600';
      case 'IMPERSONATION_STOPPED':
      case 'STOP_IMPERSONATION':
        return 'text-blue-600';
      case 'CREATE':
        return 'text-green-600';
      case 'UPDATE':
        return 'text-yellow-600';
      case 'DELETE':
        return 'text-red-600';
      case 'READ':
        return 'text-blue-600';
      default:
        return 'text-gray-600';
    }
  }

  // Compute a concise one-line summary of changed fields from old/new JSON
  private computeChangeSummary(oldValues?: string, newValues?: string, additionalData?: string): string | null {
    try {
      if (!oldValues && !newValues && !additionalData) return null;

      const oldObj = oldValues ? JSON.parse(oldValues) : null;
      const newObj = newValues ? JSON.parse(newValues) : null;

      // If both old and new are objects, compute field diffs
      if (oldObj && newObj && typeof oldObj === 'object' && typeof newObj === 'object') {
        const diffs: string[] = [];
        const keys = Array.from(new Set([...Object.keys(oldObj), ...Object.keys(newObj)]));

        for (const key of keys) {
          const oldVal = oldObj[key];
          const newVal = newObj[key];
          if (JSON.stringify(oldVal) !== JSON.stringify(newVal)) {
            const shortOld = this.prettyValue(oldVal);
            const shortNew = this.prettyValue(newVal);
            diffs.push(`${this.humanizeKey(key)}: ${shortOld} → ${shortNew}`);
          }
        }

        if (diffs.length === 0) return null;

        // If the number of diffs is small, show them all inline; otherwise show first 3 with a +N indicator
        if (diffs.length <= 5) {
          return diffs.join(', ');
        }

        return diffs.slice(0, 3).join(', ') + (diffs.length > 3 ? ` (+${diffs.length - 3} more)` : '');
      }

      // Fallback to inspecting additionalData.RequestBody for key fields
      if (additionalData) {
        try {
          const ad = JSON.parse(additionalData);
          if (ad && ad.RequestBody) {
            const req = typeof ad.RequestBody === 'string' ? JSON.parse(ad.RequestBody) : ad.RequestBody;
            const keys = ['email', 'name', 'title', 'status'];
            const parts: string[] = [];
            for (const k of keys) {
              if (req[k]) parts.push(`${this.humanizeKey(k)}: ${this.prettyValue(req[k])}`);
            }
            if (parts.length) return parts.slice(0, 3).join(', ');
          }
        } catch {}
      }

      return null;
    } catch (e) {
      return null;
    }
  }

  // For create actions, pick a couple of key fields to show inline
  private summarizeNewValues(newValues?: string, additionalData?: string): string | null {
    try {
      const obj = newValues ? JSON.parse(newValues) : null;
      if (obj && typeof obj === 'object') {
        const keys = ['email', 'name', 'title', 'status', 'department', 'projectName'];
        const parts: string[] = [];
        for (const k of keys) {
          if (obj[k]) parts.push(`${this.humanizeKey(k)}: ${this.prettyValue(obj[k])}`);
          if (parts.length >= 3) break;
        }
        if (parts.length) return parts.join(', ');
      }

      if (additionalData) {
        try {
          const ad = JSON.parse(additionalData);
          if (ad && ad.RequestBody) {
            const req = typeof ad.RequestBody === 'string' ? JSON.parse(ad.RequestBody) : ad.RequestBody;
            const keys = ['email', 'name', 'title', 'status'];
            const parts: string[] = [];
            for (const k of keys) {
              if (req[k]) parts.push(`${this.humanizeKey(k)}: ${this.prettyValue(req[k])}`);
            }
            if (parts.length) return parts.join(', ');
          }
        } catch {}
      }

      return null;
    } catch (e) {
      return null;
    }
  }

  private humanizeKey(key: string): string {
    // Convert camelCase or snake_case to normal readable label
    if (!key) return '';
    const withSpaces = key.replace(/([a-z0-9])([A-Z])/g, '$1 $2').replace(/[_-]/g, ' ');
    return withSpaces.charAt(0).toUpperCase() + withSpaces.slice(1);
  }

  private prettyValue(val: any, maxLength?: number | null): string {
    if (val === null || val === undefined) return 'null';
    if (typeof val === 'string') {
      if (typeof maxLength === 'number' && maxLength > 0) {
        return val.length > maxLength ? val.substring(0, maxLength - 3) + '...' : val;
      }
      return val;
    }
    if (typeof val === 'object') return JSON.stringify(val).replace(/\s+/g, ' ');
    const s = String(val);
    if (typeof maxLength === 'number' && maxLength > 0) {
      return s.length > maxLength ? s.substring(0, maxLength - 3) + '...' : s;
    }
    return s;
  }

  // Public helper to compute a full human-readable diff (multiline) for display in details modal
  computeFullDiffString(oldValues?: string, newValues?: string): string | null {
    try {
      if (!oldValues && !newValues) return null;

      const oldObj = oldValues ? JSON.parse(oldValues) : {};
      const newObj = newValues ? JSON.parse(newValues) : {};

      if (typeof oldObj !== 'object' || typeof newObj !== 'object') return null;

      const keys = Array.from(new Set([...Object.keys(oldObj), ...Object.keys(newObj)])).sort();
      const lines: string[] = [];

      for (const key of keys) {
        const oldVal = oldObj[key];
        const newVal = newObj[key];
        if (JSON.stringify(oldVal) !== JSON.stringify(newVal)) {
          lines.push(`${this.humanizeKey(key)}:\n  - old: ${this.prettyValue(oldVal, null)}\n  - new: ${this.prettyValue(newVal, null)}`);
        }
      }

      if (lines.length === 0) return null;
      return lines.join('\n\n');
    } catch (e) {
      return null;
    }
  }

  downloadFile(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }
}