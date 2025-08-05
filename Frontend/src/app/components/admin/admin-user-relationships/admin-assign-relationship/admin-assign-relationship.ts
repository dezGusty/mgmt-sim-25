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
import { HttpErrorResponse } from '@angular/common/http';

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
  @Input() isUsedForPost: boolean = true;

  @Output() onAssign = new EventEmitter<number[]>();
  @Output() onClose = new EventEmitter<void>();

  allManagers: {
    id: number,
    name: string,
    email: string,
    jobTitleName: string,
    subordinatesIds: number[]
  }[] = [];

  selectedManagersForCurrentUser: {
    id: number,
    name: string,
    jobTitleName: string
  }[] = [];
  
  totalCount: number = 0;
  isLoading: boolean = false;

  searchTerm: string = '';
  searchBy: 'name'| 'email' = 'name';

  private searchSubject = new Subject<void>();
  private destroy$ = new Subject<void>();

  constructor(private employeeManagerService: EmployeeManagerService,
              private usersService: UsersService) {
    
  }

  ngOnInit() {
    this.selectedManagersForCurrentUser = [];
    this.loadManagers();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadManagers(reset: boolean = false) {
    if (this.isLoading) return;

    this.isLoading = true;

    const request: IFilteredUsersRequest = {
      name: this.searchBy === 'name' ? this.searchTerm : undefined,
      email: this.searchBy === 'email' ? this.searchTerm : undefined,
      params: {
        sortBy: "lastName",
        sortDescending: false
      }
    };

    this.usersService.getAllManagersFiltered(request).subscribe({
      next: (response: IApiResponse<IFilteredApiResponse<IUser>>) => {
        console.log("All available maangers: ",response.data.data);
        this.allManagers = response.data.data.map(user => ({
          id: user.id,
          name: user.firstName + ' ' + user.lastName,
          email: user.email,
          jobTitleName: user.jobTitleName || 'Unknown job title',
          subordinatesIds: user.subordinatesIds || []
        }));


        this.allManagers.forEach(element => {
          if(element.subordinatesIds.includes(this.employee.id) && !this.selectedManagersForCurrentUser.map(manager => manager.id).includes(element.id)) {
            this.selectedManagersForCurrentUser.push({
              id: element.id,
              name: element.name,
              jobTitleName: element.jobTitleName
            });  
          }
        });

        this.allManagers.sort((a, b) => {
          const aIsSelected = this.selectedManagersForCurrentUser.some(sm => sm.id === a.id);
          const bIsSelected = this.selectedManagersForCurrentUser.some(sm => sm.id === b.id);
          
          if (aIsSelected && !bIsSelected) return -1;
          if (!aIsSelected && bIsSelected) return 1;
          return 0;
        });

        this.isLoading = false;
      }, 
      error: (err: HttpErrorResponse) => {
        this.isLoading = false;
      }
    });
  }

  resetAndLoadManagers() {
    this.loadManagers(true);
  }

  onFilterChange() {
    this.searchSubject.next();
  }

  clearFilters() {
    this.searchTerm = '';
    this.resetAndLoadManagers();
  }

  onCheckboxChange(event: Event, managerId: number,managerName: string, managerJobTitleName:string): void {
    const target = event.target as HTMLInputElement;
    if (target) {
      this.onManagerToggle(managerId, managerName, managerJobTitleName, target.checked);
    }
  }

  onManagerToggle(managerId: number, managerName: string, managerJobTitleName:string, isChecked: boolean) {
    if (isChecked) {
      if (!this.selectedManagersForCurrentUser.map(manager => manager.id).includes(managerId)) {
        this.selectedManagersForCurrentUser.push({
          id: managerId, 
          name: managerName,
          jobTitleName: managerJobTitleName
        });
      }
    } else {
      this.selectedManagersForCurrentUser = this.selectedManagersForCurrentUser.filter(manager => manager.id !== managerId);
    }
  }

  isManagerSelected(managerId: number): boolean {
    return this.selectedManagersForCurrentUser.map(manager => manager.id).includes(managerId);
  }

  onSearchKeypress(event: KeyboardEvent) {
    if (event.key === 'Enter') {
      this.loadManagers();
    }
  }

  getSelectedManagers() {
    return this.selectedManagersForCurrentUser;
  }

  trackByManagerId(index: number, manager: any): number {
    return manager.id;
  }

  onAssignClick() {
    if (this.isLoading) return;
    console.log('pressing button');
    if(this.isUsedForPost){
      this.employeeManagerService.addManagersForEmployee(this.employee.id, this.selectedManagersForCurrentUser.map(manager => manager.id)).subscribe({
      next: (response: IApiResponse<IEmployeeManager>) => {
        if (response.success) {
          console.log("Relation successfully created");
          this.onAssign.emit(this.selectedManagersForCurrentUser.map(manager => manager.id));
        }
      },
      error: (error) => {
        console.error('Error assigning managers:', error);
      }
    });
    }
    else {
      this.employeeManagerService.patchManagersForEmployee(this.employee.id, this.selectedManagersForCurrentUser.map(manager => manager.id)).subscribe({
      next: (response: IApiResponse<IEmployeeManager>) => {
        if (response.success) {
          console.log("Relation successfully created");
          this.onAssign.emit(this.selectedManagersForCurrentUser.map(manager => manager.id));
        }
      },
      error: (error) => {
        console.error('Error reassigning managers:', error);
      }
    });
    }
  }

  onCloseClick() {
    this.onClose.emit();
  }

  getSelectedManagersCount(): number {
    return this.selectedManagersForCurrentUser.length;
  }

  onOverlayClick(event: Event) {
    this.onClose.emit();
  }

  getSearchPlaceholder() : string {
    if(this.searchBy === 'email')
      return 'Search for managers by email...';
    else
      return 'Search for managers by name...';
  }
}