import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuditLogService, AuditLog } from '../../../services/auditLogService/audit-log.service';

@Component({
  selector: 'app-admin-audit-logs-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-audit-logs-list.html'
})
export class AdminAuditLogsList implements OnInit {
  auditLogs: AuditLog[] = [];
  loading = false;
  error: string | null = null;
  Math = Math;

  // Pagination
  currentPage = 1;
  pageSize = 25;
  totalPages = 0;
  totalCount = 0;

  // Filters
  filters = {
    userId: '',
    action: '',
    entityType: '',
    startDate: '',
    endDate: '',
    search: ''
  };

  // Available filter options
  availableActions: string[] = [];
  availableEntityTypes: string[] = [];

  // Selected log for details
  selectedLog: AuditLog | null = null;
  showDetails = false;

  // Summary data
  showSummary = false;
  summaryData: any = null;
  summaryLoading = false;

  constructor(public auditLogService: AuditLogService) {}

  ngOnInit(): void {
    this.loadAuditLogs();
    this.loadAvailableFilters();
    this.setDefaultDateRange();
  }

  private setDefaultDateRange(): void {
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(startDate.getDate() - 7); // Last 7 days

    this.filters.endDate = endDate.toISOString().split('T')[0];
    this.filters.startDate = startDate.toISOString().split('T')[0];
  }

  loadAuditLogs(): void {
    this.loading = true;
    this.error = null;

    const filters = Object.fromEntries(
      Object.entries(this.filters).filter(([_, value]) => value !== '')
    );

    this.auditLogService.getAuditLogs(this.currentPage, this.pageSize, filters)
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.auditLogs = response.data.data;
            this.currentPage = response.data.pageNumber;
            this.pageSize = response.data.pageSize;
            this.totalPages = response.data.totalPages;
            this.totalCount = response.data.totalCount;
          } else {
            this.error = 'Failed to load audit logs';
          }
          this.loading = false;
        },
        error: (error) => {
          this.error = 'Error loading audit logs: ' + (error.error?.error || error.message);
          this.loading = false;
        }
      });
  }

  loadAvailableFilters(): void {
    this.auditLogService.getAvailableFilters().subscribe({
      next: (response) => {
        if (response.success) {
          this.availableActions = response.data.actions;
          this.availableEntityTypes = response.data.entityTypes;
        }
      },
      error: (error) => {
        console.error('Error loading filter options:', error);
      }
    });
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadAuditLogs();
  }

  clearFilters(): void {
    this.filters = {
      userId: '',
      action: '',
      entityType: '',
      startDate: '',
      endDate: '',
      search: ''
    };
    this.setDefaultDateRange();
    this.currentPage = 1;
    this.loadAuditLogs();
  }

  changePage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadAuditLogs();
    }
  }

  get pageNumbers(): number[] {
    const pages: number[] = [];
    const maxPagesToShow = 5;
    const halfMax = Math.floor(maxPagesToShow / 2);

    let startPage = Math.max(1, this.currentPage - halfMax);
    let endPage = Math.min(this.totalPages, this.currentPage + halfMax);

    if (endPage - startPage + 1 < maxPagesToShow) {
      if (startPage === 1) {
        endPage = Math.min(this.totalPages, startPage + maxPagesToShow - 1);
      } else {
        startPage = Math.max(1, endPage - maxPagesToShow + 1);
      }
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return pages;
  }

  showLogDetails(log: AuditLog): void {
    this.selectedLog = log;
    this.showDetails = true;
  }

  closeDetails(): void {
    this.showDetails = false;
    this.selectedLog = null;
  }

  loadSummary(): void {
    this.summaryLoading = true;
    this.auditLogService.getAuditSummary(7).subscribe({
      next: (response) => {
        if (response.success) {
          this.summaryData = response.data;
          this.showSummary = true;
        }
        this.summaryLoading = false;
      },
      error: (error) => {
        console.error('Error loading summary:', error);
        this.summaryLoading = false;
      }
    });
  }

  closeSummary(): void {
    this.showSummary = false;
    this.summaryData = null;
  }

  exportLogs(format: 'json' | 'csv'): void {
    const filters = Object.fromEntries(
      Object.entries(this.filters).filter(([_, value]) => value !== '')
    );

    this.auditLogService.exportAuditLogs(format, filters).subscribe({
      next: (blob) => {
        const filename = `audit-logs-${new Date().toISOString().split('T')[0]}.${format}`;
        this.auditLogService.downloadFile(blob, filename);
      },
      error: (error) => {
        console.error('Error exporting logs:', error);
      }
    });
  }

  getActionIcon(action: string): string {
    return this.auditLogService.getActionIcon(action);
  }

  getActionColor(action: string): string {
    return this.auditLogService.getActionColor(action);
  }

  formatTimestamp(timestamp: string): string {
    return this.auditLogService.formatTimestamp(timestamp);
  }

  formatJsonData(jsonString?: string): string {
    if (!jsonString) return 'N/A';

    try {
      const parsed = JSON.parse(jsonString);
      return JSON.stringify(parsed, null, 2);
    } catch {
      return jsonString;
    }
  }

  truncateText(text: string, maxLength: number = 50): string {
    if (!text) return '';
    return text.length > maxLength ? text.substring(0, maxLength) + '...' : text;
  }

  getSuccessIcon(success: boolean): string {
    return success ? '✅' : '❌';
  }

  getSuccessColor(success: boolean): string {
    return success ? 'text-green-600' : 'text-red-600';
  }
}