import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IUser } from '../../../models/entities/iuser';
import { IUserViewModel } from '../../../view-models/user-view-model';
import { UsersService } from '../../../services/users/users-service';
import { IFilteredUsersRequest } from '../../../models/requests/ifiltered-users-request';

@Component({
  selector: 'app-admin-user-relationships',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-user-relationships.html',
  styleUrl: './admin-user-relationships.css'
})
export class AdminUserRelationships implements OnInit {
  managersIds: Set<number> = new Set<number>();
  adminsIds: Set<number> = new Set<number>();
  managers: IUserViewModel[] = []; 
  admins: IUserViewModel[] = [];
  unassignedUsers: IUserViewModel[] = [];

  searchTerm: string = '';
  searchBy: 'lastName' | 'email' = 'lastName';
  sortDescending: boolean = false;
  
  readonly pageSizeManagers: number = 3;
  currentPageManagers: number = 1;
  totalPagesManagers: number = 0;

  readonly pageSizeUnassignedUsers: number = 3;
  currentPageUnassignedUsers: number = 1;
  totalPagesUnassignedUsers: number = 0;

  constructor(private userService: UsersService) {}

  ngOnInit(): void {
    this.loadManagersWithRelationships();
    this.loadAdmins();
    this.loadUnassignedUsers();
  }

  loadAdmins(): void {
    this.userService.getAllAdmins().subscribe({
      next: (response) => {
        const rawAdmins: IUser[] = response.data;
        this.admins = rawAdmins.map(admin => this.mapToUserViewModel(admin));
      },
      error: (err) => {
        console.error('Failed to fetch admins:', err);
      }
    })
  }

  loadManagersWithRelationships(): void {
    const params: IFilteredUsersRequest = {
      [this.searchBy === 'email' ? 'email' : 'lastName']: this.searchTerm,
      params: {
        sortBy: this.searchBy === 'lastName' ? 'lastName' : 'email',
        sortDescending: this.sortDescending,
        page: this.currentPageManagers,
        pageSize: this.pageSizeManagers
      }
    };

    this.userService.getUsersIncludeRelationshipsFiltered(params).subscribe({
      next: (response) => {
        console.log('Managers API response:', response);
        const rawUsers: IUser[] = response.data.data;
        this.managers = rawUsers.map(user => this.mapToUserViewModel(user));
        
        this.totalPagesManagers = response.data.totalPages;
      },
      error: (err) => {
        console.error('Failed to fetch managers with relationships:', err);
      }
    });
  }

  loadUnassignedUsers(): void {
    const params: IFilteredUsersRequest = {
      params: {
        sortBy: 'lastName', 
        sortDescending: false,
        page: this.currentPageUnassignedUsers,
        pageSize: this.pageSizeUnassignedUsers
      }
    };

    this.userService.getUnassignedUsers(params).subscribe({
      next: (response) => {
        console.log('Unassigned users API response:', response);
        const rawUnassignedUsers: IUser[] = response.data.data;
        this.unassignedUsers = rawUnassignedUsers.map(user => this.mapToUserViewModel(user));
        
        this.totalPagesUnassignedUsers = response.data.totalPages;
      },
      error: (err) => {
        console.error('Failed to fetch unassigned users:', err);
      }
    });
  }

  mapToUserViewModel(user: IUser): IUserViewModel {
    user.managersIds?.forEach(element => {
      this.managersIds.add(element);
    });

    if(user.roles.includes("Admin")) { 
      this.adminsIds.add(user.id);
    }

    return {
      id: user.id,
      name: `${user.firstName} ${user.lastName}`,
      email: user.email,
      jobTitle: user.jobTitleId ? {
        id: user.jobTitleId,
        name: user.jobTitleName || 'Unknown',
        department: {
          id: user.departmentId || 0,
          name: user.departmentName || 'Unknown'
        }
      } : undefined,
      subordinatesIds: user.subordinatesIds || [],
      subordinatesNames: user.subordinatesNames || [],
      roles : user.roles || [],
      subordinatesJobTitleIds: user.subordinatesJobTitleIds || [],
      subordinatesJobTitleNames: user.subordinatesJobTitles  || [],
      managersIds: user.managersIds || [],
      subordinatesEmails: user.subordinatesEmails || [],
    };
  }

  getMaxDisplayedResultManagers(): number {
    return Math.min(this.currentPageManagers * this.pageSizeManagers, this.managers.length);
  }

  getStartResultIndexManagers(): number {
    return ((this.currentPageManagers - 1) * this.pageSizeManagers) + 1;
  }

  goToPageManagers(page: number): void {
    if (page >= 1 && page <= this.totalPagesManagers && page !== this.currentPageManagers) {
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
    if (page >= 1 && page <= this.totalPagesUnassignedUsers && page !== this.currentPageUnassignedUsers) {
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
    let end = Math.min(this.totalPagesUnassignedUsers, start + maxVisiblePages - 1);
    
    if (end - start + 1 < maxVisiblePages) {
      start = Math.max(1, end - maxVisiblePages + 1);
    }
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    return pages;
  }

  getMaxDisplayedResultUnassignedUsers(): number {
    return Math.min(this.currentPageUnassignedUsers * this.pageSizeUnassignedUsers, this.unassignedUsers.length);
  }

  getStartResultIndexUnassignedUsers(): number {
    return ((this.currentPageUnassignedUsers - 1) * this.pageSizeUnassignedUsers) + 1;
  }

  getSearchPlaceholder(): string {
    return this.searchBy === 'lastName' 
      ? 'Search managers by last name...' 
      : 'Search managers by email...';
  }

  onSearch(): void {
    this.currentPageManagers = 1; 
    if(this.searchTerm !== '') {
      this.loadManagersWithRelationships();
    }
  }

  toggleSortOrder(): void {
    this.sortDescending = !this.sortDescending;
    this.currentPageManagers = 1; 
    this.loadManagersWithRelationships();
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.currentPageManagers = 1; 
    this.loadManagersWithRelationships();
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

  getUnassignedUsersCount(): number {
    return this.unassignedUsers.length;
  }
  
  getSubordinateCount(manager: any): number {
    return manager.subordinatesIds?.length || 0;
  }

  getAdminCount(): number {
    return this.admins.length;
  }

  getManagerCount(): number {
    return this.managers.filter(user => user.roles?.includes("Manager")).length;
  }

  assignManager(user: any): void {
    console.log('Assign manager to:', user);
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
}