import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { CustomNavbar } from '../../shared/custom-navbar/custom-navbar';
import { ProjectService } from '../../../services/projects/project.service';
import { UsersService } from '../../../services/users/users-service';
import { IProjectWithUsers, IUserProject } from '../../../models/entities/iproject';
import { IUser } from '../../../models/entities/iuser';
import { IProjectUser } from '../../../models/entities/iproject-user';
import { Subject, BehaviorSubject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { Auth } from '../../../services/authService/auth';

@Component({
  selector: 'app-project-details',
  imports: [
    CommonModule,
    FormsModule,
    CustomNavbar,
  ],
  templateUrl: './project-details.html',
  styleUrl: './project-details.css',
})
export class ProjectDetails implements OnInit, OnDestroy {
  projectId: number | null = null;
  project: IProjectWithUsers | null = null;
  projectUsers: IProjectUser[] = [];
  errorMessage: string | null = null;
  isLoading: boolean = false;
  
  // View modes
  viewMode: 'cards' | 'table' = 'table';
  activeTab: 'view' | 'edit' | 'assign' = 'view';
  
  // Edit state
  showEditForm = false;
  editingUserId: number | null = null;
  editPercentage: number = 0;
  
  // Project edit state
  editProjectName: string = '';
  editStartDate: string = '';
  editEndDate: string = '';
  editBudgetedFTEs: number = 0;
  editIsActive: boolean = true;
  
  // Assign state
  showAssignForm = false;
  assignUserId: number | null = null;
  assignPercentage: number = 0;
  availableUsers: IUser[] = [];
  availablePercentages = [
    { value: 50, label: '50%' },
    { value: 100, label: '100%' }
  ];
  
  // Search and filter
  searchTerm: string = '';
  private searchTermSubject = new BehaviorSubject<string>('');
  debouncedSearchTerm$ = this.searchTermSubject.pipe(
    debounceTime(300),
    distinctUntilChanged()
  );
  private searchSubscription?: Subscription;
  
  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private projectService: ProjectService,
    private usersService: UsersService,
    private authService: Auth
  ) {}

  get isViewOnly(): boolean {
    return this.authService.isTemporarilyReplaced();
  }

  get canModify(): boolean {
    return !this.isViewOnly;
  }

  get filteredProjectUsers(): IProjectUser[] {
    if (!this.searchTerm) {
      return this.projectUsers;
    }
    
    const term = this.searchTerm.toLowerCase();
    return this.projectUsers.filter(projectUser => {
      const user = projectUser.user;
      if (!user) return false;
      
      const fullName = `${user.firstName} ${user.lastName}`.toLowerCase();
      const jobTitle = user.jobTitleName?.toLowerCase() || '';
      
      return fullName.includes(term) || jobTitle.includes(term);
    });
  }

  ngOnInit() {
    console.log('=== Component ngOnInit ===');
    
    // Reset state
    this.project = null;
    this.projectUsers = [];
    this.errorMessage = null;
    this.isLoading = false;
    
    this.route.params.subscribe(params => {
      console.log('Route params changed:', params);
      this.projectId = +params['id'];
      if (this.projectId) {
        console.log('Loading project with ID:', this.projectId);
        this.loadProjectDetails();
      } else {
        this.errorMessage = 'No project ID provided in route';
      }
    });
    
    this.setupSearchSubscription();
  }

  ngOnDestroy() {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
  }

  private setupSearchSubscription() {
    this.searchSubscription = this.debouncedSearchTerm$.subscribe(term => {
      // The filtering is handled by the getter, so we don't need to do anything here
      // This is just to trigger change detection
    });
  }

  onSearchTermChange(term: string) {
    this.searchTerm = term;
    this.searchTermSubject.next(term);
  }

  loadProjectDetails() {
    if (!this.projectId) return;
    
    this.isLoading = true;
    this.errorMessage = null;
    
    console.log('=== Loading project details ===');
    console.log('Project ID:', this.projectId);
    
    // Use the with-users endpoint to get project and users in one call
    this.projectService.getProjectWithUsers(this.projectId).subscribe({
      next: (response) => {
        console.log('=== API Response received ===');
        console.log('Response success:', response.success);
        console.log('Full response:', JSON.stringify(response, null, 2));
        
        this.isLoading = false;
        if (response.success && response.data) {
          console.log('Project data from API:', JSON.stringify(response.data, null, 2));
          this.project = response.data;
          this.processProjectUsers();
        } else {
          console.error('API returned unsuccessful response:', response);
          this.errorMessage = response.message || 'Failed to load project details';
        }
      },
      error: (error) => {
        console.error('=== API Error ===');
        console.error('Error loading project:', error);
        this.isLoading = false;
        this.errorMessage = 'Error loading project details';
      }
    });
  }

  private processProjectUsers() {
    if (!this.project?.assignedUsers) {
      this.projectUsers = [];
      return;
    }

    console.log('=== Processing project users ===');
    console.log('Raw assignedUsers from API:', JSON.stringify(this.project.assignedUsers, null, 2));

    // Map project users and calculate time allocation
    this.projectUsers = this.project.assignedUsers.map((userProject, index) => {
      console.log(`--- Processing user ${index + 1} ---`);
      console.log('Raw user project data:', JSON.stringify(userProject, null, 2));
      console.log('TimePercentagePerProject:', userProject.timePercentagePerProject);
      console.log('Job title name:', userProject.jobTitleName);
      console.log('Employment type:', userProject.employmentType);
      console.log('User name:', userProject.userName);
      
      // Check for missing or null values
      if (userProject.timePercentagePerProject === null || userProject.timePercentagePerProject === undefined) {
        console.warn(`WARNING: User ${userProject.userName} (ID: ${userProject.userId}) has null/undefined timePercentagePerProject`);
      }
      
      const timeAllocatedPerProject = this.calculateTimeAllocation(userProject.timePercentagePerProject || 0);
      
      // Map the flat user data to the expected nested structure
      const mappedUser: IUser = {
        id: userProject.userId,
        firstName: userProject.userName?.split(' ')[0] || 'Unknown',
        lastName: userProject.userName?.split(' ').slice(1).join(' ') || 'User',
        email: userProject.userEmail || '',
        jobTitleId: userProject.jobTitleId || 0,
        jobTitleName: userProject.jobTitleName || 'N/A',
        employmentType: userProject.employmentType || 'FullTime',
        roles: []
      };

      const result = {
        ...userProject,
        user: mappedUser,
        timeAllocatedPerProject,
        assignedPercentage: userProject.timePercentagePerProject, // Map for backward compatibility
        timePercentagePerProject: userProject.timePercentagePerProject
      } as IProjectUser;

      console.log(`Mapped result for user ${index + 1}:`, JSON.stringify(result, null, 2));
      console.log('Final TimePercentagePerProject:', result.timePercentagePerProject);
      console.log('Final assignedPercentage:', result.assignedPercentage);
      console.log('User job title:', result.user?.jobTitleName);
      console.log('User employment type:', result.user?.employmentType);
      
      return result;
    });

    console.log('=== Final processed project users ===');
    console.log('Total users processed:', this.projectUsers.length);
    this.projectUsers.forEach((user, index) => {
      console.log(`User ${index + 1}: ${user.user?.firstName} ${user.user?.lastName} - Allocation: ${user.timePercentagePerProject}%`);
    });
  }

  private calculateTimeAllocation(percentage: number): number {
    // Assuming full-time is 40 hours per week
    const fullTimeHours = 40;
    return (percentage / 100) * fullTimeHours;
  }

  calculateTotalAssignedFTEs(): number {
    if (!this.projectUsers || this.projectUsers.length === 0) {
      return 0;
    }
    
    const totalPercentage = this.projectUsers.reduce((sum, user) => {
      return sum + (user.timePercentagePerProject || 0);
    }, 0);
    
    // Convert percentage to FTE (100% = 1 FTE)
    return totalPercentage / 100;
  }

  calculateRemainingFTEs(): number {
    if (!this.project) {
      return 0;
    }
    
    const totalAssigned = this.calculateTotalAssignedFTEs();
    const remaining = this.project.budgetedFTEs - totalAssigned;
    return Math.max(0, remaining); // Don't show negative values
  }

  setViewMode(mode: 'cards' | 'table') {
    this.viewMode = mode;
  }

  setActiveTab(tab: 'view' | 'edit' | 'assign') {
    this.activeTab = tab;
    
    // Reset forms when switching tabs
    if (tab !== 'edit') {
      this.closeEditForm();
    }
    if (tab !== 'assign') {
      this.closeAssignForm();
    }
    
    // Initialize project edit form when switching to edit tab
    if (tab === 'edit') {
      this.initializeProjectEditForm();
    }
    
    // Load available users when switching to assign tab
    if (tab === 'assign' && this.availableUsers.length === 0) {
      this.loadAvailableUsers();
    }
  }

  // Edit functionality
  startEditUser(userProject: IProjectUser) {
    if (!this.canModify) return;
    
    this.editingUserId = userProject.userId;
    this.editPercentage = userProject.assignedPercentage;
    this.showEditForm = true;
  }

  closeEditForm() {
    this.showEditForm = false;
    this.editingUserId = null;
    this.editPercentage = 0;
  }

  saveUserEdit() {
    if (!this.canModify || !this.editingUserId || !this.projectId) return;
    
    this.projectService.updateUserAssignment(this.projectId, this.editingUserId, this.editPercentage).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadProjectDetails(); // This will reload both project and users
          this.closeEditForm();
        } else {
          this.errorMessage = response.message || 'Failed to update assignment';
        }
      },
      error: (error: any) => {
        this.errorMessage = 'Error updating assignment';
        console.error('Error updating assignment:', error);
      }
    });
  }

  // Assign functionality
  openAssignForm() {
    if (!this.canModify) return;
    
    this.showAssignForm = true;
    if (this.availableUsers.length === 0) {
      this.loadAvailableUsers();
    }
  }

  closeAssignForm() {
    this.showAssignForm = false;
    this.assignUserId = null;
    this.assignPercentage = 0;
  }

  private loadAvailableUsers() {
    this.usersService.getAllUsersIncludeRelationships().subscribe({
      next: (response: any) => {
        if (response.success && response.data) {
          // Filter out users that are already assigned to this project
          const assignedUserIds = this.projectUsers.map(pu => pu.userId);
          this.availableUsers = response.data.filter((user: IUser) => !assignedUserIds.includes(user.id));
        }
      },
      error: (error: any) => {
        console.error('Error loading available users:', error);
      }
    });
  }

  assignUser() {
    if (!this.canModify || !this.assignUserId || !this.projectId) return;
    
    this.projectService.assignUserToProject(this.projectId, this.assignUserId, this.assignPercentage).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadProjectDetails(); // This will reload both project and users
          this.closeAssignForm();
          // Refresh available users list
          this.loadAvailableUsers();
        } else {
          this.errorMessage = response.message || 'Failed to assign user';
        }
      },
      error: (error: any) => {
        this.errorMessage = 'Error assigning user';
        console.error('Error assigning user:', error);
      }
    });
  }

  // Utility methods
  formatDate(date: Date | string): string {
    if (!date) return '';
    const d = new Date(date);
    return d.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric' 
    });
  }

  formatPercentage(percentage: number): string {
    return `${percentage}%`;
  }

  formatTimeAllocation(hours: number): string {
    return `${hours}h/week`;
  }

  getUserFullName(user?: { firstName?: string; lastName?: string }): string {
    if (!user) return 'Unknown User';
    return `${user.firstName || ''} ${user.lastName || ''}`.trim() || 'Unknown User';
  }

  getUserInitials(user?: { firstName?: string; lastName?: string }): string {
    if (!user) return 'U';
    const firstName = user.firstName || '';
    const lastName = user.lastName || '';
    return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase() || 'U';
  }

  getEmploymentTypeDisplay(employmentType?: string): string {
    if (!employmentType) return 'N/A';
    return employmentType === 'FullTime' ? 'Full-time' : 'Part-time';
  }

  calculateHoursPerWeek(timePercentage?: number, employmentType?: string): string {
    if (!timePercentage || !employmentType) return 'N/A';
    
    const hoursPerDay = employmentType === 'FullTime' ? 8 : 4;
    const workingDaysPerWeek = 5;
    const totalHoursPerWeek = hoursPerDay * workingDaysPerWeek;
    const allocatedHours = (timePercentage / 100) * totalHoursPerWeek;
    
    return `${Math.round(allocatedHours)}h`;
  }

  // Project editing methods
  initializeProjectEditForm() {
    if (this.project) {
      this.editProjectName = this.project.name || '';
      this.editStartDate = this.formatDateForInput(this.project.startDate);
      this.editEndDate = this.formatDateForInput(this.project.endDate);
      this.editBudgetedFTEs = this.project.budgetedFTEs || 0;
      this.editIsActive = this.project.isActive ?? true;
    }
  }

  formatDateForInput(date: Date | string): string {
    if (!date) return '';
    const d = new Date(date);
    
    // Use local date formatting to avoid timezone issues
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    
    return `${year}-${month}-${day}`; // Returns YYYY-MM-DD format in local time
  }

  resetProjectEditForm() {
    this.initializeProjectEditForm();
  }

  isProjectFormValid(): boolean {
    return !!(this.editProjectName.trim() && 
              this.editStartDate && 
              this.editEndDate && 
              this.editBudgetedFTEs >= 0 &&
              new Date(this.editStartDate) <= new Date(this.editEndDate));
  }

  saveProjectDetails() {
    if (!this.canModify || !this.projectId || !this.isProjectFormValid()) return;

    const updateData = {
      name: this.editProjectName.trim(),
      startDate: new Date(this.editStartDate),
      endDate: new Date(this.editEndDate),
      budgetedFTEs: this.editBudgetedFTEs,
      isActive: this.editIsActive
    };

    console.log('Saving project details:', updateData);

    this.projectService.updateProject(this.projectId, updateData).subscribe({
      next: (response) => {
        if (response.success) {
          // Reload project details to reflect changes
          this.loadProjectDetails();
          // Optionally switch back to view tab
          this.setActiveTab('view');
        } else {
          this.errorMessage = response.message || 'Failed to update project details';
        }
      },
      error: (error: any) => {
        this.errorMessage = 'Error updating project details';
        console.error('Error updating project:', error);
      }
    });
  }

  goBack() {
    this.router.navigate(['/manager/projects']);
  }
}
