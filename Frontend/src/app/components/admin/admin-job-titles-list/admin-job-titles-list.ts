import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IJobTitle } from '../../../models/entities/ijob-title';
import { JobTitlesService } from '../../../services/jobTitles/job-titles-service';
import { JobTitleViewModel } from '../../../view-models/job-title-view-model';
import { IFilteredJobTitlesRequest } from '../../../models/requests/ifiltered-job-titles-request';
import { IFilteredApiResponse } from '../../../models/responses/ifiltered-api-response';
import { IApiResponse } from '../../../models/responses/iapi-response';

@Component({
  selector: 'app-admin-job-titles-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-job-titles-list.html',
  styleUrl: './admin-job-titles-list.css'
})
export class AdminJobTitlesList implements OnInit {
  jobTitles: JobTitleViewModel[] = [];
  searchTerm: string = '';
  searchBy: string = 'jobTitle'; 
  sortDescending: boolean = false;
  
  currentPage: number = 1;
  itemsPerPage: number = 5;
  totalJobTitles: number = 0;
  totalPages: number = 0;
  
  // Loading state
  isLoading: boolean = false;

  constructor(private jobTitleService: JobTitlesService) { }

  ngOnInit(): void {
    this.loadJobTitles();
  }

  loadJobTitles(): void {
    this.isLoading = true;
    
    const filterRequest: IFilteredJobTitlesRequest = {
      jobTitleName: this.searchBy === 'jobTitle' ? this.searchTerm : undefined,
      departmentName: this.searchBy === 'department' ? this.searchTerm : undefined,
      params: {
        sortBy: this.getSortField(),
        sortDescending: this.sortDescending,
        page: this.currentPage,
        pageSize: this.itemsPerPage
      }
    };

    this.jobTitleService.getAllJobTitlesFiltered(filterRequest).subscribe({
      next: (response: IApiResponse<IFilteredApiResponse<IJobTitle>>) => {
        console.log('API response:', response);
        const rawJobTitles: IJobTitle[] = response.data.data || [];
        this.jobTitles = rawJobTitles.map(jobTitle => this.mapToJobTitleViewModel(jobTitle));
        this.totalPages = response.data.totalPages;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to fetch job titles:', err);
        this.isLoading = false;
      }
    });
  }

  private getSortField(): string {
    switch (this.searchBy) {
      case 'jobTitle':
        return 'name';
      case 'department':
        return 'departmentName';
      default:
        return 'name';
    }
  }

  private mapToJobTitleViewModel(jobTitle: IJobTitle): JobTitleViewModel {
    return {
      id: jobTitle.id,
      name: jobTitle.name,
      department: {
        id: jobTitle.departmentId,
        name: jobTitle.departmentName || "Unknown"
      },
      employeeCount: jobTitle.employeeCount || 0,
    };
  }

  getSearchPlaceholder(): string {
    switch (this.searchBy) {
      case 'jobTitle':
        return 'Search by job title...';
      case 'department':
        return 'Search by department...';
      default:
        return 'Search job titles...';
    }
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

  deleteJobTitle(jobTitle: JobTitleViewModel) {

  }

  editJobTitle(jobTitle: JobTitleViewModel) {

  }

  trackByJobTitleId(index: number, item: JobTitleViewModel): number {
    return item.id;
  }
}