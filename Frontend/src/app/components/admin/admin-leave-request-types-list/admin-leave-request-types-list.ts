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
}