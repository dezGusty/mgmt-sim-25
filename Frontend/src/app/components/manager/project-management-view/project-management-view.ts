import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CustomNavbar } from '../../shared/custom-navbar/custom-navbar';
import { ProjectService } from '../../../services/projects/project.service';
import { UsersService } from '../../../services/users/users-service';
import { IProject } from '../../../models/entities/iproject';
import { IProjectWithUsers, IUserProject } from '../../../models/entities/iproject';
import { IFilteredProjectsRequest } from '../../../models/requests/ifiltered-projects-request';
import { Subject, BehaviorSubject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { Auth } from '../../../services/authService/auth';

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
  // Add/Edit form state
  newProject: Partial<IProject> = {
    name: '',
    startDate: new Date(),
    endDate: new Date(),
    budgetedFTEs: 0,
    isActive: true
  };
  editingProjectId: number | null = null;

  // Assign user state
  showAssignForm = false;
  assignUserId: number | null = null;
  assignPercentage: number = 0;
  availablePercentages = [
    { value: 50, label: '50%' },
    { value: 100, label: '100%' }
  ];
  assignmentWarning: string | null = null;
  projects: IProject[] = [];
  selectedProject: IProject | null = null;
  selectedProjectWithUsers: IProjectWithUsers | null = null;
  errorMessage: string | null = null;
  isLoading: boolean = false;
  showAddProjectForm = false;
  isSubmitting: boolean = false;
  private createRequestToken = 0;

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
    private projectService: ProjectService,
    private authService: Auth,
    private usersService: UsersService
  ) {}

  get isViewOnly(): boolean {
    return this.authService.isTemporarilyReplaced();
  }

  get canModify(): boolean {
    return !this.isViewOnly;
  }

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

    // Map frontend request into backend QueriedProjectRequestDto shape
    const backendRequest = {
      Name: request.searchTerm,
      IsActive: request.isActive,
      StartDateFrom: request.startDateFrom,
      StartDateTo: request.startDateTo,
      EndDateFrom: request.endDateFrom,
      EndDateTo: request.endDateTo,
      PagedQueryParams: {
        SortBy: request.sortBy || 'Id',
        SortDescending: request.sortDescending || false,
        Page: request.pageNumber || 1,
        PageSize: request.pageSize || this.itemsPerPage
      }
    };

    this.projectService.getFilteredProjects(backendRequest as any).subscribe({
      next: (response) => {
  console.debug('Filtered projects request', backendRequest);
  console.debug('Filtered projects response', response);

        if (response.success && response.data) {
          this.projects = response.data.data || [];
          this.totalPages = response.data.totalPages || 0;
          this.totalCount = response.data.totalCount || (this.totalPages * this.itemsPerPage);
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

  navigateToProjectDetails(project: IProject) {
    this.router.navigate(['/manager/projects', project.id]);
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
  this.resetNewProject();
  this.showAddProjectForm = true;
  }

  onProjectAdded() {
    this.showAddProjectForm = false;
    this.loadProjects();
  }

  resetNewProject() {
    const today = new Date();
    const tomorrow = new Date(today);
    tomorrow.setDate(today.getDate() + 1);
    
    this.newProject = {
      name: '',
      startDate: today,
      endDate: tomorrow,
      budgetedFTEs: 0,
      isActive: true
    };
    this.editingProjectId = null;
  }

  createProject() {
    this.errorMessage = null;
    this.isSubmitting = true;
    const token = ++this.createRequestToken;
    if (!this.newProject.name) {
      this.errorMessage = 'Project name is required';
      this.isSubmitting = false;
      return;
    }
    if (this.newProject.startDate! >= this.newProject.endDate!) {
      this.errorMessage = 'End date must be after start date';
      this.isSubmitting = false;
      return;
    }
    
    const today = new Date();
    today.setHours(0, 0, 0, 0); 
    const startDate = new Date(this.newProject.startDate!);
    startDate.setHours(0, 0, 0, 0);
    
    if (startDate < today) {
      this.errorMessage = 'Start date cannot be in the past';
      this.isSubmitting = false;
      return;
    }
    
    const projectToCreate = {
      ...this.newProject,
      isActive: true
    };
    
    this.projectService.createProject(projectToCreate).subscribe({
      next: (res) => {
        if (token !== this.createRequestToken) {

          return;
        }
        this.isSubmitting = false;
        if (res.success) {
          this.showAddProjectForm = false;
          this.currentPage = 1;
          this.loadProjects();
        } else {
          this.errorMessage = res.message || 'Failed to create project';
        }
      },
      error: (err) => {
        if (token !== this.createRequestToken) {
          return;
        }
        this.isSubmitting = false;
        console.error('Create project error', err);
        this.errorMessage = 'Failed to create project';
      }
    });
  }

  onCancelAddProject() {
    this.createRequestToken++;
    this.isSubmitting = false;
    this.showAddProjectForm = false;
  }

  startEditProject(project: IProject) {
    this.editingProjectId = project.id;
    this.newProject = { 
      ...project,
      startDate: project.startDate,
      endDate: project.endDate
    };
    this.showAddProjectForm = true;
  }

  saveProjectEdit() {
    console.log('saveProjectEdit called', { editingProjectId: this.editingProjectId, newProject: this.newProject });
    
    if (!this.editingProjectId) {
      console.log('No editingProjectId, returning');
      return;
    }
    
    this.errorMessage = null;
    
    // Add validation similar to createProject
    if (!this.newProject.name) {
      this.errorMessage = 'Project name is required';
      console.log('Validation error: Project name is required');
      return;
    }
    if (this.newProject.startDate! >= this.newProject.endDate!) {
      this.errorMessage = 'End date must be after start date';
      console.log('Validation error: End date must be after start date');
      return;
    }
    
    // Check if start date is in the past
    const today = new Date();
    today.setHours(0, 0, 0, 0); // Set to start of day for comparison
    const startDate = new Date(this.newProject.startDate!);
    startDate.setHours(0, 0, 0, 0);
    
    if (startDate < today) {
      this.errorMessage = 'Start date cannot be in the past';
      console.log('Validation error: Start date cannot be in the past');
      return;
    }
    
    console.log('Validation passed, calling update service');
    this.isSubmitting = true;
    
    const payload: Partial<IProject> = { ...this.newProject };
    this.projectService.updateProject(this.editingProjectId, payload).subscribe({
      next: (res) => {
        console.log('Update response:', res);
        this.isSubmitting = false;
        if (res.success) {
          this.errorMessage = null;
          this.showAddProjectForm = false;
          this.editingProjectId = null;
          
          if ((res as any).data) {
            const updated = (res as any).data as IProject;
            const idx = this.projects.findIndex(p => p.id === updated.id);
            if (idx >= 0) {
              this.projects[idx] = updated;
            } else {
              this.currentPage = 1;
              this.loadProjects();
            }
          } else {
            this.loadProjects();
          }
        } else {
          this.errorMessage = res.message || 'Failed to update project';
        }
      },
      error: (err) => {
        console.log('Update error:', err);
        this.isSubmitting = false;
        console.error('Update project error', err);
        this.errorMessage = 'Failed to update project';
      }
    });
  }


  openAssignForm(project: IProject) {
    this.selectedProject = project;
    this.showAssignForm = true;
    this.assignUserId = null;
    this.assignPercentage = 0;
    this.assignmentWarning = null;
    
    // Load full project details with user assignments
    this.projectService.getProjectById(project.id).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          // Store the full project details for validation
          this.selectedProjectWithUsers = response.data;
        }
      },
      error: (err) => {
        console.error('Error loading project details:', err);
      }
    });
  }

  assignUser() {
    if (!this.selectedProject || !this.assignUserId) {
      this.errorMessage = 'Select a project and an employee id';
      return;
    }
    if (this.assignPercentage < 0 || this.assignPercentage > 100) {
      this.errorMessage = 'Assigned percentage must be between 0 and 100';
      return;
    }

    // Check user availability before proceeding
    this.usersService.getUserById(this.assignUserId).subscribe({
      next: (userResponse) => {
        if (userResponse.success && userResponse.data) {
          const user = userResponse.data;
          // Calculate the actual FTE allocation based on user's total availability
          const actualFTEAllocation = (this.assignPercentage / 100) * (user.totalAvailability || 1);
          
          if (user.remainingAvailability !== undefined && actualFTEAllocation > user.remainingAvailability) {
            this.errorMessage = `Cannot assign employee: Assignment exceeds available capacity. Employee has ${(user.remainingAvailability * 100).toFixed(2)}% remaining availability.`;
            return;
          }

          this.proceedWithAssignment();
        } else {
          this.errorMessage = 'Failed to retrieve employee information';
        }
      },
      error: (err) => {
        console.error('Error checking employee availability:', err);
        this.errorMessage = 'Failed to verify employee availability';
      }
    });
  }

  private proceedWithAssignment() {
    if (!this.selectedProject || !this.assignUserId) return;

    this.projectService.assignUserToProject(this.selectedProject.id, this.assignUserId, this.assignPercentage).subscribe({
      next: (res) => {
        if (res.success) {
          this.errorMessage = null;
          this.assignmentWarning = null;
          this.closeAssignForm();
          this.loadProjects();
        } else {
          // Check if this is a duplicate assignment error (409 conflict)
          if (res.message && res.message.toLowerCase().includes('already assigned')) {
            this.assignmentWarning = 'Employee is already assigned to this project.';
            this.errorMessage = null;
          } else {
            this.errorMessage = res.message || 'Failed to assign employee';
            this.assignmentWarning = null;
          }
        }
      },
      error: (err) => {
        console.error('Assign employee error', err);
        
        // Handle 409 Conflict error (duplicate assignment)
        if (err.status === 409) {
          this.assignmentWarning = 'Employee is already assigned to this project.';
          this.errorMessage = null;
        } else {
          this.errorMessage = 'Failed to assign employee';
          this.assignmentWarning = null;
        }
      }
    });
  }

  closeAssignForm() {
    this.showAssignForm = false;
    this.selectedProject = null;
    this.selectedProjectWithUsers = null;
    this.assignUserId = null;
    this.assignPercentage = 0;
    this.assignmentWarning = null;
  }

  checkUserAvailability() {
    if (!this.assignUserId || this.assignPercentage <= 0) {
      this.assignmentWarning = null;
      return;
    }

    // Check if user is already assigned to this project
    if (this.selectedProjectWithUsers && this.selectedProjectWithUsers.assignedUsers) {
      const existingAssignment = this.selectedProjectWithUsers.assignedUsers.find((up: IUserProject) => up.userId === this.assignUserId);
      if (existingAssignment) {
        this.assignmentWarning = `Warning: Employee is already assigned to this project with ${existingAssignment.assignedPercentage}% allocation.`;
        return;
      }
    }

    this.usersService.getUserById(this.assignUserId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const user = response.data;
          // Calculate the actual FTE allocation based on user's total availability
          const actualFTEAllocation = (this.assignPercentage / 100) * (user.totalAvailability || 1);
          
          if (user.remainingAvailability !== undefined && actualFTEAllocation > user.remainingAvailability) {
            this.assignmentWarning = `Warning: Assignment exceeds available capacity. Employee has ${(user.remainingAvailability * 100).toFixed(2)}% remaining availability.`;
          } else {
            this.assignmentWarning = null;
          }
        }
      },
      error: (err) => {
        console.error('Error checking employee availability:', err);
        this.assignmentWarning = null;
      }
    });
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

  formatDateForInput(date: Date | undefined): string {
    if (!date) return '';
    const d = new Date(date);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  onStartDateChange(event: any) {
    this.newProject.startDate = new Date(event.target.value)
    if (this.newProject.endDate && this.newProject.startDate && this.newProject.endDate < this.newProject.startDate) {
      this.newProject.endDate = new Date(this.newProject.startDate);
    }
  }

  onEndDateChange(event: any) {
    this.newProject.endDate = new Date(event.target.value);
  }

  getTodayDateString(): string {
    const today = new Date();
    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, '0');
    const day = String(today.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  getMinEndDate(): string {
    if (this.newProject.startDate) {
      return this.formatDateForInput(this.newProject.startDate);
    }
    return this.getTodayDateString();
  }

  formatPercentage(value: number): string {
    return `${value.toFixed(2)}%`;
  }

  formatFTE(value: number): string {
    return `${value.toFixed(2)} FTE`;
  }

  getRemainingFTEColor(remainingFTEs: number): string {
    if (remainingFTEs <= 0) {
      return 'text-red-600 font-semibold'; // No capacity left - red
    } else if (remainingFTEs <= 0.5) {
      return 'text-yellow-600 font-medium'; // Low capacity - yellow
    } else {
      return 'text-green-600 font-medium'; // Good capacity - green
    }
  }
}