import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { EmployeeManagerService } from '../../../../services/employeeManager/employee-manager';
import { UsersService } from '../../../../services/users/users-service';
import { IEmployeeManager } from '../../../../models/entities/iemployee-manager';
import { IApiResponse } from '../../../../models/responses/iapi-response';
import { IFilteredUsersRequest } from '../../../../models/requests/ifiltered-users-request';
import { IFilteredApiResponse } from '../../../../models/responses/ifiltered-api-response'; 
import { IUser } from '../../../../models/entities/iuser'; 

@Component({
  selector: 'app-admin-assign-relationship',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-assign-relationship.html',
  styleUrl: './admin-assign-relationship.css'
})
export class AdminAssignRelationship implements OnInit, OnDestroy {
  @Input() employee!: {
    id: number,
    name: string,
    email: string,
  }
  @Input() managers!: {
    id: number,
    firstName: string,  
    lastName: string,
    email: string,
    jobTitleName: string,
  }[];
  @Input() currentManagerIds: number[] = [];
  @Input() isUsedForPost: boolean = true;

  @Output() onAssign = new EventEmitter<number[]>();
  @Output() onClose = new EventEmitter<void>();

  selectedManagerIds: number[] = [];
  displayedManagers: any[] = [];
  allLoadedManagers: any[] = [];
  
  currentPage: number = 1;
  pageSize: number = 5;
  totalCount: number = 0;
  hasMoreData: boolean = true;
  isLoading: boolean = false;

  filterParams = {
    lastName: '',
    email: ''
  };

  private searchSubject = new Subject<void>();
  private destroy$ = new Subject<void>();

  constructor(
    private employeeManagerService: EmployeeManagerService,
    private usersService: UsersService
  ) {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.resetAndLoadManagers();
    });
  }

  ngOnInit() {
    this.selectedManagerIds = [...this.currentManagerIds];
    

    if (this.managers && this.managers.length > 0) {
      this.displayedManagers = [...this.managers];
      this.allLoadedManagers = [...this.managers];
      this.totalCount = this.managers.length;
      this.hasMoreData = false;
    } else {
      // Load managers from API
      this.loadManagers();
    }
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadManagers(reset: boolean = false) {
    if (this.isLoading) return;

    if (reset) {
      this.currentPage = 1;
      this.displayedManagers = [];
      this.allLoadedManagers = [];
      this.hasMoreData = true;
    }

    this.isLoading = true;

    const request: IFilteredUsersRequest = {
      lastName: this.filterParams.lastName || undefined,
      email: this.filterParams.email || undefined,
      params: {
        page: this.currentPage,
        pageSize: this.pageSize,
        sortBy: 'lastName',
        sortDescending: false
      }
    };

    this.usersService.getAllManagersFiltered(request).subscribe({
      next: (response: IApiResponse<IFilteredApiResponse<IUser>>) => {
        this.isLoading = false;
        
        if (response.success && response.data) {
          const newManagers = response.data.data || [];
          
          if (reset) {
            this.displayedManagers = newManagers;
            this.allLoadedManagers = newManagers;
          } else {
            const existingIds = new Set(this.allLoadedManagers.map(m => m.id));
            const uniqueNewManagers = newManagers.filter(m => !existingIds.has(m.id));
            
            this.displayedManagers = [...this.displayedManagers, ...uniqueNewManagers];
            this.allLoadedManagers = [...this.allLoadedManagers, ...uniqueNewManagers];
          }
          
          this.currentPage++;

          this.hasMoreData = response.data.hasNext;
        }
      },
      error: (error: any) => {
        this.isLoading = false;
        console.error('Error loading managers:', error);
      }
    });
  }

  loadMoreManagers() {
    if (!this.hasMoreData || this.isLoading) return;
    this.loadManagers();
  }

  resetAndLoadManagers() {
    this.loadManagers(true);
  }

  onFilterChange() {
    this.searchSubject.next();
  }

  clearFilters() {
    this.filterParams.lastName = '';
    this.filterParams.email = '';
    this.resetAndLoadManagers();
  }

  onCheckboxChange(event: Event, managerId: number): void {
  const target = event.target as HTMLInputElement;
  if (target) {
    this.onManagerToggle(managerId, target.checked);
  }
}

  onManagerToggle(managerId: number, isChecked: boolean) {
    if (isChecked) {
      if (!this.selectedManagerIds.includes(managerId)) {
        this.selectedManagerIds.push(managerId);
      }
    } else {
      this.selectedManagerIds = this.selectedManagerIds.filter(id => id !== managerId);
    }
  }

  isManagerSelected(managerId: number): boolean {
    return this.selectedManagerIds.includes(managerId);
  }

  getSelectedManagers() {
    return this.allLoadedManagers.filter(manager => 
      this.selectedManagerIds.includes(manager.id)
    );
  }

  trackByManagerId(index: number, manager: any): number {
    return manager.id;
  }

  onAssignClick() {
    if (this.isLoading) return;
    console.log('pressing button');
    if(this.isUsedForPost){
        this.employeeManagerService.addManagersForEmployee(this.employee.id, this.selectedManagerIds).subscribe({
        next: (response: IApiResponse<IEmployeeManager>) => {
          if (response.success) {
            console.log("Relation successfully created");
            this.onAssign.emit(this.selectedManagerIds);
          }
        },
        error: (error) => {
          console.error('Error assigning managers:', error);
        }
      });
    }
    else {
        this.employeeManagerService.patchManagersForEmployee(this.employee.id, this.selectedManagerIds).subscribe({
        next: (response: IApiResponse<IEmployeeManager>) => {
          if (response.success) {
            console.log("Relation successfully created");
            this.onAssign.emit(this.selectedManagerIds);
          }
        },
        error: (error) => {
          console.error('Error assigning managers:', error);
        }
      });
    }
  }

  onCloseClick() {
    this.onClose.emit();
  }

  getSelectedManagersCount(): number {
    return this.selectedManagerIds.length;
  }

  onOverlayClick(event: Event) {
    this.onClose.emit();
  }
}