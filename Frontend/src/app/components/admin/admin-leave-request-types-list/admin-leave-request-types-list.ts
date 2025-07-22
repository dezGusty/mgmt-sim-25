import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface LeaveType {
  id: string;
  name: string;
  code: string;
  description: string;
  category: 'paid' | 'unpaid' | 'sick' | 'special';
  maxDaysPerYear: number; // -1 for unlimited
  advanceNoticeDays: number;
  requiresApproval: boolean;
  requiresDocumentation: boolean;
  status: 'active' | 'inactive';
  color: string;
  usageCount?: number;
  averageDuration?: number;
  minimumTenure?: number; // in months
  eligibleRoles?: string[];
  probationPeriodExcluded?: boolean;
  carryOverAllowed?: boolean;
  maxCarryOverDays?: number;
  createdDate?: Date;
  lastUpdated?: Date;
}

@Component({
  selector: 'app-admin-leave-request-types-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-leave-request-types-list.html',
  styleUrl: './admin-leave-request-types-list.css'
})
export class AdminLeaveTypesList implements OnInit {
  leaveTypes: LeaveType[] = [];
  filteredLeaveTypes: LeaveType[] = [];
  searchTerm: string = '';
  selectedCategory: string = '';
  selectedStatus: string = '';

  constructor() { }

  ngOnInit(): void {
    this.loadLeaveTypes();
  }

  loadLeaveTypes(): void {
    // Mock data - replace with actual service call
    this.leaveTypes = [
      {
        id: '1',
        name: 'Annual Leave',
        code: 'AL',
        description: 'Standard annual vacation leave for rest and recreation.',
        category: 'paid',
        maxDaysPerYear: 21,
        advanceNoticeDays: 14,
        requiresApproval: true,
        requiresDocumentation: false,
        status: 'active',
        color: '#10B981',
        usageCount: 145,
        averageDuration: 5,
        minimumTenure: 3,
        eligibleRoles: ['employee', 'manager'],
        probationPeriodExcluded: true,
        carryOverAllowed: true,
        maxCarryOverDays: 5
      },
      {
        id: '2',
        name: 'Sick Leave',
        code: 'SL',
        description: 'Medical leave for illness or injury recovery.',
        category: 'sick',
        maxDaysPerYear: 10,
        advanceNoticeDays: 0,
        requiresApproval: false,
        requiresDocumentation: true,
        status: 'active',
        color: '#EF4444',
        usageCount: 89,
        averageDuration: 2,
        eligibleRoles: ['employee', 'manager'],
        probationPeriodExcluded: false
      },
      {
        id: '3',
        name: 'Maternity Leave',
        code: 'ML',
        description: 'Leave for new mothers following childbirth.',
        category: 'special',
        maxDaysPerYear: 90,
        advanceNoticeDays: 30,
        requiresApproval: true,
        requiresDocumentation: true,
        status: 'active',
        color: '#F59E0B',
        usageCount: 12,
        averageDuration: 75,
        minimumTenure: 12,
        eligibleRoles: ['employee', 'manager'],
        probationPeriodExcluded: true
      },
      {
        id: '4',
        name: 'Paternity Leave',
        code: 'PL',
        description: 'Leave for new fathers following childbirth or adoption.',
        category: 'special',
        maxDaysPerYear: 14,
        advanceNoticeDays: 30,
        requiresApproval: true,
        requiresDocumentation: true,
        status: 'active',
        color: '#8B5CF6',
        usageCount: 8,
        averageDuration: 10,
        minimumTenure: 12,
        eligibleRoles: ['employee', 'manager'],
        probationPeriodExcluded: true
      },
      {
        id: '5',
        name: 'Personal Leave',
        code: 'PRL',
        description: 'Unpaid leave for personal matters and emergencies.',
        category: 'unpaid',
        maxDaysPerYear: 30,
        advanceNoticeDays: 7,
        requiresApproval: true,
        requiresDocumentation: false,
        status: 'active',
        color: '#6B7280',
        usageCount: 34,
        averageDuration: 3,
        eligibleRoles: ['employee', 'manager'],
        probationPeriodExcluded: false
      },
      {
        id: '6',
        name: 'Study Leave',
        code: 'STL',
        description: 'Leave for educational pursuits and professional development.',
        category: 'unpaid',
        maxDaysPerYear: 20,
        advanceNoticeDays: 21,
        requiresApproval: true,
        requiresDocumentation: true,
        status: 'active',
        color: '#3B82F6',
        usageCount: 18,
        averageDuration: 7,
        minimumTenure: 6,
        eligibleRoles: ['employee', 'manager'],
        probationPeriodExcluded: true
      },
      {
        id: '7',
        name: 'Bereavement Leave',
        code: 'BL',
        description: 'Compassionate leave for death of family members.',
        category: 'paid',
        maxDaysPerYear: 5,
        advanceNoticeDays: 0,
        requiresApproval: false,
        requiresDocumentation: true,
        status: 'active',
        color: '#374151',
        usageCount: 15,
        averageDuration: 3,
        eligibleRoles: ['employee', 'manager'],
        probationPeriodExcluded: false
      },
      {
        id: '8',
        name: 'Sabbatical Leave',
        code: 'SAB',
        description: 'Extended unpaid leave for long-term personal projects.',
        category: 'unpaid',
        maxDaysPerYear: 365,
        advanceNoticeDays: 90,
        requiresApproval: true,
        requiresDocumentation: true,
        status: 'inactive',
        color: '#92400E',
        usageCount: 2,
        averageDuration: 180,
        minimumTenure: 60,
        eligibleRoles: ['manager'],
        probationPeriodExcluded: true
      }
    ];

    this.filteredLeaveTypes = [...this.leaveTypes];
  }

  onSearchChange(): void {
    this.filterLeaveTypes();
  }

  onCategoryChange(): void {
    this.filterLeaveTypes();
  }

  onStatusChange(): void {
    this.filterLeaveTypes();
  }

  private filterLeaveTypes(): void {
    this.filteredLeaveTypes = this.leaveTypes.filter(leaveType => {
      const matchesSearch = !this.searchTerm || 
        leaveType.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        leaveType.code.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        leaveType.description.toLowerCase().includes(this.searchTerm.toLowerCase());

      const matchesCategory = !this.selectedCategory || 
        leaveType.category === this.selectedCategory;

      const matchesStatus = !this.selectedStatus || 
        leaveType.status === this.selectedStatus;

      return matchesSearch && matchesCategory && matchesStatus;
    });
  }

  getCategoryBadgeClass(category: string): string {
    switch (category) {
      case 'paid':
        return 'bg-green-100 text-green-800';
      case 'unpaid':
        return 'bg-gray-100 text-gray-800';
      case 'sick':
        return 'bg-red-100 text-red-800';
      case 'special':
        return 'bg-purple-100 text-purple-800';
      default:
        return 'bg-blue-100 text-blue-800';
    }
  }

  editLeaveType(leaveType: LeaveType): void {
    console.log('Edit leave type:', leaveType);
    // Implement edit functionality
  }

  viewLeaveType(leaveType: LeaveType): void {
    console.log('View leave type:', leaveType);
    // Implement view functionality
  }

  duplicateLeaveType(leaveType: LeaveType): void {
    const newLeaveType: LeaveType = {
      ...leaveType,
      id: Date.now().toString(),
      name: `${leaveType.name} (Copy)`,
      code: `${leaveType.code}-COPY`,
      usageCount: 0
    };
    
    this.leaveTypes.push(newLeaveType);
    this.filterLeaveTypes();
    console.log('Leave type duplicated:', newLeaveType);
  }

  deleteLeaveType(leaveType: LeaveType): void {
    if (leaveType.usageCount && leaveType.usageCount > 0) {
      alert(`Cannot delete ${leaveType.name} because it has been used ${leaveType.usageCount} times.`);
      return;
    }

    if (confirm(`Are you sure you want to delete ${leaveType.name}?`)) {
      this.leaveTypes = this.leaveTypes.filter(lt => lt.id !== leaveType.id);
      this.filterLeaveTypes();
      console.log('Leave type deleted:', leaveType);
    }
  }

  // Statistics methods
  getTotalLeaveTypes(): number {
    return this.leaveTypes.length;
  }

  getActiveLeaveTypes(): number {
    return this.leaveTypes.filter(lt => lt.status === 'active').length;
  }

  getPaidLeaveTypes(): number {
    return this.leaveTypes.filter(lt => lt.category === 'paid').length;
  }

  getTotalRequestsThisYear(): number {
    return this.leaveTypes.reduce((total, lt) => total + (lt.usageCount || 0), 0);
  }

  getAverageDuration(): number {
    const activeLeaveTypes = this.leaveTypes.filter(lt => lt.averageDuration);
    if (activeLeaveTypes.length === 0) return 0;
    
    const total = activeLeaveTypes.reduce((sum, lt) => sum + (lt.averageDuration || 0), 0);
    return Math.round(total / activeLeaveTypes.length);
  }

  // Quick actions
  createStandardLeaveType(): void {
    console.log('Creating standard leave types...');
    // Implement standard leave type creation (Annual, Sick, Personal)
  }

  bulkImportLeaveTypes(): void {
    console.log('Opening bulk import dialog...');
    // Implement bulk import functionality
  }

  exportLeaveTypes(): void {
    console.log('Exporting leave types...');
    // Implement export functionality
    const dataStr = JSON.stringify(this.leaveTypes, null, 2);
    const dataBlob = new Blob([dataStr], { type: 'application/json' });
    
    const link = document.createElement('a');
    link.href = URL.createObjectURL(dataBlob);
    link.download = 'leave-types-export.json';
    link.click();
  }

  // Utility methods
  formatMaxDays(maxDays: number): string {
    return maxDays === -1 ? 'Unlimited' : `${maxDays} days`;
  }

  getLeaveTypesByCategory(category: string): LeaveType[] {
    return this.leaveTypes.filter(lt => lt.category === category);
  }

  toggleLeaveTypeStatus(leaveType: LeaveType): void {
    leaveType.status = leaveType.status === 'active' ? 'inactive' : 'active';
    console.log(`Leave type ${leaveType.name} status changed to ${leaveType.status}`);
  }
}
