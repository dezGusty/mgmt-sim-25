import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JobTitlesService } from '../../../../services/job-titles/job-titles-service';
import { DepartmentService } from '../../../../services/departments/department-service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-add-job-title',
  imports: [FormsModule, CommonModule],
  templateUrl: './add-job-title.html',
  styleUrl: './add-job-title.css'
})
export class AddJobTitle {
  name: string = '';

  isSubmitting = false;
  submitted = false;

  onSubmitMessage: string = '';

  constructor(
    private jobTitlesService: JobTitlesService,
    private router: Router
  ) {}

  onSubmit(): void {
    this.isSubmitting = true;
    this.submitted = true;
    this.onSubmitMessage = ''; 

    if (!this.name.trim()) {
      this.onSubmitMessage = 'Please fill in all required fields.';
      this.isSubmitting = false;
      return;
    }

    const jobTitleData = {
      id: 0,
      name: this.name.trim(),
    };

    this.jobTitlesService.addJobTitle(jobTitleData).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        this.onSubmitMessage = response.message || 'Job title added successfully!';
        setTimeout(() => {
          this.name = '';
          this.submitted = false;
          this.onSubmitMessage = '';
        }, 2000);
      },
      error: (error) => {
        this.isSubmitting = false;
        this.onSubmitMessage = 'Error adding job title: ' + (error.error?.message || error.message || 'Unknown error');
      }
    });
  }

  onClose() {
    this.name = '';
    this.onSubmitMessage = '';
    this.submitted = false;
    this.isSubmitting = false;
  }

  cancel() {
    this.router.navigate(['/admin/job-titles']);
  }
}