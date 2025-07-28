import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UsersService } from '../../../services/users/users-service';
import { UserViewModel } from '../../../view-models/user-view-model';
import { IUser } from '../../../models/entities/iuser';
import { IFilteredUsersRequest } from '../../../models/requests/ifiltered-users-request';
import { IFilteredApiResponse } from '../../../models/responses/ifiltered-api-response';
import { IApiResponse } from '../../../models/responses/iapi-response';

@Component({
  selector: 'app-admin-users-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-users-list.html',
  styleUrl: './admin-users-list.css'
})
export class AdminUsersList implements OnInit {
  users: UserViewModel[] = [];
  searchTerm: string = '';
  searchBy: string = 'name';
  sortDescending: boolean = false;
  
  currentPage: number = 1;
  itemsPerPage: number = 5;
  totalPages: number = 0;
  
  isLoading: boolean = false;

  constructor(private usersService: UsersService) { }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    
    const filterRequest: IFilteredUsersRequest = {
      lastName: this.searchBy === 'name' ? this.searchTerm : undefined,
      email: this.searchBy === 'email' ? this.searchTerm : undefined,
      params: {
        sortBy: this.getSortField(),
        sortDescending: this.sortDescending,
        page: this.currentPage,
        pageSize: this.itemsPerPage
      }
    };

    this.usersService.getAllUsersFiltered(filterRequest).subscribe({
      next: (response: IApiResponse<IFilteredApiResponse<IUser>>) => {
        console.log('API response:', response);   
        const rawUsers: IUser[] = response.data.data || [];
        this.users = rawUsers.map(user => this.mapToUserViewModel(user));
        this.totalPages = response.data.totalPages
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to fetch users:', err);
        this.isLoading = false;
      }
    });
  }

  private getSortField(): string {
    switch (this.searchBy) {
      case 'name':
        return 'lastName';
      case 'email':
        return 'email';
      default:
        return 'lastName';
    }
  }

  private mapToUserViewModel(user: IUser): UserViewModel {
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
      status: user.isActive ? 'active' : 'inactive',
      avatar: `https://ui-avatars.com/api/?name=${user.firstName}+${user.lastName}&background=20B486&color=fff`
    };
  }

  // Search functionality
  getSearchPlaceholder(): string {
    switch (this.searchBy) {
      case 'name':
        return 'Search by name...';
      case 'email':
        return 'Search by email...';
      case 'department':
        return 'Search by department...';
      default:
        return 'Search users...';
    }
  }

  onSearch(): void {
    this.currentPage = 1; // Reset to first page
    this.loadUsers();
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.currentPage = 1;
    this.loadUsers();
  }

  toggleSortOrder(): void {
    this.sortDescending = !this.sortDescending;
    this.currentPage = 1;
    this.loadUsers();
  }

  // Pagination methods
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page;
      this.loadUsers();
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
      // Show all pages if total is less than max visible
      for (let i = 1; i <= this.totalPages; i++) {
        pages.push(i);
      }
    } else {
      // Show pages around current page
      let startPage = Math.max(1, this.currentPage - 2);
      let endPage = Math.min(this.totalPages, this.currentPage + 2);
      
      // Adjust if we're near the beginning or end
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

  getActiveUsersCount(): number {
    return this.users.filter(user => user.status === 'active').length;
  }

  getInactiveUsersCount(): number {
    return this.users.filter(user => user.status === 'inactive').length;
  }

  editUser(user: UserViewModel): void {
    console.log('Edit user:', user);
    // Implement edit functionality - poate deschizi un modal sau navighezi la o pagină de edit
  }

  deleteUser(user: UserViewModel): void {
    if (confirm(`Are you sure you want to delete ${user.name}?`)) {
      this.usersService.deleteUser(user.id).subscribe({
        next: (response) => {
          console.log('User deleted successfully:', response);
          // Reload the current page to reflect changes
          this.loadUsers();
        },
        error: (err) => {
          console.error('Failed to delete user:', err);
          alert('Failed to delete user. Please try again.');
        }
      });
    }
  }

  restoreUser(user: UserViewModel): void {
    if (confirm(`Are you sure you want to restore ${user.name}?`)) {
      this.usersService.restoreUser(user.id).subscribe({
        next: () => {
          console.log('User restored successfully');
          // Reload the current page to reflect changes
          this.loadUsers();
        },
        error: (err) => {
          console.error('Failed to restore user:', err);
          alert('Failed to restore user. Please try again.');
        }
      });
    }
  }

  trackByUserId(index: number, user: UserViewModel): number {
    return user.id;
  }
}