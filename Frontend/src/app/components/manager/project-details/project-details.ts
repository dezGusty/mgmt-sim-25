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
import { IFilteredUsersRequest } from '../../../models/requests/ifiltered-users-request';
import { IFilteredApiResponse } from '../../../models/responses/ifiltered-api-response';
import { IPendingAssignment } from '../../../models/entities/ipending-assignment';
import { IPendingRemoval } from '../../../models/entities/ipending-removal';
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
  
  // Search and filter
  searchTerm: string = '';
  private searchTermSubject = new BehaviorSubject<string>('');
  debouncedSearchTerm$ = this.searchTermSubject.pipe(
    debounceTime(300),
    distinctUntilChanged()
  );
  private searchSubscription?: Subscription;

  // Available users pagination and management
  availableUsersData: IFilteredApiResponse<IUser> | null = null;
  availableUsersCurrentPage: number = 1;
  availableUsersPageSize: number = 10;
  availableUsersSearch: string = '';
  isLoadingAvailableUsers: boolean = false;
  private availableUsersSearchSubject = new BehaviorSubject<string>('');
  debouncedAvailableUsersSearch$ = this.availableUsersSearchSubject.pipe(
    debounceTime(300),
    distinctUntilChanged()
  );
  private availableUsersSearchSubscription?: Subscription;

  // Assignment management
  pendingAssignments: IPendingAssignment[] = [];
  pendingRemovals: IPendingRemoval[] = [];
  selectedAllocation: { [userId: number]: number } = {};

  // Math reference for template
  Math = Math;
  
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

  get canAssign(): boolean {
    // Can assign if user can modify AND project is active
    // Use the current edit state if in edit mode, otherwise use the original project state
    const isProjectActive = this.activeTab === 'edit' ? this.editIsActive : (this.project?.isActive ?? true);
    return this.canModify && isProjectActive;
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
    this.setupAvailableUsersSearchSubscription();
  }

  ngOnDestroy() {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
    if (this.availableUsersSearchSubscription) {
      this.availableUsersSearchSubscription.unsubscribe();
    }
  }

  private setupSearchSubscription() {
    this.searchSubscription = this.debouncedSearchTerm$.subscribe(term => {
      // The filtering is handled by the getter, so we don't need to do anything here
      // This is just to trigger change detection
    });
  }

  private setupAvailableUsersSearchSubscription() {
    this.availableUsersSearchSubscription = this.debouncedAvailableUsersSearch$.subscribe(term => {
      this.availableUsersCurrentPage = 1;
      this.loadAvailableUsers();
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
        employmentType: userProject.employmentType ?? 0, // Default to FullTime (0) if undefined
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
    const fullTimeHours = 40;
    return (percentage / 100) * fullTimeHours;
  }

  calculateTotalAssignedFTEs(): number {
    if (!this.projectUsers || this.projectUsers.length === 0) {
      return 0;
    }
    
    let totalFTEs = 0;
    
    this.projectUsers.forEach(projectUser => {
      if (projectUser.user && projectUser.timePercentagePerProject) {
        const userTotalAvailability = this.calculateUserTotalAvailability(projectUser.user.employmentType);
        
        const projectFTEAllocation = (projectUser.timePercentagePerProject / 100) * userTotalAvailability;
        totalFTEs += projectFTEAllocation;
      }
    });
    
    return parseFloat(totalFTEs.toFixed(2));
  }

  private calculateUserTotalAvailability(employmentType?: number): number {
    // FullTime (0) = 1.0 FTE, PartTime (1) = 0.5 FTE
    if (employmentType === 1) {
      return 0.5;
    }
    return 1.0; // Default to full-time
  }

  calculateRemainingFTEs(): number {
    if (!this.project) {
      return 0;
    }
    
    const totalAssigned = this.calculateTotalAssignedFTEs();
    const remaining = this.project.budgetedFTEs - totalAssigned;
    return Math.max(0, remaining);
  }

  setViewMode(mode: 'cards' | 'table') {
    this.viewMode = mode;
  }

  setActiveTab(tab: 'view' | 'edit' | 'assign') {
    // Prevent switching to assign tab if project would be inactive
    if (tab === 'assign' && !this.canAssign) {
      return;
    }
    
    this.activeTab = tab;
    
    if (tab !== 'edit') {
      this.closeEditForm();
    }
    if (tab !== 'assign') {
      this.closeAssignForm();
    }
    
    if (tab === 'edit') {
      this.initializeProjectEditForm();
    }
    
    if (tab === 'assign') {
      this.resetAssignmentState();
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
          this.loadProjectDetails();
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
    if (!this.projectId) return;
    
    this.isLoadingAvailableUsers = true;
    
    this.projectService.getAvailableUsersForProject(
      this.projectId,
      this.availableUsersCurrentPage,
      this.availableUsersPageSize,
      this.availableUsersSearch
    ).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const filteredUsers = response.data.data.filter((user: IUser) => 
            !this.isPendingAssignment(user.id) &&
            (user.remainingAvailability || 0) > 0
          );
          
          this.availableUsersData = {
            ...response.data,
            data: filteredUsers
          };
        } else {
          this.loadAllUsersForAssignmentFallback();
        }
        this.isLoadingAvailableUsers = false;
      },
      error: (error: any) => {
        console.error('Error loading available users from project endpoint, falling back:', error);
        this.loadAllUsersForAssignmentFallback();
      }
    });
  }

  private loadAllUsersForAssignmentFallback() {
    const request: IFilteredUsersRequest = {
      globalSearch: this.availableUsersSearch || undefined,
      params: {
        page: this.availableUsersCurrentPage,
        pageSize: this.availableUsersPageSize,
        sortBy: 'firstName',
        sortDescending: false
      }
    };
    
    this.usersService.getUnassignedUsers(request).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const assignedUserIds = this.projectUsers.map(pu => pu.userId);
          const filteredUsers = response.data.data.filter((user: IUser) => 
            !assignedUserIds.includes(user.id) && 
            !this.isPendingAssignment(user.id) &&
            (user.remainingAvailability || 0) > 0
          );
          
          this.availableUsersData = {
            ...response.data,
            data: filteredUsers
          };
        } else {
          this.loadAllUsersForAssignmentFinal(request);
        }
        this.isLoadingAvailableUsers = false;
      },
      error: (error: any) => {
        console.error('Error loading unassigned users, final fallback to all users:', error);
        this.loadAllUsersForAssignmentFinal(request);
      }
    });
  }

  private loadAllUsersForAssignmentFinal(request: IFilteredUsersRequest) {
    this.usersService.getUsersIncludeRelationshipsFiltered(request).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const assignedUserIds = this.projectUsers.map(pu => pu.userId);
          const filteredUsers = response.data.data.filter((user: IUser) => 
            !assignedUserIds.includes(user.id) && 
            !this.isPendingAssignment(user.id) &&
            (user.remainingAvailability || 0) > 0
          );
          
          this.availableUsersData = {
            ...response.data,
            data: filteredUsers
          };
        }
        this.isLoadingAvailableUsers = false;
      },
      error: (error: any) => {
        console.error('Error loading all users for assignment:', error);
        this.isLoadingAvailableUsers = false;
      }
    });
  }

  assignUser() {
    if (!this.canModify || !this.assignUserId || !this.projectId) return;
    
    this.projectService.assignUserToProject(this.projectId, this.assignUserId, this.assignPercentage).subscribe({
      next: (response) => {
        if (response.success) {
          this.loadProjectDetails();
          this.closeAssignForm();
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

  getEmploymentTypeDisplay(employmentType?: number): string {
    if (employmentType === undefined) return 'N/A';
    return employmentType === 0 ? 'Full-time' : 'Part-time';
  }

  calculateHoursPerWeek(timePercentage?: number, employmentType?: number): string {
    if (!timePercentage || employmentType === undefined) return 'N/A';

    const hoursPerDay = employmentType === 0 ? 8 : 4;
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
    
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    
    return `${year}-${month}-${day}`; // Returns YYYY-MM-DD format in local time
  }

  resetProjectEditForm() {
    this.initializeProjectEditForm();
  }

  onProjectActiveStatusChange() {
    // If project becomes inactive and user is currently on assign tab,
    // switch them to view tab to avoid confusion
    if (!this.editIsActive && this.activeTab === 'assign') {
      this.setActiveTab('view');
    }
  }

  isProjectFormValid(): boolean {
    if (!(this.editProjectName.trim() && 
          this.editStartDate && 
          this.editEndDate && 
          this.editBudgetedFTEs >= 0 &&
          new Date(this.editStartDate) <= new Date(this.editEndDate))) {
      return false;
    }
    
    // Check if budgeted FTEs is below current assigned FTEs
    if (this.isBudgetedFTEsBelowAssigned()) {
      return false;
    }
    
    // For existing projects, allow editing even if they started in the past
    // Only validate that start date is not unreasonably far in the past (e.g., more than 10 years)
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const startDate = new Date(this.editStartDate);
    startDate.setHours(0, 0, 0, 0);
    
    // Allow start dates up to 10 years in the past
    const tenYearsAgo = new Date(today);
    tenYearsAgo.setFullYear(tenYearsAgo.getFullYear() - 10);
    
    return startDate >= tenYearsAgo;
  }

  isBudgetedFTEsBelowAssigned(): boolean {
    if (!this.project) return false;
    const currentAssignedFTEs = this.getCurrentAssignedFTEs();
    return this.editBudgetedFTEs < currentAssignedFTEs;
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
          this.loadProjectDetails();
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

  onStartDateChange() {
    // If end date is before new start date, update it
    if (this.editEndDate && this.editStartDate && new Date(this.editEndDate) < new Date(this.editStartDate)) {
      const startDate = new Date(this.editStartDate);
      this.editEndDate = this.formatDateForInput(startDate);
    }
  }

  getTodayDateString(): string {
    const today = new Date();
    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, '0');
    const day = String(today.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  getMinEndDate(): string {
    if (this.editStartDate) {
      return this.editStartDate;
    }
    return this.getTodayDateString();
  }

  // Reset assignment state
  resetAssignmentState() {
    this.pendingAssignments = [];
    this.pendingRemovals = [];
    this.selectedAllocation = {};
  }

  // Check if there are pending changes
  hasPendingChanges(): boolean {
    return this.pendingAssignments.length > 0 || this.pendingRemovals.length > 0;
  }

  // Available users search
  onAvailableUsersSearchChange() {
    this.availableUsersSearch = this.availableUsersSearch || '';
    this.availableUsersSearchSubject.next(this.availableUsersSearch);
  }

  // Pagination methods
  previousAvailablePage() {
    if (this.availableUsersCurrentPage > 1) {
      this.availableUsersCurrentPage--;
      this.loadAvailableUsers();
    }
  }

  nextAvailablePage() {
    if (this.availableUsersCurrentPage < (this.availableUsersData?.totalPages || 1)) {
      this.availableUsersCurrentPage++;
      this.loadAvailableUsers();
    }
  }

  // Capacity calculation methods
  getCurrentAssignedFTEs(): number {
    return this.calculateTotalAssignedFTEs();
  }

  getPreviewAssignedFTEs(): number {
    const currentFTEs = this.getCurrentAssignedFTEs();
    
    // Calculate FTEs for pending additions (considering employment type)
    const pendingAdditions = this.pendingAssignments.reduce((sum, assignment) => {
      const userTotalAvailability = this.calculateUserTotalAvailability(assignment.user.employmentType);
      const assignmentFTEs = (assignment.percentage / 100) * userTotalAvailability;
      return sum + assignmentFTEs;
    }, 0);
    
    // Calculate FTEs for pending removals (considering employment type)
    const pendingRemovals = this.pendingRemovals.reduce((sum, removal) => {
      if (removal.user) {
        const userTotalAvailability = this.calculateUserTotalAvailability(removal.user.employmentType);
        const removalFTEs = (removal.originalPercentage / 100) * userTotalAvailability;
        return sum + removalFTEs;
      }
      return sum;
    }, 0);
    
    return parseFloat((currentFTEs + pendingAdditions - pendingRemovals).toFixed(2));
  }

  getPreviewRemainingFTEs(): number {
    if (!this.project) return 0;
    const remaining = this.project.budgetedFTEs - this.getPreviewAssignedFTEs();
    return parseFloat(remaining.toFixed(2));
  }

  getCapacityUsagePercentage(): number {
    if (!this.project || this.project.budgetedFTEs === 0) return 0;
    return parseFloat(((this.getPreviewAssignedFTEs() / this.project.budgetedFTEs) * 100).toFixed(1));
  }

  isCapacityExceeded(): boolean {
    return this.getPreviewRemainingFTEs() < 0;
  }

  wouldExceedCapacity(user: IUser, percentage: number): boolean {
    if (!this.project) return false;
    
    // Calculate what the FTEs would be if we add this assignment
    const userTotalAvailability = this.calculateUserTotalAvailability(user.employmentType);
    const proposedFTEs = (percentage / 100) * userTotalAvailability;
    const currentAssignedFTEs = this.getPreviewAssignedFTEs();
    const projectedTotalFTEs = currentAssignedFTEs + proposedFTEs;
    
    return projectedTotalFTEs > this.project.budgetedFTEs;
  }

  // Assignment preview methods
  getAssignmentPreviewCount(): number {
    const currentCount = this.projectUsers.length;
    const pendingAdditions = this.pendingAssignments.length;
    const pendingRemovals = this.pendingRemovals.length;
    return currentCount + pendingAdditions - pendingRemovals;
  }

  // User allocation methods
  getAvailablePercentagesForUser(user: IUser): number[] {
    const maxAvailable = Math.floor((user.remainingAvailability || 0) * 100);
    const percentages: number[] = [];
    
    // Generate percentages in 25% increments: 25, 50, 75, 100
    for (let i = 25; i <= maxAvailable && i <= 100; i += 25) {
      if (!this.wouldExceedCapacity(user, i)) {
        percentages.push(i);
      }
    }
    
    return percentages;
  }

  // Pending assignment checks
  isPendingAssignment(userId: number): boolean {
    return this.pendingAssignments.some(assignment => assignment.user.id === userId);
  }

  isPendingRemoval(userId: number): boolean {
    return this.pendingRemovals.some(removal => removal.userId === userId);
  }

  // Assignment management
  addToPendingAssignments(user: IUser, percentage: number) {
    if (!percentage || this.isPendingAssignment(user.id)) return;
    
    // Check if this assignment would exceed the project capacity
    if (this.wouldExceedCapacity(user, percentage)) {
      const remainingFTEs = this.getPreviewRemainingFTEs();
      alert(`Cannot assign ${user.firstName} ${user.lastName} at ${percentage}%. This would exceed the project capacity by ${Math.abs(remainingFTEs).toFixed(2)} FTEs. Available capacity: ${Math.max(0, remainingFTEs).toFixed(2)} FTEs.`);
      return;
    }
    
    this.pendingAssignments.push({
      user: user,
      percentage: percentage
    });
    
    // Clear the selection
    this.selectedAllocation[user.id] = 0;
    
    // Reload available users to update the list
    this.loadAvailableUsers();
  }

  removeFromPendingAssignments(userId: number) {
    this.pendingAssignments = this.pendingAssignments.filter(assignment => assignment.user.id !== userId);
    // Reload available users to update the list
    this.loadAvailableUsers();
  }

  addToPendingRemovals(projectUser: IProjectUser) {
    if (!projectUser.user || this.isPendingRemoval(projectUser.userId)) return;
    
    this.pendingRemovals.push({
      userId: projectUser.userId,
      user: projectUser.user,
      originalPercentage: projectUser.timePercentagePerProject || 0
    });
  }

  removeFromPendingRemovals(userId: number) {
    this.pendingRemovals = this.pendingRemovals.filter(removal => removal.userId !== userId);
  }

  // Save and cancel operations
  async saveAssignmentChanges() {
    if (!this.canModify || !this.projectId || !this.hasPendingChanges()) return;
    
    try {
      // Process removals first
      for (const removal of this.pendingRemovals) {
        await this.projectService.removeUserFromProject(this.projectId, removal.userId).toPromise();
      }
      
      // Process new assignments
      for (const assignment of this.pendingAssignments) {
        await this.projectService.assignUserToProject(
          this.projectId, 
          assignment.user.id, 
          assignment.percentage
        ).toPromise();
      }
      
      // Reload project details and reset state
      this.loadProjectDetails();
      this.resetAssignmentState();
      this.loadAvailableUsers();
      
    } catch (error: any) {
      this.errorMessage = 'Error saving assignment changes';
      console.error('Error saving assignment changes:', error);
    }
  }

  cancelAssignmentChanges() {
    this.resetAssignmentState();
    this.loadAvailableUsers();
  }
}
