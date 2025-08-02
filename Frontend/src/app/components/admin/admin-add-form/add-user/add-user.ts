import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IJobTitleViewModel } from '../../../../view-models/job-title-view-model';
import { UsersService } from '../../../../services/users/users-service';
import { JobTitlesService } from '../../../../services/job-titles/job-titles-service';
import { IAddUser } from '../../../../models/entities/iuser';
import { IApiResponse } from '../../../../models/responses/iapi-response';
import { IUser } from '../../../../models/entities/iuser';
import { IDepartmentViewModel } from '../../../../view-models/department-view-model';
import { DepartmentService } from '../../../../services/departments/department-service';
import { IQueryParams } from '../../../../models/requests/iquery-params';
import { IFilteredDepartmentsRequest } from '../../../../models/requests/ifiltered-departments-request';

interface NotificationMessage {
  type: 'success' | 'error' | 'info';
  title: string;
  message: string;
  show: boolean;
}

@Component({
  selector: 'app-add-user',
  imports: [FormsModule, CommonModule],
  templateUrl: './add-user.html',
  styleUrl: './add-user.css'
})
export class AddUser {
  firstName: string = '';
  lastName: string = '';
  email: string = '';
  
  searchTextJobTitles: string = '';
  filteredJobTitles: IJobTitleViewModel[] = [];
  selectedJobTitleId: number = 0;
  selectedJobTitleName: string = '';
  isJobTitlesDropdownOpen: boolean = false;
  currentPageJobTitles: number = 1;
  pageSizeJobTitles: number = 10;
  canLoadMoreJobTitles: boolean = false;
  isLoadingJobTitles: boolean = false;

  searchTextDepartments: string = '';
  filteredDepartments: IDepartmentViewModel[] = [];
  selectedDepartmentId: number = 0;
  selectedDepartmentName: string = '';
  isDepartmentsDropdownOpen: boolean = false;
  currentPageDepartment: number = 1;
  pageSizeDepartments: number = 10;
  canLoadMoreDepartments: boolean = false;
  isLoadingDepartments: boolean = false;


  readonly maxDate: string;
  dateOfEmployment: Date = new Date();
  leaveDaysLeftCurrentYear: number = 0;
  isAdmin = false;
  isManager = false;
  
  employeeRoleInfo: string = 'All the users are automatically set to employees.';
  
  isSubmitting: boolean = false;

  onSubmitMessage: string = '';

  constructor(
    private userService: UsersService,
    private jobTitleService: JobTitlesService,
    private departmentService: DepartmentService
  ) {
    this.maxDate = new Date().toISOString().split('T')[0];
  }

  resetForm(): void {
    this.firstName = '';
    this.lastName = '';
    this.email = '';
    this.searchTextJobTitles = '';
    this.searchTextDepartments = '';
    this.selectedJobTitleId = 0;
    this.selectedDepartmentId = 0;
    this.selectedJobTitleName = '';
    this.selectedDepartmentName = '';
    this.dateOfEmployment = new Date();
    this.leaveDaysLeftCurrentYear = 0;
    this.isAdmin = false;
    this.isManager = false;
    this.closeDropdownJobTitles();
    this.closeDropdownDepartments();
  }

  private loadDepartments(resetPage: boolean = true) : void {
    if (resetPage) {
          this.currentPageJobTitles = 1;
          this.filteredDepartments = [];
        }
        this.isLoadingJobTitles = true;
        
        const params: IFilteredDepartmentsRequest = {
          name: this.searchTextDepartments.trim() || undefined,
          params: {
            page: this.currentPageDepartment,
            pageSize: this.pageSizeDepartments,
            sortBy: 'name',
            sortDescending: false
          }
        };
        this.departmentService.getAllDepartmentsFiltered(params).subscribe({
          next: (response) => {
            console.log('API Response:', response);
            
            if (resetPage) {
              this.filteredDepartments = response.data.data;
            } else {
              this.filteredDepartments = [...this.filteredDepartments, ...response.data.data];
            }
            
            this.canLoadMoreDepartments = response.data.hasNext || 
                              (response.data.totalPages && this.currentPageDepartment < response.data.totalPages) || false;
            
            this.isLoadingDepartments = false;
            this.isDepartmentsDropdownOpen = this.filteredDepartments.length > 0;
          },
          error: (error) => {
            console.error('Error loading deparments:', error);
            this.isLoadingDepartments = false;
            this.canLoadMoreDepartments = false;
            this.onSubmitMessage = 'Error retrieving the departments.';
          }
        });
  }

  private loadJobTitles(resetPage: boolean = true): void {
    if (resetPage) {
      this.currentPageJobTitles = 1;
      this.filteredJobTitles = [];
    }
    this.isLoadingJobTitles = true;
    
    const params = {
      jobTitleName: this.searchTextJobTitles.trim() || undefined,
      params: {
        page: this.currentPageJobTitles,
        pageSize: this.pageSizeJobTitles,
        sortBy: 'name',
        sortDescending: false
      }
    };
    this.jobTitleService.getAllJobTitlesFiltered(params).subscribe({
      next: (response) => {
        console.log('API Response:', response);
        
        if (resetPage) {
          this.filteredJobTitles = response.data.data;
        } else {
          this.filteredJobTitles = [...this.filteredJobTitles, ...response.data.data];
        }
        
        this.canLoadMoreJobTitles = response.data.hasNext || 
                          (response.data.totalPages && this.currentPageJobTitles < response.data.totalPages) || false;
        
        this.isLoadingJobTitles = false;
        this.isJobTitlesDropdownOpen = this.filteredJobTitles.length > 0;
      },
      error: (error) => {
        console.error('Error loading job titles:', error);
        this.isLoadingJobTitles = false;
        this.canLoadMoreJobTitles = false;
        this.onSubmitMessage = 'Error retrieving the job titles.';
      }
    });
  }

  isFieldInvalid(field: any): boolean {
    return field.invalid && (field.dirty || field.touched);
  }

  isJobTitleFieldValid(): boolean {
    return this.selectedJobTitleId > 0;
  }

  isDepartmentFieldValid(): boolean {
    return this.selectedDepartmentId > 0;
  }

  onSearchTextChangeJobTitles(searchValue: string): void {
    this.searchTextJobTitles = searchValue;
    this.selectedJobTitleId = 0; 
    this.selectedJobTitleName = '';
    
    this.loadJobTitles(true);
  }

  
  onSearchTextChangeDepartments(searchValue: string): void {
    this.searchTextDepartments = searchValue;
    this.selectedDepartmentId = 0; 
    this.selectedDepartmentName = '';
    
    this.loadDepartments(true);
  }

  openDropdownJobTitles(): void {
    if (!this.isJobTitlesDropdownOpen) {
      this.isJobTitlesDropdownOpen = true;
      if (this.filteredJobTitles.length === 0) {
        this.loadJobTitles();
      }
    }
  }

  closeDropdownJobTitles(): void {
    this.isJobTitlesDropdownOpen = false;
  }

  openDropdownDepartments(): void {
    if (!this.isDepartmentsDropdownOpen) {
      this.isDepartmentsDropdownOpen = true;
      if (this.filteredDepartments.length === 0) {
        this.loadDepartments();
      }
    }
  }

  closeDropdownDepartments(): void {
    this.isDepartmentsDropdownOpen = false;
  }

  selectJobTitle(jobTitle: IJobTitleViewModel): void {
    this.selectedJobTitleId = jobTitle.id;
    this.selectedJobTitleName = jobTitle.name || '';
    this.searchTextJobTitles = jobTitle.name || '';
    this.closeDropdownJobTitles();
  }

  selectDepartment(deparment: IDepartmentViewModel  ): void {
    this.selectedDepartmentId = deparment.id;
    this.selectedDepartmentName = deparment.name || '';
    this.searchTextDepartments = deparment.name || '';
    this.closeDropdownDepartments();
  }

  loadMoreJobTitles(): void {
    if (this.canLoadMoreJobTitles && !this.isLoadingJobTitles) {
      console.log('Loading more - current page:', this.currentPageJobTitles);
      this.currentPageJobTitles++;
      this.loadJobTitles(false);
    } else {
      console.log('Cannot load more. canLoadMore:', this.canLoadMoreJobTitles, 'isLoading:', this.isLoadingJobTitles);
    }
  }

  loadMoreDepartments() : void {
    if (this.canLoadMoreDepartments && !this.isLoadingDepartments) {
      console.log('Loading more - current page:', this.currentPageDepartment);
      this.currentPageDepartment++;
      this.loadDepartments(false);
    } else {
      console.log('Cannot load more. canLoadMore:', this.canLoadMoreJobTitles, 'isLoading:', this.isLoadingJobTitles);
    }
  }

  onClickOutside(event: Event): void {
    this.closeDropdownJobTitles();
    this.closeDropdownDepartments();
  }

  highlightSearchText(text: string | undefined, searchText: string): string {
    if (!text || !searchText.trim()) {
      return text || '';
    }
    
    const regex = new RegExp(`(${searchText})`, 'gi');
    return text.replace(regex, '<strong class="bg-yellow-200">$1</strong>');
  }

  getDateOfEmploymentForInput(): string {
    return this.dateOfEmployment.toISOString().split('T')[0];
  }

  setDateFromInput(dateString: string): void {
    if (dateString) {
      this.dateOfEmployment = new Date(dateString);
    }
  }

  submit(form: any): void {
    if (!form.valid || !this.isJobTitleFieldValid()) {
      this.onSubmitMessage = 'Please fill all the required fields.';
      return;
    }

    this.isSubmitting = true;

    let roles: number[] = [];
    if (this.isAdmin) roles.push(3);
    if (this.isManager) roles.push(2);
    
    const userToAdd: IAddUser = {
      id: 0,
      firstName: this.firstName,
      lastName: this.lastName,
      email: this.email,
      jobTitleId: this.selectedJobTitleId,
      departmentId: this.selectedDepartmentId,
      employeeRolesId: roles,
      dateOfEmployment: this.dateOfEmployment
    };

    this.userService.addUser(userToAdd).subscribe({
      next: (response: IApiResponse<IUser>) => {
        this.isSubmitting = false;
        console.log('User added successfully:', response);
        
        if (response.success) {
          this.onSubmitMessage = 'User added successfully.'
          this.resetForm();
          form.resetForm();
        } else {
          this.onSubmitMessage = 'Error adding user:' + response.message;
        }
      },
      error: (error) => {
        this.isSubmitting = false;
        console.error('Error adding user:', error);
        
        this.onSubmitMessage = 'An error occured during adding a new user';
        
        if (error.status === 400) {
          this.onSubmitMessage = 'Fill all the mandatory fields!';
        } else if (error.status === 409) {
          this.onSubmitMessage = 'This mail is already used.';
        } else if (error.status === 500) {
          this.onSubmitMessage = 'Server error, try later.';
        } else if (error.error && error.error.message) {
          this.onSubmitMessage = error.error.message;
        }
      }
    });
  }
}