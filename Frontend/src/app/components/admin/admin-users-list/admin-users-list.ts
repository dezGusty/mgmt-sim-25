import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UsersService } from '../../../services/users/users-service';
import { IUserViewModel } from '../../../view-models/user-view-model';
import { IUser } from '../../../models/entities/iuser';
import { IFilteredUsersRequest } from '../../../models/requests/ifiltered-users-request';
import { IFilteredApiResponse } from '../../../models/responses/ifiltered-api-response';
import { IApiResponse } from '../../../models/responses/iapi-response';
import { IJobTitle } from '../../../models/entities/ijob-title';
import { JobTitlesService } from '../../../services/job-titles/job-titles-service';

@Component({
  selector: 'app-admin-users-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-users-list.html',
  styleUrl: './admin-users-list.css'
})
export class AdminUsersList implements OnInit {
  users: IUserViewModel[] = [];
  searchTerm: string = '';
  searchBy: string = 'name';
  sortDescending: boolean = false;
  
  currentPage: number = 1;
  itemsPerPage: number = 5;
  totalPages: number = 0;
  
  isLoading: boolean = false;

  showEditModal = false;
  userToEdit: IUserViewModel | null = null;

  editForm = {
    id: 0,
    firstName: '',
    lastName: '',
    email: '',
    jobTitleId: 0,
    dateOfEmployment: new Date(),
    leaveDaysLeftCurrentYear: 0,
    isAdmin: false,
    isManager: false,
    isEmployee: false
  };

  jobTitles: IJobTitle[] = [];
  isSubmitting: boolean = false;
  errorMessage: string = '';

  constructor(private usersService: UsersService, private jobTitlesService: JobTitlesService) { }

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
    console.log(this.searchBy);
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

  private mapToUserViewModel(user: IUser): IUserViewModel {
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
    this.currentPage = 1;
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

  getActiveUsersCount(): number {
    return this.users.filter(user => user.status === 'active').length;
  }

  getInactiveUsersCount(): number {
    return this.users.filter(user => user.status === 'inactive').length;
  }

  editUser(user: IUserViewModel): void {
    this.userToEdit = { ...user };
    this.populateEditForm(user);
    this.loadJobTitles();
    this.showEditModal = true;
  }

  populateEditForm(user: IUserViewModel): void {
    const nameParts = user.name.split(' ');
    this.editForm = {
      id: user.id,
      firstName: nameParts[0] || '',
      lastName: nameParts.slice(1).join(' ') || '',
      email: user.email,
      jobTitleId: user.jobTitle?.id || 0,
      dateOfEmployment: new Date(),
      leaveDaysLeftCurrentYear: 0,
      isAdmin: false,
      isManager: false,
      isEmployee: false
    };
    
    this.errorMessage = '';
  }

  loadJobTitles(): void {
    this.jobTitlesService.getAllJobTitles().subscribe({
      next: (response) => {
        if (response.success) {
          this.jobTitles = response.data || [];
        }
      },
      error: (error) => {
        console.error('Error loading job titles:', error);
      }
    });
  }

  onSubmitEdit(): void {
    if (!this.isFormValid()) {
      this.errorMessage = 'Please fill in all required fields.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const userToUpdate: IUser = {
      id: this.editForm.id,
      email: this.editForm.email,
      firstName: this.editForm.firstName,
      lastName: this.editForm.lastName,
      jobTitleId: this.editForm.jobTitleId,
      dateOfEmployment: this.editForm.dateOfEmployment,
      leaveDaysLeftCurrentYear: this.editForm.leaveDaysLeftCurrentYear,
      roles: this.getSelectedRoles(),
      isActive: true
    };

    this.usersService.updateUser(userToUpdate).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        if (response.success) {
          this.showEditModal = false;
          this.loadUsers();
          console.log('User updated successfully:', response.data);
        } else {
          this.errorMessage = response.message || 'Failed to update user';
        }
      },
      error: (error) => {
        this.isSubmitting = false;
        this.errorMessage = 'Error updating user: ' + (error.error?.message || error.message);
        console.error('Error updating user:', error);
      }
    });
  }

  getSelectedRoles(): string[] {
    const roles: string[] = [];
    if (this.editForm.isAdmin) roles.push('Admin');
    if (this.editForm.isManager) roles.push('Manager');
    if (this.editForm.isEmployee) roles.push('Employee');
    return roles;
  }

  isFormValid(): boolean {
    return !!(
      this.editForm.firstName.trim() &&
      this.editForm.lastName.trim() &&
      this.editForm.email.trim() &&
      this.editForm.jobTitleId > 0 &&
      (this.editForm.isAdmin || this.editForm.isManager || this.editForm.isEmployee)
    );
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.userToEdit = null;
    this.errorMessage = '';
  }

  deleteUser(user: IUserViewModel): void {
    if (confirm(`Are you sure you want to delete ${user.name}?`)) {
      this.usersService.deleteUser(user.id).subscribe({
        next: (response) => {
          console.log('User deleted successfully:', response);
          this.loadUsers();
        },
        error: (err) => {
          console.error('Failed to delete user:', err);
          alert('Failed to delete user. Please try again.');
        }
      });
    }
  }

  restoreUser(user: IUserViewModel): void {
    if (confirm(`Are you sure you want to restore ${user.name}?`)) {
      this.usersService.restoreUser(user.id).subscribe({
        next: () => {
          console.log('User restored successfully');
          this.loadUsers();
        },
        error: (err) => {
          console.error('Failed to restore user:', err);
          alert('Failed to restore user. Please try again.');
        }
      });
    }
  }

  trackByUserId(index: number, user: IUserViewModel): number {
    return user.id;
  }

  getDateForInput(): string {
    return this.editForm.dateOfEmployment.toISOString().split('T')[0];
  }

  setDateFromInput(dateString: string): void {
    this.editForm.dateOfEmployment = new Date(dateString);
  }
}