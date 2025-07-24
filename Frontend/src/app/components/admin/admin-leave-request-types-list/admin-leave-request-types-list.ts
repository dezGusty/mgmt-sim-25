import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LeaveRequestTypeService } from '../../../services/leaveRequestType/leave-request-type';
import { LeaveRequestType } from '../../../models/entities/LeaveRequestType';
import { LeaveRequestTypeViewModel } from '../../../view-models/LeaveRequestTypeViewModel';
import { ColorGenerator } from '../../../services/colorGenerator/color-generator';

@Component({
  selector: 'app-admin-leave-request-types-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-leave-request-types-list.html',
  styleUrl: './admin-leave-request-types-list.css'
})
export class AdminLeaveTypesList implements OnInit {
  leaveRequestTypes: LeaveRequestTypeViewModel[] = [];
  filteredLeaveTypes: LeaveRequestTypeViewModel[] = [];
  searchTerm: string = '';
  selectedCategory: string = '';
  selectedStatus: string = '';

  constructor(private leaveRequestTypeService: LeaveRequestTypeService,
              private colorGenerator: ColorGenerator) {

  }

  ngOnInit(): void {
    this.loadLeaveTypes();
  }

  loadLeaveTypes(): void {
    this.leaveRequestTypeService.getAllLeaveRequestTypes().subscribe(
      response => {
          console.log('API response:', response);

          const rawLeaveTypes: LeaveRequestType[] = response;
          this.leaveRequestTypes = rawLeaveTypes.map(leaveType => this.mapToLeaveTypeViewModel(leaveType));
          this.filteredLeaveTypes = [...this.leaveRequestTypes];

          console.log('Mapped Leave Types:', this.leaveRequestTypes);
      }
    )
  }
  
  private mapToLeaveTypeViewModel(leaveType: LeaveRequestType): LeaveRequestTypeViewModel {
    return {
      id : leaveType.id,
      additionalDetails: leaveType.additionalDetails || '',
      description: leaveType.description || '',
      color: this.colorGenerator.generateColorFromId(leaveType.id)
    };
  }

  trackByLeaveType(index: number, leaveType: LeaveRequestTypeViewModel): any {
    return leaveType.id + '-' + leaveType.color;
  }

  onSearchChange(): void {
    this.filterLeaveTypes();
  }

  private filterLeaveTypes(): void {
    this.filteredLeaveTypes = this.leaveRequestTypes.filter(leaveType => {
      const matchesSearch = !this.searchTerm || 
        leaveType.description?.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        leaveType.additionalDetails?.toLowerCase().includes(this.searchTerm.toLowerCase());

      return matchesSearch;
    });
  }

  getCategoryBadgeClass(category: string): string {
    switch (category) {
      default:
        return 'bg-blue-100 text-blue-800';
    }
  }
}
