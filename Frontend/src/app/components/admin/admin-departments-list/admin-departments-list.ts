import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DepartmentService } from '../../../services/departments/department-service';
import { IDepartment } from '../../../models/entities/idepartment';
import { IFilteredDepartmentsRequest } from '../../../models/requests/ifiltered-departments-request';
import { DepartmentViewModel } from '../../../view-models/department-view-model';

@Component({
  selector: 'app-admin-departments-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-departments-list.html',
  styleUrl: './admin-departments-list.css'
})
export class AdminDepartmentsList implements OnInit {
  departments: DepartmentViewModel[] = [];

  searchTerm: string = '';
  isLoading: boolean = true;
  error: string = '';
  
  readonly pageSize = 6;
  currentPage: number = 1;
  totalPages: number = 0;
  sortDescending: boolean = false;

  constructor(private departmentService: DepartmentService) { }

  ngOnInit(): void {
    this.loadDepartments();
  }

  loadDepartments(): void {
    this.isLoading = true;
    this.error = '';

    const params: IFilteredDepartmentsRequest = {
      name: this.searchTerm,
      params: {
        sortBy: "name", 
        sortDescending: this.sortDescending,
        page: this.currentPage,
        pageSize: this.pageSize
      }
    };

    this.departmentService.getAllDepartmentsFiltered(params).subscribe({
      next: (response) => {
        console.log(response);
        this.departments = response.data.map(d => this.mapToDepartmentViewModel(d));
        this.totalPages = response.totalPages
        
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.error = 'Failed to load departments. Please try again later.';
        console.error('Error loading departments:', err);
        this.departments = [];
      }
    });
  }

  mapToDepartmentViewModel(department: IDepartment): DepartmentViewModel {
    return {
      id: department.id,
      name: department.name,
      description: department.description
    };
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page;
      this.loadDepartments();
    }
  }

  onSearch(): void {
    this.currentPage = 1; 
    this.loadDepartments();
  }

  goToNextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadDepartments();
    }
  }

  goToPreviousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadDepartments();
    }
  }

  goToFirstPage(): void {
    if (this.currentPage !== 1) {
      this.currentPage = 1;
      this.loadDepartments();
    }
  }

  goToLastPage(): void {
    if (this.currentPage !== this.totalPages) {
      this.currentPage = this.totalPages;
      this.loadDepartments();
    }
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisiblePages = 5;
    const half = Math.floor(maxVisiblePages / 2);
    
    let start = Math.max(1, this.currentPage - half);
    let end = Math.min(this.totalPages, start + maxVisiblePages - 1);
    
    if (end - start + 1 < maxVisiblePages) {
      start = Math.max(1, end - maxVisiblePages + 1);
    }
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    return pages;
  }

  getStartResultIndex(): number {
    return ((this.currentPage - 1) * this.pageSize) + 1;
  }

  onSearchChange(): void {
    this.currentPage = 1; 
    this.loadDepartments();
  }

  toggleSortOrder(): void {
    this.sortDescending = !this.sortDescending;
    this.currentPage = 1; 
    this.loadDepartments();
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.currentPage = 1; 
    this.loadDepartments();
  }

  editDepartment(department: DepartmentViewModel): void {
    console.log('Edit department:', department);
  }

  viewDepartment(department: DepartmentViewModel): void {
    console.log('View department:', department);
  }

  deleteDepartment(department: DepartmentViewModel): void {
    console.log('Delete department:', department);
  }

  onFormSubmit(event: { type: string, data: any }): void {
    if (event.type === 'department') {
      this.loadDepartments();
    }
  }
}