import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LeaveRequestTypeService } from '../../../services/leaveRequestType/leave-request-type-service';
import { ILeaveRequestType } from '../../../models/entities/ileave-request-type';
import { ILeaveRequestTypeViewModel } from '../../../view-models/leave-request-type-view-model';
import { ColorGenerator } from '../../../services/colorGenerator/color-generator';

@Component({
  selector: 'app-admin-leave-request-types-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-leave-request-types-list.html',
  styleUrl: './admin-leave-request-types-list.css'
})
export class AdminLeaveTypesList implements OnInit {
  leaveRequestTypes: ILeaveRequestTypeViewModel[] = [];
  searchTerm: string = '';

  // Edit modal properties
  showEditModal = false;
  leaveTypeToEdit: ILeaveRequestTypeViewModel | null = null;
  
  editForm = {
    id: 0,
    description: '',
    additionalDetails: '',
    isPaid: false
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

  loadLeaveTypes(): void {
    this.leaveRequestTypeService.getAllLeaveRequestTypes().subscribe(
      response => {
        console.log('API response:', response);
        
        const rawLeaveTypes: ILeaveRequestType[] = response.data;
        this.leaveRequestTypes = rawLeaveTypes.map(leaveType => 
          this.mapToLeaveTypeViewModel(leaveType)
        );
        
        console.log('Mapped Leave Types:', this.leaveRequestTypes);
      }
    );
  }
  
  private mapToLeaveTypeViewModel(leaveType: ILeaveRequestType): ILeaveRequestTypeViewModel {
    return {
      id: leaveType.id,
      additionalDetails: leaveType.additionalDetails || '',
      description: leaveType.description || '',
      isPaid: leaveType.isPaid,
      color: this.colorGenerator.generateColorFromId(leaveType.id)
    };
  }

  get filteredLeaveTypes(): ILeaveRequestTypeViewModel[] {
    if (!this.searchTerm) {
      return this.leaveRequestTypes;
    }
    
    const searchLower = this.searchTerm.toLowerCase();
    return this.leaveRequestTypes.filter(leaveType => 
      leaveType.description?.toLowerCase().includes(searchLower) ||
      leaveType.additionalDetails?.toLowerCase().includes(searchLower)
    );
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
      additionalDetails: leaveType.additionalDetails || '',
      isPaid: leaveType.isPaid || false
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
      additionalDetails: this.editForm.additionalDetails,
      isPaid: this.editForm.isPaid
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
    return !!(this.editForm.description.trim());
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