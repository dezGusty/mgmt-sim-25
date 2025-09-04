import { Component } from '@angular/core';
import { Subject } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IUser } from '../../../../models/entities/iuser';
import { IUserViewModel } from '../../../../view-models/user-view-model';
import { UsersService } from '../../../../services/users/users-service';
import { IFilteredUsersRequest } from '../../../../models/requests/ifiltered-users-request';
import { AdminAssignRelationship } from '../admin-assign-relationship/admin-assign-relationship';
import { IApiResponse } from '../../../../models/responses/iapi-response';
import { IFilteredApiResponse } from '../../../../models/responses/ifiltered-api-response';
import { UserSearchType } from '../../../../models/enums/user-search-type';
import { HighlightPipe } from '../../../../pipes/highlight.pipe';
import { SecondManagerService } from '../../../../services/second-manager/second-manager.service';
import { ISecondManagerResponse, ISecondManagerViewModel } from '../../../../models/entities/isecond-manager';

@Component({
  selector: 'app-admin-user-relationships',
  imports: [CommonModule, FormsModule, AdminAssignRelationship, HighlightPipe],
  templateUrl: './admin-user-relationships.html',
  styleUrl: './admin-user-relationships.css',
})
export class AdminUserRelationships implements OnInit {
  private searchTermSubject = new Subject<string>();
  managersIds: Set<number> = new Set<number>();
  adminsIds: Set<number> = new Set<number>();
  managers: IUserViewModel[] = [];
  admins: IUserViewModel[] = [];
  unassignedUsers: IUserViewModel[] = [];

  // Second managers data
  activeSecondManagers: ISecondManagerViewModel[] = [];
  isLoadingSecondManagers: boolean = false;
  secondManagersErrorMessage: string = '';

  // Second manager modal data
  showSecondManagerModal: boolean = false;
  selectedSecondManagerEmployee: { id: number; name: string; email: string } | null = null;
  replacedManagerId: number = 0;
  isSubmittingSecondManager: boolean = false;
  secondManagerErrorMessage: string = '';
  secondManagerForm = {
    startDate: '',
    endDate: ''
  };

  // Edit second manager modal data
  showEditSecondManagerModal: boolean = false;
  selectedSecondManagerForEdit: ISecondManagerViewModel | null = null;
  isSubmittingEditSecondManager: boolean = false;
  editSecondManagerErrorMessage: string = '';
  editSecondManagerForm = {
    endDate: ''
  };

  // Toast success messaging
  showSuccessMessage: boolean = false;
  successMessage: string = '';
  private toastTimeoutRef: any = null;

  // Confirm Remove Second Manager modal state
  showConfirmRemoveSecondManager: boolean = false;
  pendingRemoveSecondManager: ISecondManagerViewModel | null = null;
  removeSecondManagerErrorMessage: string = '';

  searchTerm: string = '';
  searchBy: UserSearchType = UserSearchType.Global;
  sortDescending: boolean = false;

  currentSearchTerm: string = '';
  currentSearchBy: UserSearchType = UserSearchType.Global;
  currentHighlightTerm: string = '';

  UserSearchType = UserSearchType;

  allAdmins: IUserViewModel[] = [];   // full dataset   // current page slice
  originalAdmins: IUserViewModel[] = []; 
  totalAdminsCount: number = 0;
  totalPagesAdmins: number = 0;
  currentPageAdmins: number = 1;
  pageSizeAdmins: number = 3;
  pageSizeManagers: number = 5;
  currentPageManagers: number = 1;
  totalPagesManagers: number = 0;
  totalManagersCount: number = 0;

  pageSizeUnassignedUsers: number = 3;
  currentPageUnassignedUsers: number = 1;
  totalPagesUnassignedUsers: number = 0;
  totalUnassignedUsersCount: number = 0;

  showAssignRelationShipComponent: boolean = false;
  selectedEmployee!: {
    id: number,
    name: string,
    email: string,
  };
  managersToAssigned: {
    id: number,
    firstName: string,
    lastName: string,
    email: string,
    jobTitleName: string,
    subordinatesIds: number[]
  }[] = [];
  currentManagerIds: number[] = [];
  postRelationship: boolean = false;

  isLoadingManagers: boolean = false;
  isLoadingAdmins: boolean = false;
  isLoadingUnassignedUsers: boolean = false;

  managersErrorMessage: string = '';
  adminsErrorMessage: string = '';
  unassignedUsersErrorMessage: string = '';

  hasInitialDataLoaded: boolean = false;

  isAdminSectionCollapsed: boolean = true;
  isManagerSectionCollapsed: boolean = true;
  isUnassignedUsersSectionCollapsed: boolean = true;

  managerEmployeesCollapsed: Map<number, boolean> = new Map();

  constructor(
    private userService: UsersService,
    private secondManagerService: SecondManagerService
  ) { }

  ngOnInit(): void {
    this.loadInitialData();
    this.searchTermSubject.pipe(debounceTime(400)).subscribe(term => {
      this.searchTerm = term;
      this.onSearch();
    });
  }
  onSearchTermChange(term: string): void {
    this.searchTermSubject.next(term);
  }

  private loadInitialData(): void {
    this.managersIds.clear();
    this.adminsIds.clear();
    this.managerEmployeesCollapsed.clear();
    
    this.loadManagersWithRelationships();
    this.loadAdmins();
    this.loadUnassignedUsers();
    this.loadActiveSecondManagers();
    this.loadExactCounts();
  }

  private loadExactCounts(): void {
    this.loadTotalAdminsCount();
    this.loadTotalManagersCount();
    this.loadTotalUnassignedUsersCount();
  }

  private loadTotalAdminsCount(): void {
    this.userService.getTotalAdminsCount().subscribe({
      next: (response) => {
        this.totalAdminsCount = response.data;
      },
      error: (err) => {
        console.error('Failed to fetch total admins count:', err);
      },
    });
  }

  private loadTotalManagersCount(): void {
    this.userService.getTotalManagersCount().subscribe({
      next: (response) => {
        this.totalManagersCount = response.data;
      },
      error: (err) => {
        console.error('Failed to fetch total managers count:', err);
      },
    });
  }

  private loadTotalUnassignedUsersCount(): void {
    this.userService.getTotalUnassignedUsersCount().subscribe({
      next: (response) => {
        this.totalUnassignedUsersCount = response.data;
      },
      error: (err) => {
        console.error('Failed to fetch total unassigned users count:', err);
      },
    });
  }


  loadAdmins(): void {
    this.isLoadingAdmins = true;
    this.adminsErrorMessage = '';

    this.userService.getAllAdmins().subscribe({
      next: (response) => {
        this.isLoadingAdmins = false;
        const rawAdmins: IUser[] = response.data;

        if (!rawAdmins || rawAdmins.length === 0) {
          this.adminsErrorMessage = 'No admins were found.';
          this.admins = [];
          this.allAdmins = [];
          this.totalPagesAdmins = 0;
        } else {
          this.originalAdmins = rawAdmins.map(a => this.mapToUserViewModel(a));
          this.allAdmins = [...this.originalAdmins];
          
          if (this.currentSearchBy === UserSearchType.Admins && this.currentSearchTerm.trim()) {
            this.filterAdmins();
          } else {
            this.currentPageAdmins = 1;
            this.sliceAdmins();
          }
        }

        this.checkIfInitialDataLoaded();
      },
      error: (err) => {
        this.isLoadingAdmins = false;
        this.adminsErrorMessage = 'Error during retrieving admins.';
        this.admins = [];
        this.allAdmins = [];
        this.totalPagesAdmins = 0;
        console.error('Failed to fetch admins:', err);
        this.checkIfInitialDataLoaded();
      },
    });
  }


  private sliceAdmins(): void {
    const startIndex = (this.currentPageAdmins - 1) * this.pageSizeAdmins;
    const endIndex = Math.min(startIndex + this.pageSizeAdmins, this.allAdmins.length);
    this.admins = this.allAdmins.slice(startIndex, endIndex);

    this.totalPagesAdmins = Math.ceil(this.allAdmins.length / this.pageSizeAdmins);
  }


  loadManagersWithRelationships(): void {
    this.isLoadingManagers = true;
    this.managersErrorMessage = '';

    let searchParams: any = {};

    if (this.currentSearchBy === UserSearchType.Global || this.currentSearchBy === UserSearchType.Managers) {
      if (this.currentSearchTerm.trim()) {
        searchParams.globalSearch = this.currentSearchTerm;
      }
    } else {
      this.isLoadingManagers = false;
      this.managers = [];
      this.totalPagesManagers = 0;
      this.checkIfInitialDataLoaded();
      return;
    }

    const params: IFilteredUsersRequest = {
      ...searchParams,
      params: {
        sortBy: 'lastName',
        sortDescending: this.sortDescending,
        page: this.currentPageManagers,
        pageSize: this.pageSizeManagers,
      },
    };

    this.userService.getUsersIncludeRelationshipsFiltered(params).subscribe({
      next: (response) => {
        this.isLoadingManagers = false;
        console.log('Managers API response:', response);
        const rawUsers: IUser[] = response.data.data;

        if (!rawUsers || rawUsers.length === 0) {
          this.managersErrorMessage = this.getManagersEmptyMessage();
          this.managers = [];
          this.totalPagesManagers = 0;
        } else {
          this.managers = rawUsers.map((user) => this.mapToUserViewModel(user));
          this.totalPagesManagers = response.data.totalPages;
          this.managersErrorMessage = '';
        }

        this.checkIfInitialDataLoaded();
        this.autoExpandSectionsWithResults();
      },
      error: (err) => {
        this.isLoadingManagers = false;
        this.managersErrorMessage = 'Error retrieving the managers.';
        this.managers = [];
        this.totalPagesManagers = 0;
        console.error('Failed to fetch managers with relationships:', err);
        this.checkIfInitialDataLoaded();
        this.autoExpandSectionsWithResults();
      },
    });
  }

  loadUnassignedUsers(): void {
    this.isLoadingUnassignedUsers = true;
    this.unassignedUsersErrorMessage = '';

    let searchParams: any = {};

    if (this.currentSearchBy === UserSearchType.Global || this.currentSearchBy === UserSearchType.Unassigned) {
      if (this.currentSearchTerm.trim()) {
        searchParams.globalSearch = this.currentSearchTerm;
      }
    } else {
      this.isLoadingUnassignedUsers = false;
      this.unassignedUsers = [];
      this.totalPagesUnassignedUsers = 0;
      this.checkIfInitialDataLoaded();
      return;
    }

    const params: IFilteredUsersRequest = {
      ...searchParams,
      params: {
        sortBy: 'lastName',
        sortDescending: false,
        page: this.currentPageUnassignedUsers,
        pageSize: this.pageSizeUnassignedUsers,
      },
    };

    this.userService.getUnassignedUsers(params).subscribe({
      next: (response) => {
        this.isLoadingUnassignedUsers = false;
        console.log('Unassigned users API response:', response);
        const rawUnassignedUsers: IUser[] = response.data.data;

        if (!rawUnassignedUsers || rawUnassignedUsers.length === 0) {
          this.unassignedUsersErrorMessage = this.getUnassignedUsersEmptyMessage();
          this.unassignedUsers = [];
          this.totalPagesUnassignedUsers = 0;
        } else {
          this.unassignedUsers = rawUnassignedUsers.map((user) =>
            this.mapToUserViewModel(user)
          );
          this.totalPagesUnassignedUsers = response.data.totalPages;
          this.unassignedUsersErrorMessage = '';
        }

        this.checkIfInitialDataLoaded();
        this.autoExpandSectionsWithResults();
      },
      error: (err) => {
        this.isLoadingUnassignedUsers = false;
        this.unassignedUsersErrorMessage = 'Error loading the unassigned users.';
        this.unassignedUsers = [];
        this.totalPagesUnassignedUsers = 0;
        console.error('Failed to fetch unassigned users:', err);
        this.checkIfInitialDataLoaded();
        this.autoExpandSectionsWithResults();
      },
    });
  }

  getManagersEmptyMessage(): string {
    if (this.currentSearchTerm.trim()) {
      switch (this.currentSearchBy) {
        case UserSearchType.Global:
          return 'No managers found for the global search term.';
        case UserSearchType.Managers:
          return `No managers found for "${this.currentSearchTerm}".`;
        default:
          return `No managers found for "${this.currentSearchTerm}".`;
      }
    }
    return 'No managers found.';
  }

  getUnassignedUsersEmptyMessage(): string {
    if (this.currentSearchTerm.trim()) {
      return `No unassigned users found for "${this.currentSearchTerm}".`;
    }
    return 'No unassigned users found.';
  }

  private checkIfInitialDataLoaded(): void {
    if (!this.isLoadingManagers && !this.isLoadingAdmins && !this.isLoadingUnassignedUsers && !this.isLoadingSecondManagers) {
      this.hasInitialDataLoaded = true;
    }
  }

  mapToUserViewModel(user: IUser): IUserViewModel {
    if (user.roles.includes('Manager')) {
      this.managersIds.add(user.id);
    }

    if (user.roles.includes('Admin')) {
      this.adminsIds.add(user.id);
    }

    let filteredSubordinatesIds = user.subordinatesIds || [];
    let filteredSubordinatesNames = user.subordinatesNames || [];
    let filteredSubordinatesEmails = user.subordinatesEmails || [];
    let filteredSubordinatesJobTitleIds = user.subordinatesJobTitleIds || [];
    let filteredSubordinatesJobTitleNames = user.subordinatesJobTitles || [];

    if ((this.currentSearchBy === UserSearchType.Global || this.currentSearchBy === UserSearchType.Managers) && this.currentSearchTerm.trim()) {
      const searchTerm = this.currentSearchTerm.toLowerCase();
      const filteredIndices: number[] = [];

      filteredSubordinatesNames.forEach((name, index) => {
        const email = filteredSubordinatesEmails[index] || '';
        const jobTitle = filteredSubordinatesJobTitleNames[index] || '';
        
        if (name.toLowerCase().includes(searchTerm) ||
            email.toLowerCase().includes(searchTerm) ||
            jobTitle.toLowerCase().includes(searchTerm)) {
          filteredIndices.push(index);
        }
      });

      if (filteredIndices.length > 0) {
        this.managerEmployeesCollapsed.set(user.id, false);
      }

      filteredSubordinatesIds = filteredIndices.map(i => filteredSubordinatesIds[i]);
      filteredSubordinatesNames = filteredIndices.map(i => filteredSubordinatesNames[i]);
      filteredSubordinatesEmails = filteredIndices.map(i => filteredSubordinatesEmails[i]);
      filteredSubordinatesJobTitleIds = filteredIndices.map(i => filteredSubordinatesJobTitleIds[i]);
      filteredSubordinatesJobTitleNames = filteredIndices.map(i => filteredSubordinatesJobTitleNames[i]);
    }

    return {
      id: user.id,
      name: `${user.firstName} ${user.lastName}`,
      email: user.email,
      jobTitle: user.jobTitleId
        ? {
          id: user.jobTitleId,
          name: user.jobTitleName || 'Unknown',
        }
        : undefined,
      department: {
        id: user.departmentId || 0,
        name: user.departmentName || 'Unknown',
      },
      subordinatesIds: filteredSubordinatesIds,
      subordinatesNames: filteredSubordinatesNames,
      roles: user.roles || [],
      subordinatesJobTitleIds: filteredSubordinatesJobTitleIds,
      subordinatesJobTitleNames: filteredSubordinatesJobTitleNames,
      managersIds: user.managersIds || [],
      subordinatesEmails: filteredSubordinatesEmails,
    };
  }

  hasManagersError(): boolean {
    return !!this.managersErrorMessage;
  }

  hasAdminsError(): boolean {
    return !!this.adminsErrorMessage;
  }

  hasUnassignedUsersError(): boolean {
    return !!this.unassignedUsersErrorMessage;
  }

  hasSearchResults(): boolean {
    return this.managers.length > 0;
  }

  retryLoadManagers(): void {
    this.loadManagersWithRelationships();
  }

  retryLoadAdmins(): void {
    this.loadAdmins();
  }

  retryLoadUnassignedUsers(): void {
    this.loadUnassignedUsers();
  }

  getStartResultIndexManagers(): number {
    return (this.currentPageManagers - 1) * this.pageSizeManagers + 1;
  }

  getMaxDisplayedResultManagers(): number {
    return this.getStartResultIndexManagers() + this.managers.length - 1;
  }


  goToPageManagers(page: number): void {
    if (
      page >= 1 &&
      page <= this.totalPagesManagers &&
      page !== this.currentPageManagers
    ) {
      this.currentPageManagers = page;
      this.loadManagersWithRelationships();
    }
  }

  goToNextPageManagers(): void {
    if (this.currentPageManagers < this.totalPagesManagers) {
      this.currentPageManagers++;
      this.loadManagersWithRelationships();
    }
  }

  goToPreviousPageManagers(): void {
    if (this.currentPageManagers > 1) {
      this.currentPageManagers--;
      this.loadManagersWithRelationships();
    }
  }

  goToFirstPageManagers(): void {
    if (this.currentPageManagers !== 1) {
      this.currentPageManagers = 1;
      this.loadManagersWithRelationships();
    }
  }

  goToLastPageManagers(): void {
    if (this.currentPageManagers !== this.totalPagesManagers) {
      this.currentPageManagers = this.totalPagesManagers;
      this.loadManagersWithRelationships();
    }
  }

  getPageNumbersManagers(): number[] {
    const pages: number[] = [];
    const maxVisiblePages = 5;
    const half = Math.floor(maxVisiblePages / 2);

    let start = Math.max(1, this.currentPageManagers - half);
    let end = Math.min(this.totalPagesManagers, start + maxVisiblePages - 1);

    if (end - start + 1 < maxVisiblePages) {
      start = Math.max(1, end - maxVisiblePages + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }

    return pages;
  }

  goToPageUnassignedUsers(page: number): void {
    if (
      page >= 1 &&
      page <= this.totalPagesUnassignedUsers &&
      page !== this.currentPageUnassignedUsers
    ) {
      this.currentPageUnassignedUsers = page;
      this.loadUnassignedUsers();
    }
  }

  goToNextPageUnassignedUsers(): void {
    if (this.currentPageUnassignedUsers < this.totalPagesUnassignedUsers) {
      this.currentPageUnassignedUsers++;
      this.loadUnassignedUsers();
    }
  }

  goToPreviousPageUnassignedUsers(): void {
    if (this.currentPageUnassignedUsers > 1) {
      this.currentPageUnassignedUsers--;
      this.loadUnassignedUsers();
    }
  }

  goToFirstPageUnassignedUsers(): void {
    if (this.currentPageUnassignedUsers !== 1) {
      this.currentPageUnassignedUsers = 1;
      this.loadUnassignedUsers();
    }
  }

  goToLastPageUnassignedUsers(): void {
    if (this.currentPageUnassignedUsers !== this.totalPagesUnassignedUsers) {
      this.currentPageUnassignedUsers = this.totalPagesUnassignedUsers;
      this.loadUnassignedUsers();
    }
  }

  getPageNumbersUnassignedUsers(): number[] {
    const pages: number[] = [];
    const maxVisiblePages = 5;
    const half = Math.floor(maxVisiblePages / 2);

    let start = Math.max(1, this.currentPageUnassignedUsers - half);
    let end = Math.min(
      this.totalPagesUnassignedUsers,
      start + maxVisiblePages - 1
    );

    if (end - start + 1 < maxVisiblePages) {
      start = Math.max(1, end - maxVisiblePages + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }

    return pages;
  }

  getMaxDisplayedResultUnassignedUsers(): number {
    return this.getStartResultIndexUnassignedUsers() + this.unassignedUsers.length - 1;
  }

  getStartResultIndexUnassignedUsers(): number {
    return (
      (this.currentPageUnassignedUsers - 1) * this.pageSizeUnassignedUsers + 1
    );
  }

  getSearchPlaceholder(): string {
    switch (this.searchBy) {
      case UserSearchType.Global:
        return 'Search by name, email, job title, department...';
      case UserSearchType.Admins:
        return 'Search admins by name, email, job title, department...';
      case UserSearchType.Managers:
        return 'Search managers by name, email, job title, department...';
      case UserSearchType.Unassigned:
        return 'Search unassigned users by name, email, job title, department...';
      default:
        return 'Search...';
    }
  }

  filterAdmins(): void {
    if (!this.currentSearchTerm.trim()) {
      this.allAdmins = [...this.originalAdmins];
      this.currentPageAdmins = 1;
      this.sliceAdmins();
      return;
    }

    const searchTerm = this.currentSearchTerm.toLowerCase();
    this.allAdmins = this.originalAdmins.filter(admin => 
      admin.name.toLowerCase().includes(searchTerm) ||
      admin.email.toLowerCase().includes(searchTerm) ||
      (admin.jobTitle?.name || '').toLowerCase().includes(searchTerm) ||
      (admin.department?.name || '').toLowerCase().includes(searchTerm)
    );

    this.currentPageAdmins = 1;
    this.sliceAdmins();
    this.autoExpandSectionsWithResults();
  }

  onSearchCategoryChange(): void {
    this.onSearch();
  }

  onSearch(): void {
    this.currentSearchTerm = this.searchTerm;
    this.currentSearchBy = this.searchBy;
    this.currentHighlightTerm = this.searchTerm;
    this.currentPageManagers = 1;
    this.currentPageUnassignedUsers = 1;

    this.managersIds.clear();
    this.adminsIds.clear();

    this.managerEmployeesCollapsed.clear();

    if (this.currentSearchBy !== UserSearchType.Admins) {
      this.allAdmins = [...this.originalAdmins];
      this.currentPageAdmins = 1;
      this.sliceAdmins();
    }
    if (this.currentSearchTerm.trim()) {
      switch (this.currentSearchBy) {
        case UserSearchType.Admins:
          this.isAdminSectionCollapsed = false;
          break;
        case UserSearchType.Managers:
          this.isManagerSectionCollapsed = false;
          break;
        case UserSearchType.Unassigned:
          this.isUnassignedUsersSectionCollapsed = false;
          break;
        case UserSearchType.Global:
          break;
      }
    }

    switch (this.currentSearchBy) {
      case UserSearchType.Global:
        if (this.currentSearchTerm.trim()) {
          this.filterAdmins();
        } else {
          this.allAdmins = [...this.originalAdmins];
          this.currentPageAdmins = 1;
          this.sliceAdmins();
        }
        this.loadManagersWithRelationships();
        this.loadUnassignedUsers();
        break;
      case UserSearchType.Managers:
        this.loadManagersWithRelationships();
        break;
      case UserSearchType.Admins:
        this.filterAdmins();
        break;
      case UserSearchType.Unassigned:
        this.loadUnassignedUsers();
        break;
    }
  }

  toggleSortOrder(): void {
    this.sortDescending = !this.sortDescending;
    this.currentPageManagers = 1;
    this.loadManagersWithRelationships();
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.searchBy = UserSearchType.Global;
    this.currentSearchTerm = '';
    this.currentSearchBy = UserSearchType.Global;
    this.currentHighlightTerm = '';
    this.currentPageManagers = 1;
    this.currentPageUnassignedUsers = 1;
    
    this.managersIds.clear();
    this.adminsIds.clear();
    
    this.managerEmployeesCollapsed.clear();
    

    this.isAdminSectionCollapsed = true;
    this.isManagerSectionCollapsed = true;
    this.isUnassignedUsersSectionCollapsed = true;
    

    this.allAdmins = [...this.originalAdmins];
    this.currentPageAdmins = 1;
    this.sliceAdmins();
    
    this.loadManagersWithRelationships();
    this.loadUnassignedUsers();
  }

  isManager(userId: number | undefined): boolean {
    return userId !== undefined && this.managersIds.has(userId);
  }

  isAdmin(userId: number | undefined): boolean {
    return userId !== undefined && this.adminsIds.has(userId);
  }

  areUnassignedUsers(): boolean {
    return this.unassignedUsers.length > 0;
  }

  getTotalUnassignedUsersCount(): number {
    return this.totalUnassignedUsersCount;
  }

  isSearchActive(): boolean {
    return this.currentSearchTerm.trim().length > 0;
  }

  getUnassignedUsersCount(): number {
    return this.unassignedUsers.length;
  }

  getSubordinateCount(manager: any): number {
    return manager.subordinatesIds?.length || 0;
  }

  getAdminCount(): number {
    return this.admins.length;
  }

  getTotalAdminCount(): number {
    return this.totalAdminsCount;
  }

  getManagerCount(): number {
    return this.managers.length;
  }

  getTotalManagerCount(): number {
    return this.totalManagersCount;
  }

  assignManager(user: IUserViewModel, isPost: boolean = false): void {
    this.postRelationship = isPost;
    this.selectedEmployee = {
      id: user.id,
      name: user.name,
      email: user.email
    };
    this.showAssignRelationShipComponent = true;
  }

  editRelationship(relationship: any): void {
    console.log('Edit relationship:', relationship);
  }

  viewRelationship(relationship: any): void {
    console.log('View relationship:', relationship);
  }

  getRoleBadgeClass(role: string): string {
    switch (role?.toLowerCase()) {
      case 'admin':
        return 'bg-red-100 text-red-800';
      case 'manager':
        return 'bg-green-100 text-green-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }

  onAssignManagers(selectedManagerIds: number[]) {
    this.loadAdmins();
    this.loadManagersWithRelationships();
    this.loadUnassignedUsers();
    this.loadExactCounts();
  }

  onSearchKeypress(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.onSearch();
    }
  }

  getHighlightTerm(fieldType: 'name' | 'email' | 'jobTitle'): string {
    if (!this.currentHighlightTerm) return '';

    switch (this.currentSearchBy) {
      case UserSearchType.Global:
      case UserSearchType.Admins:
      case UserSearchType.Managers:
      case UserSearchType.Unassigned:
        return this.currentHighlightTerm;
      default:
        return '';
    }
  }

  getInitials(fullName: string): string {
    if (!fullName) return '';
    const names = fullName.split(' ');
    const firstInitial = names[0]?.charAt(0).toUpperCase() || '';
    const lastInitial = names[names.length - 1]?.charAt(0).toUpperCase() || '';
    return firstInitial + lastInitial;
  }

  closeAssignModal() {
    this.showAssignRelationShipComponent = false;
  }

  onItemsPerPageChangeManagers(): void {
    this.currentPageManagers = 1;
    this.loadManagersWithRelationships();
  }

  onItemsPerPageChangeUnassignedUsers(): void {
    this.currentPageUnassignedUsers = 1;
    this.loadUnassignedUsers();
  }

  goToPageAdmins(page: number): void {
    if (page >= 1 && page <= this.totalPagesAdmins && page !== this.currentPageAdmins) {
      this.currentPageAdmins = page;
      this.sliceAdmins();
    }
  }

  goToNextPageAdmins(): void {
    if (this.currentPageAdmins < this.totalPagesAdmins) {
      this.currentPageAdmins++;
      this.sliceAdmins();
    }
  }

  goToPreviousPageAdmins(): void {
    if (this.currentPageAdmins > 1) {
      this.currentPageAdmins--;
      this.sliceAdmins();
    }
  }

  goToFirstPageAdmins(): void {
    if (this.currentPageAdmins !== 1) {
      this.currentPageAdmins = 1;
      this.sliceAdmins();
    }
  }

  goToLastPageAdmins(): void {
    if (this.currentPageAdmins !== this.totalPagesAdmins) {
      this.currentPageAdmins = this.totalPagesAdmins;
      this.sliceAdmins();
    }
  }

  getPageNumbersAdmins(): number[] {
    const pages: number[] = [];
    const maxVisiblePages = 5;
    const half = Math.floor(maxVisiblePages / 2);

    let start = Math.max(1, this.currentPageAdmins - half);
    let end = Math.min(this.totalPagesAdmins, start + maxVisiblePages - 1);

    if (end - start + 1 < maxVisiblePages) {
      start = Math.max(1, end - maxVisiblePages + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }

    return pages;
  }

  getStartResultIndexAdmins(): number {
    return (this.currentPageAdmins - 1) * this.pageSizeAdmins + 1;
  }

  getMaxDisplayedResultAdmins(): number {
    return this.getStartResultIndexAdmins() + this.admins.length - 1;
  }

  onItemsPerPageChangeAdmins(): void {
    this.currentPageAdmins = 1;
    this.totalPagesAdmins = Math.ceil(this.allAdmins.length / this.pageSizeAdmins);
    this.sliceAdmins();
  }

  Math = Math;

  toggleAdminSection(): void {
    this.isAdminSectionCollapsed = !this.isAdminSectionCollapsed;
  }

  toggleManagerSection(): void {
    this.isManagerSectionCollapsed = !this.isManagerSectionCollapsed;
  }

  toggleUnassignedUsersSection(): void {
    this.isUnassignedUsersSectionCollapsed = !this.isUnassignedUsersSectionCollapsed;
  }

  toggleManagerEmployees(managerId: number): void {
    const currentState = this.managerEmployeesCollapsed.get(managerId) || false;
    this.managerEmployeesCollapsed.set(managerId, !currentState);
  }

  isManagerEmployeesCollapsed(managerId: number): boolean {
    return this.managerEmployeesCollapsed.get(managerId) ?? true;
  }

  shouldShowAdminSection(): boolean {
    const shouldShowByCategory = !this.currentSearchTerm.trim() || 
           this.currentSearchBy === UserSearchType.Global || 
           this.currentSearchBy === UserSearchType.Admins;
    
    if (this.currentSearchBy === UserSearchType.Global && this.currentSearchTerm.trim()) {
      return shouldShowByCategory && this.admins.length > 0;
    }
    
    return shouldShowByCategory;
  }

  shouldShowManagerSection(): boolean {
    const shouldShowByCategory = !this.currentSearchTerm.trim() || 
           this.currentSearchBy === UserSearchType.Global || 
           this.currentSearchBy === UserSearchType.Managers;
    
    if (this.currentSearchBy === UserSearchType.Global && this.currentSearchTerm.trim()) {
      return shouldShowByCategory && this.managers.length > 0;
    }
    
    return shouldShowByCategory;
  }

  shouldShowUnassignedSection(): boolean {
    const shouldShowByCategory = !this.currentSearchTerm.trim() || 
           this.currentSearchBy === UserSearchType.Global || 
           this.currentSearchBy === UserSearchType.Unassigned;
    
    if (this.currentSearchBy === UserSearchType.Global && this.currentSearchTerm.trim()) {
      return shouldShowByCategory && this.unassignedUsers.length > 0;
    }
    
    return shouldShowByCategory;
  }

  private autoExpandSectionsWithResults(): void {
    if (this.currentSearchBy === UserSearchType.Global && this.currentSearchTerm.trim()) {
      if (this.admins.length > 0) {
        this.isAdminSectionCollapsed = false;
      }
      
      if (this.managers.length > 0) {
        this.isManagerSectionCollapsed = false;
      }
      
      if (this.unassignedUsers.length > 0) {
        this.isUnassignedUsersSectionCollapsed = false;
      }
    }
  }

  loadActiveSecondManagers(): void {
    this.isLoadingSecondManagers = true;
    this.secondManagersErrorMessage = '';

    this.secondManagerService.getActiveSecondManagers().subscribe({
      next: (response) => {
        this.isLoadingSecondManagers = false;
        console.log('Active second managers API response:', response);
        
        if (Array.isArray(response)) {
          this.activeSecondManagers = response.map(sm => {
            console.log('Processing second manager:', sm);
            return this.mapSecondManagerToViewModel(sm);
          });
        } else {
          console.error('Unexpected response format - not an array:', response);
          this.activeSecondManagers = [];
        }
        
        this.secondManagersErrorMessage = '';
        this.checkIfInitialDataLoaded();
      },
      error: (err) => {
        this.isLoadingSecondManagers = false;
        this.secondManagersErrorMessage = 'Error loading active second managers.';
        this.activeSecondManagers = [];
        console.error('Failed to fetch active second managers:', err);
        this.checkIfInitialDataLoaded();
      }
    });
  }

  mapSecondManagerToViewModel(secondManager: any): ISecondManagerViewModel {
    return {
      id: secondManager.secondManagerEmployeeId,
      name: secondManager.secondManagerEmployeeName || 'Unknown Employee',
      email: secondManager.secondManagerEmployeeEmail || 'No email',
      jobTitle: undefined, 
      department: undefined, 
      replacedManagerId: secondManager.replacedManagerId,
      replacedManagerName: secondManager.replacedManagerName || 'Unknown Manager',
      startDate: new Date(secondManager.startDate),
      endDate: new Date(secondManager.endDate),
      isActive: secondManager.isActive
    };
  }

  getActiveSecondManagerForManager(managerId: number): ISecondManagerViewModel | null {
    return this.activeSecondManagers.find(sm => sm.replacedManagerId === managerId) || null;
  }

  getSecondManagerDaysRemaining(secondManager: ISecondManagerViewModel): number {
    const today = new Date();
    const end = new Date(secondManager.endDate);
    const diffMs = end.getTime() - today.getTime();
    const days = Math.ceil(diffMs / (1000 * 60 * 60 * 24));
    return Math.max(0, days);
  }

  getSecondManagerTotalDays(secondManager: ISecondManagerViewModel): number {
    const start = new Date(secondManager.startDate);
    const end = new Date(secondManager.endDate);
    const diffMs = end.getTime() - start.getTime();
    const days = Math.max(1, Math.ceil(diffMs / (1000 * 60 * 60 * 24)));
    return days;
  }

  getSecondManagerProgressPercent(secondManager: ISecondManagerViewModel): number {
    const start = new Date(secondManager.startDate).getTime();
    const end = new Date(secondManager.endDate).getTime();
    const now = Date.now();
    if (now <= start) return 0;
    if (now >= end) return 100;
    const pct = ((now - start) / (end - start)) * 100;
    return Math.max(0, Math.min(100, Math.round(pct)));
  }

  getSecondManagerStatusColor(secondManager: ISecondManagerViewModel): string {
    const days = this.getSecondManagerDaysRemaining(secondManager);
    if (days <= 1) return 'text-red-600 bg-red-50 border-red-200';
    if (days <= 3) return 'text-amber-700 bg-amber-50 border-amber-200';
    return 'text-emerald-700 bg-emerald-50 border-emerald-200';
  }

  getSecondManagerDotClass(secondManager: ISecondManagerViewModel): string {
    const days = this.getSecondManagerDaysRemaining(secondManager);
    if (days <= 1) return 'bg-red-500';
    if (days <= 3) return 'bg-amber-500';
    return 'bg-emerald-500';
  }

  openSecondManagerModal(employee: { id: number; name: string; email: string }, managerId: number): void {
    const existingSecondManager = this.getActiveSecondManagerForManager(managerId);
    if (existingSecondManager) {
      alert(`There is already an active second manager (${existingSecondManager.name}) for this manager until ${new Date(existingSecondManager.endDate).toLocaleDateString()}. Please wait until the current assignment expires or remove it first.`);
      return;
    }

    this.selectedSecondManagerEmployee = employee;
    this.replacedManagerId = managerId;
    this.showSecondManagerModal = true;
    this.secondManagerErrorMessage = '';

    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const endDate = new Date();
    endDate.setDate(endDate.getDate() + 30);

    this.secondManagerForm = {
      startDate: tomorrow.toISOString().split('T')[0],
      endDate: endDate.toISOString().split('T')[0]
    };
  }

  closeSecondManagerModal(): void {
    this.showSecondManagerModal = false;
    this.selectedSecondManagerEmployee = null;
    this.replacedManagerId = 0;
    this.secondManagerErrorMessage = '';
    this.isSubmittingSecondManager = false;
    this.secondManagerForm = {
      startDate: '',
      endDate: ''
    };
  }

  isSecondManagerFormValid(): boolean {
    if (!this.secondManagerForm.startDate || !this.secondManagerForm.endDate) {
      return false;
    }

    const startDate = new Date(this.secondManagerForm.startDate);
    const endDate = new Date(this.secondManagerForm.endDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (startDate < today) {
      return false;
    }

    if (endDate <= startDate) {
      return false;
    }

    return true;
  }

  onSubmitSecondManager(): void {
    if (!this.isSecondManagerFormValid() || !this.selectedSecondManagerEmployee) {
      return;
    }

    this.isSubmittingSecondManager = true;
    this.secondManagerErrorMessage = '';

    const currentTime = new Date();
    const timeString = currentTime.toISOString().split('T')[1];

    const startDateTime = `${this.secondManagerForm.startDate}T${timeString}`;
    const endDateTime = `${this.secondManagerForm.endDate}T${timeString}`;

    const createRequest = {
      secondManagerEmployeeId: this.selectedSecondManagerEmployee.id,
      replacedManagerId: this.replacedManagerId,
      startDate: startDateTime,
      endDate: endDateTime
    };

    this.secondManagerService.createSecondManager(createRequest).subscribe({
      next: (response) => {
        this.isSubmittingSecondManager = false;
        console.log('Second manager created successfully:', response);

        this.loadActiveSecondManagers();

        this.closeSecondManagerModal();

        this.showToast('Second manager created successfully!');
      },
      error: (err) => {
        this.isSubmittingSecondManager = false;
        console.error('Failed to create second manager:', err);

        if (err.error && err.error.message) {
          this.secondManagerErrorMessage = err.error.message;
        } else {
          this.secondManagerErrorMessage = 'Failed to create second manager. Please try again.';
        }
      }
    });
  }

  removeSecondManager(secondManager: ISecondManagerViewModel): void {
    this.pendingRemoveSecondManager = secondManager;
    this.removeSecondManagerErrorMessage = '';
    this.showConfirmRemoveSecondManager = true;
  }

  confirmRemoveSecondManager(): void {
    if (!this.pendingRemoveSecondManager) {
      this.cancelRemoveSecondManager();
      return;
    }

    const secondManager = this.pendingRemoveSecondManager;

    this.secondManagerService.deleteSecondManager(
      secondManager.id,
      secondManager.replacedManagerId,
      secondManager.startDate
    ).subscribe({
      next: () => {
        console.log('Second manager removed successfully');

        this.loadActiveSecondManagers();

        this.showToast(`${secondManager.name} has been removed as second manager successfully!`);
        this.showConfirmRemoveSecondManager = false;
        this.pendingRemoveSecondManager = null;
      },
      error: (err: any) => {
        console.error('Failed to remove second manager:', err);

        let errorMessage = 'Failed to remove second manager. Please try again.';
        if (err.error && err.error.message) {
          errorMessage = err.error.message;
        }

        this.removeSecondManagerErrorMessage = errorMessage;
      }
    });
  }

  cancelRemoveSecondManager(): void {
    this.showConfirmRemoveSecondManager = false;
    this.pendingRemoveSecondManager = null;
    this.removeSecondManagerErrorMessage = '';
  }

  private showToast(message: string): void {
    this.successMessage = message;
    this.showSuccessMessage = true;
    if (this.toastTimeoutRef) {
      clearTimeout(this.toastTimeoutRef);
    }
    this.toastTimeoutRef = setTimeout(() => {
      this.showSuccessMessage = false;
      this.successMessage = '';
      this.toastTimeoutRef = null;
    }, 2000);
  }

  closeSuccessMessage(): void {
    if (this.toastTimeoutRef) {
      clearTimeout(this.toastTimeoutRef);
      this.toastTimeoutRef = null;
    }
    this.showSuccessMessage = false;
    this.successMessage = '';
  }

  openEditSecondManagerModal(secondManager: ISecondManagerViewModel): void {
    this.selectedSecondManagerForEdit = secondManager;
    this.showEditSecondManagerModal = true;
    this.editSecondManagerErrorMessage = '';

    // Set the current end date as the default value
    const endDate = new Date(secondManager.endDate);
    this.editSecondManagerForm = {
      endDate: endDate.toISOString().split('T')[0]
    };
  }

  closeEditSecondManagerModal(): void {
    this.showEditSecondManagerModal = false;
    this.selectedSecondManagerForEdit = null;
    this.editSecondManagerErrorMessage = '';
    this.isSubmittingEditSecondManager = false;
    this.editSecondManagerForm = {
      endDate: ''
    };
  }

  isEditSecondManagerFormValid(): boolean {
    if (!this.editSecondManagerForm.endDate || !this.selectedSecondManagerForEdit) {
      return false;
    }

    const newEndDate = new Date(this.editSecondManagerForm.endDate);
    const startDate = new Date(this.selectedSecondManagerForEdit.startDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    // New end date must be after start date
    if (newEndDate <= startDate) {
      return false;
    }

    // New end date must be different from current end date
    const currentEndDate = new Date(this.selectedSecondManagerForEdit.endDate);
    if (newEndDate.getTime() === currentEndDate.getTime()) {
      return false;
    }

    return true;
  }

  onSubmitEditSecondManager(): void {
    if (!this.isEditSecondManagerFormValid() || !this.selectedSecondManagerForEdit) {
      return;
    }

    this.isSubmittingEditSecondManager = true;
    this.editSecondManagerErrorMessage = '';

    const currentTime = new Date();
    const timeString = currentTime.toISOString().split('T')[1];
    const endDateTime = `${this.editSecondManagerForm.endDate}T${timeString}`;

    const updateRequest = {
      newEndDate: endDateTime
    };

    this.secondManagerService.updateSecondManager(
      this.selectedSecondManagerForEdit.id,
      this.selectedSecondManagerForEdit.replacedManagerId,
      this.selectedSecondManagerForEdit.startDate,
      updateRequest
    ).subscribe({
      next: (response) => {
        this.isSubmittingEditSecondManager = false;
        console.log('Second manager updated successfully:', response);

        this.loadActiveSecondManagers();

        this.closeEditSecondManagerModal();

        this.showToast('Second manager end date updated successfully!');
      },
      error: (err) => {
        this.isSubmittingEditSecondManager = false;
        console.error('Failed to update second manager:', err);

        if (err.error && err.error.message) {
          this.editSecondManagerErrorMessage = err.error.message;
        } else {
          this.editSecondManagerErrorMessage = 'Failed to update second manager. Please try again.';
        }
      }
    });
  }
}