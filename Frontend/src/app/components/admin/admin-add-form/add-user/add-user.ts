// add-user.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IJobTitleViewModel } from '../../../../view-models/job-title-view-model';
import { UsersService } from '../../../../services/users/users-service';
import { OnInit } from '@angular/core';
import { JobTitlesService } from '../../../../services/job-titles/job-titles-service';
import { IAddUser } from '../../../../models/entities/iuser';
import { IApiResponse } from '../../../../models/responses/iapi-response';
import { IUser } from '../../../../models/entities/iuser';

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
export class AddUser implements OnInit {
  firstName: string = '';
  lastName: string = '';
  email: string = '';
  
  searchText: string = '';
  filteredJobTitles: IJobTitleViewModel[] = [];
  selectedJobTitleId: number = 0;
  selectedJobTitleName: string = '';
  isDropdownOpen: boolean = false;
  currentPage: number = 1;
  pageSize: number = 5;
  canLoadMore: boolean = false;
  isLoading: boolean = false;
  readonly maxDate: string;
  dateOfEmployment: Date = new Date();
  leaveDaysLeftCurrentYear: number = 0;
  readonly baseLeaveDays: number = 21; 
  maxLeaveDays: number = 21;
  isAdmin = false;
  isManager = false;
  isEmployee = false;
  
  // Notification system
  notification: NotificationMessage = {
    type: 'info',
    title: '',
    message: '',
    show: false
  };
  
  isSubmitting: boolean = false;

  constructor(
    private userService: UsersService,
    private jobTitleService: JobTitlesService
  ) {
    this.maxDate = new Date().toISOString().split('T')[0];
  }

  ngOnInit(): void {
  }

  showNotification(type: 'success' | 'error' | 'info', title: string, message: string): void {
    this.notification = { type, title, message, show: true };
    
    if (type === 'success') {
      setTimeout(() => {
        this.hideNotification();
      }, 5000);
    }
  }

  hideNotification(): void {
    this.notification.show = false;
  }

  resetForm(): void {
    this.firstName = '';
    this.lastName = '';
    this.email = '';
    this.searchText = '';
    this.selectedJobTitleId = 0;
    this.selectedJobTitleName = '';
    this.dateOfEmployment = new Date();
    this.leaveDaysLeftCurrentYear = 0;
    this.isAdmin = false;
    this.isManager = false;
    this.isEmployee = false;
    this.filteredJobTitles = [];
    this.closeDropdown();
  }

  private loadJobTitles(resetPage: boolean = true): void {
    if (resetPage) {
      this.currentPage = 1;
      this.filteredJobTitles = [];
    }
    this.isLoading = true;
    
    const params = {
      jobTitleName: this.searchText.trim() || undefined,
      params: {
        page: this.currentPage,
        pageSize: this.pageSize,
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
        
        this.canLoadMore = response.data.hasNext || 
                          (response.data.totalPages && this.currentPage < response.data.totalPages) || false;
        
        this.isLoading = false;
        this.isDropdownOpen = this.filteredJobTitles.length > 0;
      },
      error: (error) => {
        console.error('Error loading job titles:', error);
        this.isLoading = false;
        this.canLoadMore = false;
        this.showNotification('error', 'Eroare', 'Nu s-au putut încărca job title-urile. Vă rugăm să încercați din nou.');
      }
    });
  }

  isFieldInvalid(field: any): boolean {
    return field.invalid && (field.dirty || field.touched);
  }

  isJobTitleFieldValid(): boolean {
    return this.selectedJobTitleId > 0;
  }

  isRoleFieldValid(): boolean {
    return this.isAdmin || this.isManager || this.isEmployee;
  }

  onSearchTextChange(searchValue: string): void {
    this.searchText = searchValue;
    this.selectedJobTitleId = 0; 
    this.selectedJobTitleName = '';
    
    this.loadJobTitles(true);
  }

  openDropdown(): void {
    if (!this.isDropdownOpen) {
      this.isDropdownOpen = true;
      if (this.filteredJobTitles.length === 0) {
        this.loadJobTitles();
      }
    }
  }

  closeDropdown(): void {
    this.isDropdownOpen = false;
  }

  selectJobTitle(jobTitle: IJobTitleViewModel): void {
    this.selectedJobTitleId = jobTitle.id;
    this.selectedJobTitleName = jobTitle.name || '';
    this.searchText = jobTitle.name || '';
    this.closeDropdown();
  }

  loadMore(): void {
    if (this.canLoadMore && !this.isLoading) {
      console.log('Loading more - current page:', this.currentPage);
      this.currentPage++;
      this.loadJobTitles(false);
    } else {
      console.log('Cannot load more. canLoadMore:', this.canLoadMore, 'isLoading:', this.isLoading);
    }
  }

  onClickOutside(event: Event): void {
    this.closeDropdown();
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
      this.calculateMaxLeaveDays();
    }
  }

  private calculateMaxLeaveDays(): void {
    const yearsOfExperience = this.calculateYearsOfExperience();
    
    const bonusYearPeriods = Math.floor(yearsOfExperience / 10);
    const bonusDays = bonusYearPeriods * 4;
    
    this.maxLeaveDays = this.baseLeaveDays + bonusDays;
    
    console.log(`Years of experience: ${yearsOfExperience}, Bonus periods: ${bonusYearPeriods}, Max leave days: ${this.maxLeaveDays}`);
  }

  private calculateYearsOfExperience(): number {
    if (!this.dateOfEmployment) {
      return 0;
    }
    const currentDate = new Date();
    const employmentDate = new Date(this.dateOfEmployment);
    
    let years = currentDate.getFullYear() - employmentDate.getFullYear();
    
    const currentMonthDay = currentDate.getMonth() * 100 + currentDate.getDate();
    const employmentMonthDay = employmentDate.getMonth() * 100 + employmentDate.getDate();
    
    if (currentMonthDay < employmentMonthDay) {
      years--;
    }
    
    return Math.max(0, years);
  }

  getYearsOfExperienceDisplay(): string {
    const years = this.calculateYearsOfExperience();
    return years === 1 ? '1 year' : `${years} years`;
  }

  getBonusLeaveDaysDisplay(): string {
    const years = this.calculateYearsOfExperience();
    const bonusYearPeriods = Math.floor(years / 10);
    const bonusDays = bonusYearPeriods * 4;
    
    if (bonusDays === 0) {
      return '';
    }
    
    return `(+${bonusDays} bonus days for ${bonusYearPeriods * 10}+ years of service)`;
  }

  submit(form: any): void {
    if (!form.valid || !this.isJobTitleFieldValid() || !this.isRoleFieldValid()) {
      this.showNotification('error', 'Invalid form', 'Please fill all the mandatory fields!');
      return;
    }

    this.isSubmitting = true;
    this.hideNotification();

    let roles: number[] = [];
    if (this.isAdmin) roles.push(3);
    if (this.isManager) roles.push(2);
    if (this.isEmployee) roles.push(1);
    
    const userToAdd: IAddUser = {
      id: 0,
      firstName: this.firstName,
      lastName: this.lastName,
      email: this.email,
      jobTitleId: this.selectedJobTitleId,
      employeeRolesIds: roles,
      leaveDaysLeftCurrentYear: this.leaveDaysLeftCurrentYear,
      dateOfEmployment: this.dateOfEmployment
    };

    this.userService.addUser(userToAdd).subscribe({
      next: (response: IApiResponse<IUser>) => {
        this.isSubmitting = false;
        console.log('User added successfully:', response);
        
        if (response.success) {
          this.showNotification(
            'success',
            response.message,
            `${this.firstName} ${this.lastName} added in our system with email: ${this.email}.`
          );
          
          this.resetForm();
          form.resetForm();
        } else {
          this.showNotification(
            'error',
            'Error during adding a new user',
            response.message || 'Unexpected error'
          );
        }
      },
      error: (error) => {
        this.isSubmitting = false;
        console.error('Error adding user:', error);
        
        let errorMessage = 'An error occured during adding a new user';
        
        if (error.status === 400) {
          errorMessage = 'Fill all the mandatory fields!';
        } else if (error.status === 409) {
          errorMessage = 'This mail is already used.';
        } else if (error.status === 500) {
          errorMessage = 'Server error, try later.';
        } else if (error.error && error.error.message) {
          errorMessage = error.error.message;
        }
        
        this.showNotification('error', 'Error', errorMessage);
      }
    });
  }
}