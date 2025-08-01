import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LeaveRequestTypeService } from '../../../services/leaveRequestType/leave-request-type-service';
import { ILeaveRequestType } from '../../../models/entities/ileave-request-type';
import { ILeaveRequestTypeViewModel } from '../../../view-models/leave-request-type-view-model';
import { ColorGenerator } from '../../../services/colorGenerator/color-generator';
import { IFilteredLeaveRequestTypeRequest } from '../../../models/requests/ifiltered-leave-request-types-request';
import { IApiResponse } from '../../../models/responses/iapi-response';
import { IFilteredApiResponse } from '../../../models/responses/ifiltered-api-response';
import { max } from 'rxjs';

@Component({
  selector: 'app-admin-leave-request-types-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-leave-request-types-list.html',
  styleUrl: './admin-leave-request-types-list.css'
})
export class AdminLeaveTypesList implements OnInit {
  leaveRequestTypes: ILeaveRequestTypeViewModel[] = [];
  searchTerm: string = '';

  showEditModal = false;
  leaveTypeToEdit: ILeaveRequestTypeViewModel | null = null;
  
  editForm = {
    id: 0,
    description: '',
    title: '',
    isPaid: false,
    maxDays: 0
  };
  
  isSubmitting = false;
  errorMessage = '';

  constructor(
    private leaveRequestTypeService: LeaveRequestTypeService,
    private colorGenerator: ColorGenerator
  ) {}

  ngOnInit(): void {
    this.loadLeaveTypes();
  }

  isLoading: boolean = false;

  loadLeaveTypes(): void {
    this.isLoading = true;
    this.errorMessage = '';
    
    let params: IFilteredLeaveRequestTypeRequest = {
      name: this.searchTerm,
      params: {
        page: 1,
        pageSize: 100,
        sortBy: "id",
        sortDescending: false
      }
    };
    
    this.leaveRequestTypeService.getAllLeaveTypesFiltered(params).subscribe({
      next: (response: IApiResponse<IFilteredApiResponse<ILeaveRequestType>>) => {
        console.log('API response:', response);
        
        if (response.success) {
          const rawLeaveTypes: ILeaveRequestType[] = response.data.data || [];
          this.leaveRequestTypes = rawLeaveTypes.map(leaveType => 
            this.mapToLeaveTypeViewModel(leaveType)
          );
          console.log('Mapped Leave Types:', this.leaveRequestTypes);
        } else {
          this.errorMessage = response.message || 'Could not load all the leave request types.';
          this.leaveRequestTypes = [];
        }
        
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to fetch leave request types:', err);
        this.isLoading = false;
        
        if (err.status >= 400 && err.status < 500) {
          this.errorMessage = err.error?.message || 'Error processing the request.';
        } else if (err.status >= 500) {
          this.errorMessage = 'Server error, try again later.';
        } else {
          this.errorMessage = 'Unexpected error happened.';
        }
        
        this.leaveRequestTypes = [];
      }
    });
  }
  
  private mapToLeaveTypeViewModel(leaveType: ILeaveRequestType): ILeaveRequestTypeViewModel {
    return {
      id: leaveType.id,
      title: leaveType.title || '',
      description: leaveType.description || '',
      isPaid: leaveType.isPaid,
      maxDays: leaveType.maxDays || 0,
      color: this.colorGenerator.generateColorFromId(leaveType.id)
    };
  }

  get filteredLeaveTypes(): ILeaveRequestTypeViewModel[] {
    return this.leaveRequestTypes;
  }

  trackByLeaveType(index: number, leaveType: ILeaveRequestTypeViewModel): string {
    return `${leaveType.id}-${leaveType.color}`;
  }

  editLeaveType(leaveType: ILeaveRequestTypeViewModel): void {
    this.leaveTypeToEdit = { ...leaveType };
    this.populateEditForm(leaveType);
    this.showEditModal = true;
  }

  populateEditForm(leaveType: ILeaveRequestTypeViewModel): void {
    this.editForm = {
      id: leaveType.id,
      description: leaveType.description || '',
      title: leaveType.title || '',
      isPaid: leaveType.isPaid || false,
      maxDays: leaveType.maxDays || 0
    };
    this.errorMessage = '';
  }

  onSubmitEdit(): void {
    if (!this.isFormValid()) {
      this.errorMessage = 'Please fill in all required fields.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const leaveTypeToUpdate: ILeaveRequestType = {
      id: this.editForm.id,
      description: this.editForm.description,
      title: this.editForm.title,
      isPaid: this.editForm.isPaid,
      maxDays: this.editForm.maxDays
    };

    this.leaveRequestTypeService.updateLeaveRequestType(leaveTypeToUpdate).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        if (response.success) {
          this.showEditModal = false;
          this.loadLeaveTypes();
          console.log('Leave request type updated successfully:', response.data);
        } else {
          this.errorMessage = response.message || 'Failed to update leave request type';
        }
      },
      error: (error) => {
        this.isSubmitting = false;
        this.errorMessage = 'Error updating leave request type: ' + (error.error?.message || error.message);
        console.error('Error updating leave request type:', error);
      }
    });
  }

  isFormValid(): boolean {
    return !!(
    this.editForm.description.trim() && 
    this.editForm.title.trim() &&
    this.editForm.maxDays > 0
  );
  }

  onSearch() {
    console.log('Search term:', this.searchTerm);
    this.loadLeaveTypes();
  }

  clearSearch() {
    this.searchTerm = '';
    this.loadLeaveTypes();
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.leaveTypeToEdit = null;
    this.errorMessage = '';
  }
  

  deleteLeaveType(leaveType: ILeaveRequestTypeViewModel): void {
    const confirmMessage = `Are you sure you want to delete the leave request type "${leaveType.description}"?`;
    
    if (confirm(confirmMessage)) {
      this.leaveRequestTypeService.deleteLeaveRequestType(leaveType.id).subscribe({
        next: (response) => {
          if (response.success) {
            console.log('Leave request type deleted successfully');
            this.loadLeaveTypes(); // Reload the list
          } else {
            alert('Failed to delete leave request type: ' + (response.message || 'Unknown error'));
          }
        },
        error: (error) => {
          console.error('Error deleting leave request type:', error);
          alert('Error deleting leave request type: ' + (error.error?.message || error.message));
        }
      });
    }
  }
}