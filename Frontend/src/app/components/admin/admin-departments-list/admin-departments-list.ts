import { Component } from '@angular/core';
import { OnInit } from '@angular/core'; 
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface Department {
  id: string;
  name: string;
  code: string;
  description: string;
  employeeCount: number;
  managerCount: number;
  budgetUsed: number;
  status: 'active' | 'inactive';
  color: string;
  headManager?: {
    id: string;
    name: string;
    avatar?: string;
  };
  createdDate?: Date;
  lastUpdated?: Date;
}

@Component({
  selector: 'app-admin-departments-list',
  imports: [CommonModule ,FormsModule],
  templateUrl: './admin-departments-list.html',
  styleUrl: './admin-departments-list.css'
})
export class AdminDepartmentsList implements OnInit {
  departments: Department[] = [];
  filteredDepartments: Department[] = [];
  searchTerm: string = '';
  selectedStatus: string = '';

  constructor() { }

  ngOnInit(): void {
    this.loadDepartments();
  }

  loadDepartments(): void {
    // Mock data - replace with actual service call
    this.departments = [
      {
        id: '1',
        name: 'Engineering',
        code: 'ENG',
        description: 'Software development and technical infrastructure team responsible for building and maintaining our products.',
        employeeCount: 45,
        managerCount: 3,
        budgetUsed: 78,
        status: 'active',
        color: '#3B82F6',
        headManager: {
          id: '1',
          name: 'John Smith',
          avatar: 'https://ui-avatars.com/api/?name=John+Smith&background=20B486&color=fff'
        }
      },
      {
        id: '2',
        name: 'Marketing',
        code: 'MKT',
        description: 'Brand management, digital marketing, and customer acquisition strategies.',
        employeeCount: 28,
        managerCount: 2,
        budgetUsed: 65,
        status: 'active',
        color: '#10B981',
        headManager: {
          id: '2',
          name: 'Sarah Johnson',
          avatar: 'https://ui-avatars.com/api/?name=Sarah+Johnson&background=20B486&color=fff'
        }
      },
      {
        id: '3',
        name: 'Sales',
        code: 'SLS',
        description: 'Customer relationship management and revenue generation activities.',
        employeeCount: 32,
        managerCount: 2,
        budgetUsed: 82,
        status: 'active',
        color: '#F59E0B',
        headManager: {
          id: '3',
          name: 'Michael Brown',
          avatar: 'https://ui-avatars.com/api/?name=Michael+Brown&background=20B486&color=fff'
        }
      },
      {
        id: '4',
        name: 'Human Resources',
        code: 'HR',
        description: 'Employee relations, talent acquisition, and organizational development.',
        employeeCount: 12,
        managerCount: 1,
        budgetUsed: 45,
        status: 'active',
        color: '#8B5CF6',
        headManager: {
          id: '4',
          name: 'Emily Davis',
          avatar: 'https://ui-avatars.com/api/?name=Emily+Davis&background=20B486&color=fff'
        }
      },
      {
        id: '5',
        name: 'Finance',
        code: 'FIN',
        description: 'Financial planning, accounting, and budget management.',
        employeeCount: 15,
        managerCount: 1,
        budgetUsed: 92,
        status: 'active',
        color: '#EF4444'
      },
      {
        id: '6',
        name: 'Operations',
        code: 'OPS',
        description: 'Business operations and process optimization.',
        employeeCount: 8,
        managerCount: 1,
        budgetUsed: 33,
        status: 'inactive',
        color: '#6B7280'
      }
    ];

    this.filteredDepartments = [...this.departments];
  }

  onSearchChange(): void {
    this.filterDepartments();
  }

  onStatusChange(): void {
    this.filterDepartments();
  }

  private filterDepartments(): void {
    this.filteredDepartments = this.departments.filter(department => {
      const matchesSearch = !this.searchTerm || 
        department.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        department.code.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        department.description.toLowerCase().includes(this.searchTerm.toLowerCase());

      const matchesStatus = !this.selectedStatus || 
        department.status === this.selectedStatus;

      return matchesSearch && matchesStatus;
    });
  }

  editDepartment(department: Department): void {
    console.log('Edit department:', department);
    // Implement edit functionality
  }

  viewDepartment(department: Department): void {
    console.log('View department:', department);
    // Implement view functionality
  }

  deleteDepartment(department: Department): void {
    if (confirm(`Are you sure you want to delete ${department.name} department?`)) {
      this.departments = this.departments.filter(d => d.id !== department.id);
      this.filterDepartments();
      console.log('Department deleted:', department);
    }
  }

  // Utility methods
  getTotalEmployees(): number {
    return this.departments.reduce((total, dept) => total + dept.employeeCount, 0);
  }

  getAverageBudgetUtilization(): number {
    if (this.departments.length === 0) return 0;
    const total = this.departments.reduce((sum, dept) => sum + dept.budgetUsed, 0);
    return Math.round(total / this.departments.length);
  }

  getActiveDepartments(): number {
    return this.departments.filter(dept => dept.status === 'active').length;
  }
}