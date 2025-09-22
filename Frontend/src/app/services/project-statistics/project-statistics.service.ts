import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AuditLogService, AuditLog } from '../auditLogService/audit-log.service';
import { ProjectService } from '../projects/project.service';
import {
  IProjectStatistics,
  IMonthlyAllocationData,
  IEmployeeActivityData,
  IBudgetUtilization,
  IProjectMilestone,
  IProjectActivitySummary,
  IAllocationEvent
} from '../../models/entities/iproject-statistics';

@Injectable({
  providedIn: 'root'
})
export class ProjectStatisticsService {

  constructor(
    private http: HttpClient,
    private auditLogService: AuditLogService,
    private projectService: ProjectService
  ) {}

  getProjectStatistics(projectId: number): Observable<IProjectStatistics> {
    return forkJoin({
      projectDetails: this.projectService.getProjectWithUsers(projectId),
      auditLogs: this.getProjectAuditLogs(projectId)
    }).pipe(
      map(({ projectDetails, auditLogs }) => {
        if (!projectDetails.success || !projectDetails.data) {
          throw new Error('Failed to load project details');
        }

        const project = projectDetails.data;
        const allocationEvents = this.extractAllocationEvents(auditLogs);

        return {
          projectId: projectId,
          projectName: project.name || 'Unknown Project',
          monthlyAllocationData: this.generateMonthlyAllocationData(allocationEvents, project),
          employeeActivity: this.generateEmployeeActivity(allocationEvents, project),
          budgetUtilization: this.generateBudgetUtilization(allocationEvents, project),
          milestones: this.generateMilestones(allocationEvents, project),
          summary: this.generateActivitySummary(allocationEvents, project),
          lastUpdated: new Date()
        } as IProjectStatistics;
      }),
      catchError(error => {
        console.error('Error generating project statistics:', error);
        return of(this.getEmptyStatistics(projectId));
      })
    );
  }

  private getProjectAuditLogs(projectId: number): Observable<AuditLog[]> {
    const startDate = new Date();
    startDate.setFullYear(startDate.getFullYear() - 1); // Get last year of data

    return this.auditLogService.getEntityAuditTrail('Project', projectId).pipe(
      map(response => response.success ? response.data : []),
      catchError(() => of([]))
    );
  }

  private extractAllocationEvents(auditLogs: AuditLog[]): IAllocationEvent[] {
    const events: IAllocationEvent[] = [];

    auditLogs.forEach(log => {
      // Look for project assignment related actions
      if (log.entityType.toLowerCase() === 'userproject' ||
          log.endpoint?.includes('/projects/') && log.endpoint?.includes('/users')) {

        const event = this.parseAllocationEvent(log);
        if (event) {
          events.push(event);
        }
      }
    });

    return events.sort((a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime());
  }

  private parseAllocationEvent(log: AuditLog): IAllocationEvent | null {
    try {
      let action: 'assigned' | 'removed' | 'percentage_changed' = 'assigned';
      let oldPercentage: number | undefined;
      let newPercentage: number | undefined;
      let employeeName = 'Unknown Employee';
      let employeeEmail = log.userEmail || 'unknown@email.com';
      let employeeId = log.userId;

      // Determine action type
      if (log.action.toUpperCase() === 'DELETE') {
        action = 'removed';
      } else if (log.action.toUpperCase() === 'UPDATE') {
        action = 'percentage_changed';
      } else if (log.action.toUpperCase() === 'CREATE') {
        action = 'assigned';
      }

      // Try to extract percentage information from old/new values
      if (log.oldValues) {
        try {
          const oldData = JSON.parse(log.oldValues);
          oldPercentage = oldData.timePercentagePerProject || oldData.assignedPercentage;
        } catch (e) {}
      }

      if (log.newValues) {
        try {
          const newData = JSON.parse(log.newValues);
          newPercentage = newData.timePercentagePerProject || newData.assignedPercentage;
          // Try to get employee info from new values
          if (newData.userName) employeeName = newData.userName;
          if (newData.userEmail) employeeEmail = newData.userEmail;
          if (newData.userId) employeeId = newData.userId;
        } catch (e) {}
      }

      // Try to extract from additional data
      if (log.additionalData) {
        try {
          const additionalData = JSON.parse(log.additionalData);
          if (additionalData.RequestBody) {
            const requestBody = typeof additionalData.RequestBody === 'string'
              ? JSON.parse(additionalData.RequestBody)
              : additionalData.RequestBody;

            if (requestBody.timePercentagePerProject !== undefined) {
              newPercentage = requestBody.timePercentagePerProject;
            }
            if (requestBody.userId) employeeId = requestBody.userId;
          }
        } catch (e) {}
      }

      // Use entity name if available
      if (log.entityName && log.entityName !== 'HttpRequest') {
        employeeName = log.entityName;
      }

      return {
        id: log.id,
        employeeId: employeeId,
        employeeName: employeeName,
        employeeEmail: employeeEmail,
        action: action,
        oldPercentage: oldPercentage,
        newPercentage: newPercentage,
        timestamp: new Date(log.timestamp),
        auditLogId: log.id
      } as IAllocationEvent;

    } catch (error) {
      console.warn('Failed to parse allocation event from audit log:', log.id, error);
      return null;
    }
  }

  private generateMonthlyAllocationData(events: IAllocationEvent[], project: any): IMonthlyAllocationData[] {
    const monthlyData = new Map<string, IMonthlyAllocationData>();

    // Initialize months from project start to now
    const startDate = new Date(project.startDate);
    const endDate = new Date();

    for (let d = new Date(startDate); d <= endDate; d.setMonth(d.getMonth() + 1)) {
      const monthKey = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`;
      monthlyData.set(monthKey, {
        month: monthKey,
        allocations: 0,
        deallocations: 0,
        totalEmployees: 0,
        totalFTEs: 0
      });
    }

    // Process events
    events.forEach(event => {
      const eventDate = new Date(event.timestamp);
      const monthKey = `${eventDate.getFullYear()}-${String(eventDate.getMonth() + 1).padStart(2, '0')}`;

      const monthData = monthlyData.get(monthKey);
      if (monthData) {
        if (event.action === 'assigned') {
          monthData.allocations++;
        } else if (event.action === 'removed') {
          monthData.deallocations++;
        }
      }
    });

    // Calculate cumulative totals
    let runningTotal = 0;
    const sortedData = Array.from(monthlyData.values()).sort((a, b) => a.month.localeCompare(b.month));

    sortedData.forEach(data => {
      runningTotal += (data.allocations - data.deallocations);
      data.totalEmployees = Math.max(0, runningTotal);
      data.totalFTEs = this.calculateFTEsForMonth(data.month, events);
    });

    return sortedData;
  }

  private calculateFTEsForMonth(month: string, events: IAllocationEvent[]): number {
    // Calculate total FTEs at the end of the given month
    const monthEnd = new Date(month + '-01');
    monthEnd.setMonth(monthEnd.getMonth() + 1);
    monthEnd.setDate(0); // Last day of the month

    let totalFTEs = 0;
    const activeEmployees = new Map<number, number>();

    // Process events up to the end of the month
    events.filter(event => new Date(event.timestamp) <= monthEnd).forEach(event => {
      if (event.action === 'assigned' && event.newPercentage) {
        activeEmployees.set(event.employeeId, event.newPercentage);
      } else if (event.action === 'removed') {
        activeEmployees.delete(event.employeeId);
      } else if (event.action === 'percentage_changed' && event.newPercentage) {
        activeEmployees.set(event.employeeId, event.newPercentage);
      }
    });

    // Sum up FTEs (assuming full-time = 1.0 FTE)
    activeEmployees.forEach(percentage => {
      totalFTEs += percentage / 100;
    });

    return parseFloat(totalFTEs.toFixed(2));
  }

  private generateEmployeeActivity(events: IAllocationEvent[], project: any): IEmployeeActivityData[] {
    const employeeMap = new Map<number, IEmployeeActivityData>();

    events.forEach(event => {
      if (!employeeMap.has(event.employeeId)) {
        employeeMap.set(event.employeeId, {
          employeeId: event.employeeId,
          employeeName: event.employeeName,
          employeeEmail: event.employeeEmail,
          jobTitle: '', // Would need to fetch from employee data
          allocatedPercentage: 0,
          monthsActive: 0,
          allocationHistory: []
        });
      }

      const employee = employeeMap.get(event.employeeId)!;
      const monthKey = `${event.timestamp.getFullYear()}-${String(event.timestamp.getMonth() + 1).padStart(2, '0')}`;

      if (event.action === 'assigned' || event.action === 'percentage_changed') {
        employee.allocatedPercentage = event.newPercentage || 0;
        employee.allocationHistory.push({
          month: monthKey,
          percentage: event.newPercentage || 0,
          startDate: event.timestamp.toISOString()
        });
      } else if (event.action === 'removed') {
        employee.allocatedPercentage = 0;
        const lastEntry = employee.allocationHistory[employee.allocationHistory.length - 1];
        if (lastEntry) {
          lastEntry.endDate = event.timestamp.toISOString();
        }
      }
    });

    // Calculate months active for each employee
    employeeMap.forEach(employee => {
      const uniqueMonths = new Set(employee.allocationHistory.map(h => h.month));
      employee.monthsActive = uniqueMonths.size;
    });

    return Array.from(employeeMap.values());
  }

  private generateBudgetUtilization(events: IAllocationEvent[], project: any): IBudgetUtilization[] {
    const monthlyData = this.generateMonthlyAllocationData(events, project);

    return monthlyData.map(data => ({
      month: data.month,
      budgetedFTEs: project.budgetedFTEs || 0,
      actualFTEs: data.totalFTEs,
      utilizationPercentage: project.budgetedFTEs > 0
        ? parseFloat(((data.totalFTEs / project.budgetedFTEs) * 100).toFixed(1))
        : 0,
      variance: parseFloat((data.totalFTEs - (project.budgetedFTEs || 0)).toFixed(2))
    }));
  }

  private generateMilestones(events: IAllocationEvent[], project: any): IProjectMilestone[] {
    const milestones: IProjectMilestone[] = [];

    // Project start
    milestones.push({
      date: new Date(project.startDate).toISOString(),
      type: 'project_start',
      description: `Project "${project.name}" started`,
      impact: 'positive'
    });

    // Major allocation events (more than 3 people in a month)
    const monthlyAllocations = new Map<string, number>();
    events.forEach(event => {
      const monthKey = `${event.timestamp.getFullYear()}-${String(event.timestamp.getMonth() + 1).padStart(2, '0')}`;
      monthlyAllocations.set(monthKey, (monthlyAllocations.get(monthKey) || 0) + 1);
    });

    monthlyAllocations.forEach((count, month) => {
      if (count >= 3) {
        milestones.push({
          date: new Date(month + '-01').toISOString(),
          type: 'major_assignment',
          description: `${count} allocation changes in ${month}`,
          impact: 'neutral'
        });
      }
    });

    // Project end (if inactive)
    if (!project.isActive) {
      milestones.push({
        date: new Date(project.endDate).toISOString(),
        type: 'project_end',
        description: `Project "${project.name}" ended`,
        impact: 'neutral'
      });
    }

    return milestones.sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime());
  }

  private generateActivitySummary(events: IAllocationEvent[], project: any): IProjectActivitySummary {
    const uniqueEmployees = new Set(events.map(e => e.employeeId));
    const currentEmployees = project.assignedUsers?.length || 0;

    // Calculate peak employee count
    const monthlyData = this.generateMonthlyAllocationData(events, project);
    const peakMonth = monthlyData.reduce((prev, current) =>
      current.totalEmployees > prev.totalEmployees ? current : prev,
      { totalEmployees: 0, month: '' }
    );

    // Calculate average allocation
    const totalPercentage = events
      .filter(e => e.action === 'assigned' || e.action === 'percentage_changed')
      .reduce((sum, e) => sum + (e.newPercentage || 0), 0);
    const avgAllocation = events.length > 0 ? totalPercentage / events.length : 0;

    // Calculate project duration
    const startDate = new Date(project.startDate);
    const endDate = project.isActive ? new Date() : new Date(project.endDate);
    const durationMonths = Math.ceil((endDate.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24 * 30.44));

    return {
      totalEmployeesEver: uniqueEmployees.size,
      currentEmployees: currentEmployees,
      averageAllocationPercentage: parseFloat(avgAllocation.toFixed(1)),
      peakEmployeeCount: peakMonth.totalEmployees,
      peakEmployeeMonth: peakMonth.month,
      totalAllocationEvents: events.filter(e => e.action === 'assigned').length,
      totalDeallocationEvents: events.filter(e => e.action === 'removed').length,
      projectDuration: durationMonths
    };
  }

  private getEmptyStatistics(projectId: number): IProjectStatistics {
    return {
      projectId: projectId,
      projectName: 'Unknown Project',
      monthlyAllocationData: [],
      employeeActivity: [],
      budgetUtilization: [],
      milestones: [],
      summary: {
        totalEmployeesEver: 0,
        currentEmployees: 0,
        averageAllocationPercentage: 0,
        peakEmployeeCount: 0,
        peakEmployeeMonth: '',
        totalAllocationEvents: 0,
        totalDeallocationEvents: 0,
        projectDuration: 0
      },
      lastUpdated: new Date()
    };
  }
}