import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JobTitle } from '../../../models/entities/JobTitle';
import { JobTitlesService } from '../../../services/jobTitles/job-titles';
import { JobTitleViewModel } from '../../../view-models/JobTitleViewModel';

@Component({
  selector: 'app-admin-job-titles-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-job-titles-list.html',
  styleUrl: './admin-job-titles-list.css'
})
export class AdminJobTitlesList implements OnInit {
  jobTitles: JobTitleViewModel[] = [];
  filteredJobTitles: JobTitleViewModel[] = [];
  searchTerm: string = '';
  selectedDepartment: string = '';
  selectedLevel: string = '';

  constructor(private jobTitleService:JobTitlesService) {

  }

  ngOnInit(): void {
    this.loadJobTitles();
  }

  loadJobTitles(): void {
    this.jobTitleService.getAllJobTitles().subscribe({
      next: (response) => {
        console.log('API response:', response);

        const rawJobTitles: JobTitle[] = response;
        this.jobTitles = rawJobTitles.map(jobTitle => ({
          id: jobTitle.id,
          departmentId: jobTitle.departmentId,
          departmentName: jobTitle.departmentName || 'Unknown',
          name: jobTitle.name,
          employeeCount: jobTitle.employeeCount || 0,
        }));
        this.filteredJobTitles = [...this.jobTitles];
      }
  });
  }

  onSearchChange(): void {
    this.filterJobTitles();
  }

  onDepartmentChange(): void {
    this.filterJobTitles();
  }

  onLevelChange(): void {
    this.filterJobTitles();
  }

  private filterJobTitles(): void {
    this.filteredJobTitles = this.jobTitles.filter(jobTitle => {
      const matchesSearch = !this.searchTerm || 
        jobTitle.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        jobTitle.departmentName.toLowerCase().includes(this.searchTerm.toLowerCase());


      return matchesSearch;
    });
  }

  editJobTitle(jobTitle: JobTitle): void {
    console.log('Edit job title:', jobTitle);
    // Implement edit functionality
  }

  viewJobTitle(jobTitle: JobTitle): void {
    console.log('View job title:', jobTitle);
    // Implement view functionality
  }

  deleteJobTitle(jobTitle: JobTitle): void {
    
  }

  getTotalJobTitles(): number {
    return this.jobTitles.length;
  }
}
