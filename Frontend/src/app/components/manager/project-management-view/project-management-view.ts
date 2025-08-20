import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CustomNavbar } from '../../shared/custom-navbar/custom-navbar';
import { ProjectService } from '../../../services/projects/project.service';
import { IProject } from '../../../models/entities/iproject';
import { IFilteredProjectsRequest } from '../../../models/requests/ifiltered-projects-request';
import { Subject, BehaviorSubject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-project-management-view',
  imports: [
    CommonModule,
    FormsModule,
    CustomNavbar,
  ],
  templateUrl: './project-management-view.html',
  styleUrl: './project-management-view.css',
})
export class ProjectManagementView implements OnInit, OnDestroy {
  projects: IProject[] = [];
  selectedProject: IProject | null = null;
  errorMessage: string | null = null;
  isLoading: boolean = false;
  showAddProjectForm = false;

  // Filter and search
  currentFilter: 'All' | 'Active' | 'Inactive' = 'All';
  searchTerm: string = '';
  searchCriteria: 'all' | 'name' | 'dateRange' = 'all';
  viewMode: 'card' | 'table' | 'calendar' = 'table';

  // Search debouncing
  private searchTermSubject = new BehaviorSubject<string>('');
  debouncedSearchTerm$ = this.searchTermSubject.pipe(
    debounceTime(300),
    distinctUntilChanged()
  );
  private searchSubscription?: Subscription;

  // Pagination
  currentPage: number = 1;
  itemsPerPage: number = 10;
  totalPages: number = 0;
  totalCount: number = 0;

  // Sorting
  sortColumn: string = 'name';
  sortDirection: 'asc' | 'desc' = 'asc';

  // Expose Math for template
  Math = Math;

  constructor(
    private router: Router,
    private projectService: ProjectService
  ) {}

  ngOnInit() {
    this.loadProjects();
    this.setupSearchSubscription();
  }

  ngOnDestroy() {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
    this.searchTermSubject.complete();
  }

  setupSearchSubscription() {
    this.searchSubscription = this.debouncedSearchTerm$.subscribe(term => {
      if (this.searchTerm !== term) {
        this.searchTerm = term;
        this.currentPage = 1;
        this.loadProjects();
      }
    });
  }

  loadProjects() {
    this.isLoading = true;
    this.errorMessage = null;

    const request: IFilteredProjectsRequest = {
      searchTerm: this.searchTerm || undefined,
      isActive: this.getFilterValue(),
      pageNumber: this.currentPage,
      pageSize: this.itemsPerPage,
      sortBy: this.sortColumn,
      sortDescending: this.sortDirection === 'desc'
    };

    this.projectService.getFilteredProjects(request).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.projects = response.data.data || [];
          this.totalPages = response.data.totalPages || 0;
          this.totalCount = this.totalPages * this.itemsPerPage; // Estimate based on pages
        } else {
          this.errorMessage = response.message || 'Failed to load projects';
          this.projects = [];
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading projects:', error);
        this.errorMessage = 'Failed to load projects. Please try again.';
        this.projects = [];
        this.isLoading = false;
      }
    });
  }

  private getFilterValue(): boolean | undefined {
    switch (this.currentFilter) {
      case 'Active':
        return true;
      case 'Inactive':
        return false;
      default:
        return undefined;
    }
  }

  getSearchPlaceholder(): string {
    switch (this.searchCriteria) {
      case 'name':
        return 'Search by project name...';
      case 'dateRange':
        return 'Search by date range...';
      default:
        return 'Search projects...';
    }
  }

  goBack() {
    this.router.navigate(['/manager']);
  }

  setFilter(filter: 'All' | 'Active' | 'Inactive') {
    this.currentFilter = filter;
    this.currentPage = 1;
    this.loadProjects();
  }

  setViewMode(mode: 'card' | 'table' | 'calendar') {
    this.viewMode = mode;
  }

  onSearchInput(event: any) {
    const value = event.target.value;
    this.searchTermSubject.next(value);
  }

  sort(column: string) {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
    this.loadProjects();
  }

  onPageChange(page: number) {
    this.currentPage = page;
    this.loadProjects();
  }

  selectProject(project: IProject) {
    this.selectedProject = project;
  }

  closeProjectDetail() {
    this.selectedProject = null;
  }

  addProject() {
    this.showAddProjectForm = true;
  }

  onProjectAdded() {
    this.showAddProjectForm = false;
    this.loadProjects();
  }

  getStatusColor(isActive: boolean): string {
    return isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800';
  }

  getStatusText(isActive: boolean): string {
    return isActive ? 'Active' : 'Inactive';
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString();
  }

  formatPercentage(value: number): string {
    return `${value.toFixed(1)}%`;
  }
}