import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DepartmentService } from '../../../services/departments/department-service';
import { IDepartment } from '../../../models/entities/idepartment';
import { IFilteredDepartmentsRequest } from '../../../models/requests/ifiltered-departments-request';
import { IDepartmentViewModel } from '../../../view-models/department-view-model';
import { IApiResponse } from '../../../models/responses/iapi-response';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-admin-departments-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-departments-list.html',
  styleUrl: './admin-departments-list.css'
})
export class AdminDepartmentsList implements OnInit {
  departments: IDepartmentViewModel[] = [];
  searchTerm: string = '';
  currentSearchTerm: string = ''; // Termenul folosit pentru ultima căutare
  isLoading: boolean = true;
  error: string = '';
  
  readonly pageSize = 6;
  currentPage: number = 1;
  totalPages: number = 0;
  sortDescending: boolean = false;

  // Edit modal properties
  showEditModal = false;
  departmentToEdit: IDepartmentViewModel | null = null;
  
  editForm = {
    id: 0,
    name: '',
    description: ''
  };
  
  isSubmitting = false;
  errorMessage = '';
  hasInitiallyLoaded = false;

  constructor(private departmentService: DepartmentService) { }

  ngOnInit(): void {
    this.loadDepartments();
  }

  loadDepartments(): void {
    this.isLoading = true;
    this.error = '';

    const params: IFilteredDepartmentsRequest = {
      name: this.currentSearchTerm,
      includeDeleted: true,
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
        this.departments = response.data.data.map(d => this.mapToDepartmentViewModel(d));
        this.totalPages = response.data.totalPages;
        this.hasInitiallyLoaded = true;
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.hasInitiallyLoaded = true;
        this.error = 'Failed to load departments. Please try again later.';
        console.error('Error loading departments:', err);
        this.departments = [];
      }
    });
  }

  getEmptyStateMessage(): string {
    if (!this.hasInitiallyLoaded) {
      return '';
    }
    
    if (this.currentSearchTerm.trim()) {
      return `No departments match your search for "${this.currentSearchTerm}".`;
    }
    
    return 'No departments found. Get started by creating a new department.';
  }

  shouldShowEmptyState(): boolean {
    return !this.isLoading && !this.error && this.departments.length === 0 && this.hasInitiallyLoaded;
  }

  mapToDepartmentViewModel(department: IDepartment): IDepartmentViewModel {
    return {
      id: department.id,
      name: department.name,
      description: department.description,
      employeeCount: department.employeeCount,
      isActive: department.deletedAt === null
    };
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page;
      this.loadDepartments();
    }
  }

  onSearch(): void {
    this.currentSearchTerm = this.searchTerm;
    this.currentPage = 1; 
    this.loadDepartments();
  }

  onSearchKeypress(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.onSearch();
    }
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

  toggleSortOrder(): void {
    this.sortDescending = !this.sortDescending;
    this.currentPage = 1; 
    this.loadDepartments();
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.currentSearchTerm = '';
    this.currentPage = 1; 
    this.loadDepartments();
  }

  editDepartment(department: IDepartmentViewModel): void {
    this.departmentToEdit = { ...department };
    this.populateEditForm(department);
    this.showEditModal = true;
  }

  populateEditForm(department: IDepartmentViewModel): void {
    this.editForm = {
      id: department.id,
      name: department.name || '',
      description: department.description || ''
    };
    this.errorMessage = '';
  }

  onSubmitEdit(): void {
    if (!this.isFormValid()) {
      this.errorMessage = 'Please fill in all required fields.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const departmentToUpdate: IDepartment = {
      id: this.editForm.id,
      name: this.editForm.name,
      description: this.editForm.description
    };

    this.departmentService.updateDepartment(this.editForm.id, departmentToUpdate).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        if (response.success) {
          this.showEditModal = false;
          this.loadDepartments();
          console.log('Department updated successfully:', response.data);
        } else {
          this.errorMessage = response.message || 'Failed to update department';
        }
      },
      error: (error) => {
        this.isSubmitting = false;
        this.errorMessage = 'Error updating department: ' + (error.error?.message || error.message);
        console.error('Error updating department:', error);
      }
    });
  }

  isFormValid(): boolean {
    return !!(this.editForm.name.trim());
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.departmentToEdit = null;
    this.errorMessage = '';
  }

  viewDepartment(department: IDepartmentViewModel): void {
    console.log('View department:', department);
  }

  deleteDepartment(department: IDepartmentViewModel): void {
    if (confirm(`Are you sure you want to delete the department "${department.name}"?`)) {
      this.departmentService.deleteDepartment(department.id).subscribe({
        next: (response) => {
          if (response.success) {
            console.log('Department deleted successfully:', response.data);
            this.loadDepartments();
          } else {
            alert('Failed to delete department: ' + response.message);
          }
        },
        error: (err) => {
          console.error('Failed to delete department:', err);
          alert('Failed to delete department. Please try again.');
        }
      });
    }
  }

  onFormSubmit(event: { type: string, data: any }): void {
    if (event.type === 'department') {
      this.loadDepartments();
    }
  }

  restoreDepartment(id: number) {
    this.departmentService.restoreDepartment(id).subscribe({
      next: (response: IApiResponse<IDepartment>) => {
        if(response.success){
          this.loadDepartments();
        }
      },
      error: (err: HttpErrorResponse) => {
        console.log(`Restore failed: ${err.error.message}`);
        alert('Failed to restore department. Please try again.');
      }
    })
  }
}