import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IJobTitle } from '../../../models/entities/ijob-title';
import { JobTitlesService } from '../../../services/job-titles/job-titles-service';
import { IJobTitleViewModel } from '../../../view-models/job-title-view-model';
import { IFilteredJobTitlesRequest } from '../../../models/requests/ifiltered-job-titles-request';
import { IFilteredApiResponse } from '../../../models/responses/ifiltered-api-response';
import { IApiResponse } from '../../../models/responses/iapi-response';
import { DepartmentService } from '../../../services/departments/department-service';
import { IDepartment } from '../../../models/entities/idepartment';
import { HttpErrorResponse } from '@angular/common/http';
import { JobTitleActivityStatus } from '../../../models/enums/job-title-activity-status';

@Component({
  selector: 'app-admin-job-titles-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-job-titles-list.html',
  styleUrl: './admin-job-titles-list.css'
})
export class AdminJobTitlesList implements OnInit {
  jobTitles: IJobTitleViewModel[] = [];
  searchTerm: string = '';
  searchBy: 'global' | 'inactiveStatus' | 'activeStatus' = 'global';
  sortDescending: boolean = false;
  
  currentPage: number = 1;
  itemsPerPage: number = 8;
  totalJobTitles: number = 0;
  totalPages: number = 0;
  
  isLoading: boolean = false;

  showEditModal = false;
  jobTitleToEdit: IJobTitleViewModel | null = null;
  
  editForm = {
    id: 0,
    name: '',
  };
  
  isSubmitting = false;
  errorMessage = '';

  constructor(
    private jobTitleService: JobTitlesService,
    private departmentService: DepartmentService) {

  }

  ngOnInit(): void {
    this.loadJobTitles();
  }

 loadJobTitles(): void {
  this.isLoading = true;
  this.errorMessage = '';

  const filterRequest: IFilteredJobTitlesRequest = {
    jobTitleName: this.searchTerm ? this.searchTerm : undefined,
    activityStatus: this.searchBy === 'activeStatus' ? JobTitleActivityStatus.ACTIVE 
      : this.searchBy === 'inactiveStatus' ? JobTitleActivityStatus.INACTIVE : JobTitleActivityStatus.ALL,
    params: {
      sortBy: 'name',
      sortDescending: this.sortDescending,
      page: this.currentPage,
      pageSize: this.itemsPerPage
    }
  };

  this.jobTitleService.getAllJobTitlesFiltered(filterRequest).subscribe({
      next: (response: IApiResponse<IFilteredApiResponse<IJobTitle>>) => {
        console.log('API response:', response);
        
        if (response.success) {
          const rawJobTitles: IJobTitle[] = response.data.data || [];
          this.jobTitles = rawJobTitles.map(jobTitle => this.mapToJobTitleViewModel(jobTitle));
          this.totalPages = response.data.totalPages;
        } else {

          this.errorMessage = response.message || 'Error loading the job titles.';
          this.jobTitles = [];
          this.totalPages = 0;
        }
        
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to fetch job titles:', err);
        this.isLoading = false;
        
        if (err.status >= 400 && err.status < 500) {
          this.errorMessage = err.error?.message || 'The request could not be handled properly.';
        } else if (err.status >= 500) {
          this.errorMessage = 'Server error, try again later.';
        } else {
          this.errorMessage = 'Unexpected error happened';
        }
        
        this.jobTitles = [];
        this.totalPages = 0;
        this.currentPage = 1;
      }
    });
  }

  private mapToJobTitleViewModel(jobTitle: IJobTitle): IJobTitleViewModel {
    return {
      id: jobTitle.id,
      name: jobTitle.name,
      employeeCount: jobTitle.employeeCount || 0,
      isActive: jobTitle.deletedAt === null,
    };
  }

  onSearch(): void {
    this.currentPage = 1; 
    this.loadJobTitles();
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.currentPage = 1;
    this.loadJobTitles();
  }

  toggleSortOrder(): void {
    this.sortDescending = !this.sortDescending;
    this.currentPage = 1;
    this.loadJobTitles();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page;
      this.loadJobTitles();
    }
  }

  goToFirstPage(): void {
    this.goToPage(1);
  }

  goToLastPage(): void {
    this.goToPage(this.totalPages);
  }

  goToPreviousPage(): void {
    if (this.currentPage > 1) {
      this.goToPage(this.currentPage - 1);
    }
  }

  goToNextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.goToPage(this.currentPage + 1);
    }
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisiblePages = 5;
    
    if (this.totalPages <= maxVisiblePages) {
      for (let i = 1; i <= this.totalPages; i++) {
        pages.push(i);
      }
    } else {
      let startPage = Math.max(1, this.currentPage - 2);
      let endPage = Math.min(this.totalPages, this.currentPage + 2);
      
      if (this.currentPage <= 3) {
        endPage = Math.min(this.totalPages, 5);
      }
      if (this.currentPage >= this.totalPages - 2) {
        startPage = Math.max(1, this.totalPages - 4);
      }
      
      for (let i = startPage; i <= endPage; i++) {
        pages.push(i);
      }
    }
    
    return pages;
  }

  deleteJobTitle(jobTitle: IJobTitleViewModel): void {
  const confirmMessage = `Are you sure you want to delete the job title "${jobTitle.name}"?`;
  
  if (confirm(confirmMessage)) {
      this.isLoading = true;
      
      this.jobTitleService.deleteJobTitle(jobTitle.id).subscribe({
        next: (response) => {
          this.isLoading = false;
          if (response.success) {
            console.log('Job title deleted successfully');
            this.loadJobTitles(); 
          } else {
            alert('Failed to delete job title: ' + (response.message || 'Unknown error'));
          }
        },
        error: (error) => {
          this.isLoading = false;
          console.error('Error deleting job title:', error);
          alert('Error deleting job title: ' + (error.error?.message || error.message));
        }
      });
    }
  }

  editJobTitle(jobTitle: IJobTitleViewModel): void {
    this.jobTitleToEdit = { ...jobTitle };
    this.populateEditForm(jobTitle);
    this.showEditModal = true;
  }

  populateEditForm(jobTitle: IJobTitleViewModel): void {
    this.editForm = {
      id: jobTitle.id,
      name: jobTitle.name || '',
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

    const jobTitleToUpdate: IJobTitle = {
      id: this.editForm.id,
      name: this.editForm.name,
      employeeCount: 0
    };

    this.jobTitleService.updateJobTitle(jobTitleToUpdate).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        if (response.success) {
          this.showEditModal = false;
          this.loadJobTitles();
          console.log('Job title updated successfully:', response.data);
        } else {
          this.errorMessage = response.message || 'Failed to update job title';
        }
      },
      error: (error) => {
        this.isSubmitting = false;
        this.errorMessage = 'Error updating job title: ' + (error.error?.message || error.message);
        console.error('Error updating job title:', error);
      }
    });
  }
  
  onSearchKeypress(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.onSearch();
    }
  }

  isFormValid(): boolean {
    return !!(this.editForm.name.trim());
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.jobTitleToEdit = null;
    this.errorMessage = '';
  }

  trackByJobTitleId(index: number, item: IJobTitleViewModel): number {
    return item.id;
  }

  restoreJobTitle(id: number) {
    this.jobTitleService.restoreJobTitle(id).subscribe({
      next: (response: IApiResponse<IJobTitle>) => {
        if(response.success){
          this.loadJobTitles();
        }
      },
      error: (err: HttpErrorResponse) => {
        console.log(`Restore failed: ${err.error.message}`);
        alert('Failed to restore job title. Please try again.');
      }
    })
  }
}