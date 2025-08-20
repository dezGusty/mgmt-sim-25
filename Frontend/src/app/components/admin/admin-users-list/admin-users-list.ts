import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UsersService } from '../../../services/users/users-service';
import { IUserViewModel } from '../../../view-models/user-view-model';
import { IAddUser, IUpdateUser, IUser } from '../../../models/entities/iuser';
import { IFilteredUsersRequest } from '../../../models/requests/ifiltered-users-request';
import { IFilteredApiResponse } from '../../../models/responses/ifiltered-api-response';
import { IApiResponse } from '../../../models/responses/iapi-response';
import { IJobTitle } from '../../../models/entities/ijob-title';
import { JobTitlesService } from '../../../services/job-titles/job-titles-service';
import { EmployeeRolesService } from '../../../services/employee-roles/employee-roles';
import { IEmployeeManager } from '../../../models/entities/iemployee-manager';
import { IEmployeeRole } from '../../../models/entities/iemployee-role';
import { IDepartment } from '../../../models/entities/idepartment';
import { DepartmentService } from '../../../services/departments/department-service';
import { UserActivityStatus } from '../../../models/enums/user-activity-status';

@Component({
  selector: 'app-admin-users-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-users-list.html',
  styleUrl: './admin-users-list.css'
})
export class AdminUsersList implements OnInit {
  users: IUserViewModel[] = [];
  searchTerm: string = '';
  searchBy: string = 'globalSearch';
  searchByActivityStatus: 'activeStatus' | 'inactiveStatus' | 'global' = 'global';
  sortDescending: boolean = false;
  userRoles: Map<string, number> = new Map();

  currentPage: number = 1;
  itemsPerPage: number = 5;
  totalPages: number = 0;

  isLoading: boolean = false;
  hasError: boolean = false;
  errorMessage: string = '';
  hasSearched: boolean = false;

  showEditModal = false;
  userToEdit: IUserViewModel | null = null;

  editForm = {
    id: 0,
    firstName: '',
    lastName: '',
    email: '',
    jobTitleId: 0,
    departmentId: 0,
    dateOfEmployment: new Date(),
    isAdmin: false,
    isManager: false,
    isHr: false,
  };

  jobTitles: IJobTitle[] = [];
  departments: IDepartment[] = [];
  isSubmitting: boolean = false;
  editErrorMessage: string = '';

  employeeRoleInfo: string = 'All the users are automatically set to employees.';

  constructor(private usersService: UsersService,
    private jobTitlesService: JobTitlesService,
    private employeeRoleService: EmployeeRolesService,
    private departmentService: DepartmentService) {

  }

  ngOnInit(): void {
    this.loadUsers();
    this.loadUserRoles();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.hasError = false;
    this.errorMessage = '';
    console.log(this.searchByActivityStatus);
    const filterRequest: IFilteredUsersRequest = {
      name: this.searchBy === 'name' ? this.searchTerm : undefined,
      email: this.searchBy === 'email' ? this.searchTerm : undefined,
      department: this.searchBy === 'department' ? this.searchTerm : undefined,
      jobTitle: this.searchBy === 'jobTitle' ? this.searchTerm : undefined,
      globalSearch: this.searchBy === 'globalSearch' ? this.searchTerm : undefined,
      status: this.searchByActivityStatus === 'activeStatus' ? UserActivityStatus.ACTIVE : this.searchByActivityStatus === 'inactiveStatus'
        ? UserActivityStatus.INACTIVE : UserActivityStatus.ALL,
      params: {
        sortBy: this.getSortField(),
        sortDescending: this.sortDescending,
        page: this.currentPage,
        pageSize: this.itemsPerPage
      }
    };

    console.log('Search parameters:', this.searchBy, this.searchTerm);

    this.usersService.getAllUsersFiltered(filterRequest).subscribe({
      next: (response: IApiResponse<IFilteredApiResponse<IUser>>) => {
        console.log('API response:', response);
        this.isLoading = false;

        if (response.success && response.data) {
          const rawUsers: IUser[] = response.data.data || [];
          this.users = rawUsers.map(user => this.mapToUserViewModel(user));
          this.totalPages = response.data.totalPages || 0;

          if (this.users.length === 0) {
            if (this.searchTerm.trim()) {
              this.hasError = true;
              this.errorMessage = this.getNoResultsMessage();
            } else if (this.hasSearched) {
              this.hasError = true;
              this.errorMessage = 'No users found in the system.';
            }

          } else {
            this.hasError = false;
            this.errorMessage = '';
          }
        } else {
          this.handleError(response.message || 'Failed to load users. Please try again.');
        }
      },
      error: (err) => {
        console.error('Failed to fetch users:', err);
        this.isLoading = false;
        this.handleError(this.getErrorMessage(err));
      }
    });
  }

  loadUserRoles() {
    this.employeeRoleService.getAllEmployeeRoles().subscribe({
      next: (response: IApiResponse<IEmployeeRole[]>) => {
        response.data.forEach(er => {
          console.log(`rolename :${er.rolename}, ${er.id}`);
          this.userRoles.set(er.rolename, er.id);
        });
      },
      error: (error) => {
        console.error('Error loading user roles:', error);
      }
    });
  }

  private handleError(message: string): void {
    this.hasError = true;
    this.errorMessage = message;
    this.users = [];
    this.totalPages = 0;
  }

  private getErrorMessage(error: any): string {
    if (error.status === 0) {
      return 'Unable to connect to the server. Please check your internet connection.';
    } else if (error.status >= 500) {
      return 'Server error occurred. Please try again later.';
    } else if (error.status === 404) {
      return 'The requested resource was not found.';
    } else if (error.status === 403) {
      return 'You do not have permission to view this data.';
    } else if (error.error?.message) {
      return error.error.message;
    } else if (error.message) {
      return error.message;
    } else {
      return 'An unexpected error occurred while loading users.';
    }
  }

  private getNoResultsMessage(): string {
    if (this.searchTerm.trim()) {
      return `No users found matching "${this.searchTerm}". Try adjusting your search criteria.`;
    } else {
      return 'No users found. Try adjusting your search criteria.';
    }
  }

  private getSortField(): string {
    switch (this.searchBy) {
      case 'name':
        return 'lastName';
      case 'email':
        return 'email';
      case 'department':
        return 'departmentName';
      case 'jobTitle':
        return 'jobTitleName';
      case 'globalSearch':
        return 'lastName';
      default:
        return 'lastName';
    }
  }

  private mapToUserViewModel(user: IUser): IUserViewModel {
    return {
      id: user.id,
      name: `${user.firstName} ${user.lastName}`,
      email: user.email,
      department: {
        id: user.departmentId || 0,
        name: user.departmentName || 'Unknown'
      },
      jobTitle: user.jobTitleId ? {
        id: user.jobTitleId,
        name: user.jobTitleName || 'Unknown',
      } : undefined,
      roles: user.roles,
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
      case 'jobTitle':
        return 'Search by job title...';
      case 'globalSearch':
        return 'Search across all fields...';
      default:
        return 'Search users...';
    }
  }

  onSearch(): void {
    this.currentPage = 1;
    this.hasSearched = true;
    this.loadUsers();
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.currentPage = 1;
    this.hasSearched = false;
    this.hasError = false;
    this.errorMessage = '';
    this.loadUsers();
  }

  toggleSortOrder(): void {
    this.sortDescending = !this.sortDescending;
    this.currentPage = 1;
    this.loadUsers();
  }

  retryLoad(): void {
    this.hasError = false;
    this.errorMessage = '';
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
    this.loadDepartments();
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
      departmentId: user.department?.id || 0,
      dateOfEmployment: new Date(),
      isAdmin: user.roles?.includes("Admin") ? true : false,
      isManager: user.roles?.includes("Manager") ? true : false,
      isHr: user.roles?.includes("HR") ? true : false,
    };

    this.editErrorMessage = '';
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

  loadDepartments(): void {
    this.departmentService.getAllDepartments().subscribe({
      next: (response) => {
        if (response.success) {
          this.departments = response.data || [];
        }
      },
      error: (error) => {
        console.error('Error loading departments:', error);
      }
    });
  }

  onSubmitEdit(): void {
    if (!this.isFormValid()) {
      this.editErrorMessage = 'Please fill in all required fields.';
      return;
    }

    this.isSubmitting = true;
    this.editErrorMessage = '';

    const selectedRoles = this.getSelectedRoles();
    const roleIds = selectedRoles.map(rolename => this.userRoles.get(rolename)).filter(id => id !== undefined && id > 0) as number[];

    console.log('Selected roles:', selectedRoles);
    console.log('Role IDs to send:', roleIds);

    const userToUpdate: IUpdateUser = {
      id: this.editForm.id,
      email: this.editForm.email,
      firstName: this.editForm.firstName,
      lastName: this.editForm.lastName,
      jobTitleId: this.editForm.jobTitleId,
      dateOfEmployment: this.editForm.dateOfEmployment,
      employeeRolesId: roleIds,
    };

    this.usersService.updateUser(userToUpdate).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        if (response.success) {
          this.showEditModal = false;
          // Resetăm formularul și reîncărcăm datele
          this.resetForm();
          // Forțăm reîncărcarea completă a datelor
          this.currentPage = 1;
          this.loadUsers();
          console.log('User updated successfully:', response.data);
        } else {
          this.editErrorMessage = response.message || 'Failed to update user';
        }
      },
      error: (error) => {
        this.isSubmitting = false;
        this.editErrorMessage = 'Error updating user: ' + (error.error?.message || error.message);
        console.error('Error updating user:', error);
      }
    });
  }

  private resetForm(): void {
    this.editForm = {
      id: 0,
      firstName: '',
      lastName: '',
      email: '',
      jobTitleId: 0,
      departmentId: 0,
      dateOfEmployment: new Date(),
      isAdmin: false,
      isManager: false,
      isHr: false,
    };
  }

  getSelectedRoles(): string[] {
    const roles: string[] = [];
    if (this.editForm.isAdmin) roles.push('Admin');
    if (this.editForm.isManager) roles.push('Manager');
    if (this.editForm.isHr) roles.push('HR');
    return roles;
  }

  isFormValid(): boolean {
    return !!(
      this.editForm.firstName.trim() &&
      this.editForm.lastName.trim() &&
      this.editForm.email.trim() &&
      this.editForm.jobTitleId > 0
    );
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.userToEdit = null;
    this.editErrorMessage = '';
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

  onSearchKeypress(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.onSearch();
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