import { Component } from '@angular/core';
import { DepartmentService } from '../../../../services/departments/department-service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { identifierName } from '@angular/compiler';

@Component({
  selector: 'app-add-department',
  imports: [FormsModule, CommonModule],
  templateUrl: './add-department.html',
  styleUrl: './add-department.css'
})
export class AddDepartment {
  name: string = '';
  description: string = '';

  isSubmitting = false;
  submitted = false;

  onSubmitMessage: string = '';

  constructor(private departmentsService: DepartmentService) {
    
  }

  onSubmit() : void{
    this.isSubmitting = true;
    this.submitted = true;
    this.onSubmitMessage = ''; // Clear previous messages

    if(!this.name.trim()) {
      this.onSubmitMessage = 'Please fill in the department name.';
      this.isSubmitting = false;
      return;
    }

    const departmentData = {
      id: 0,
      name: this.name.trim(),
      description: this.description.trim() || 'No description available'
    }

    this.departmentsService.addDepartment(departmentData).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        this.onSubmitMessage = 'Department added successfully!';
        // Reset form after a delay to show success message
        setTimeout(() => {
          this.name = '';
          this.description = '';
          this.submitted = false;
          this.onSubmitMessage = '';
        }, 2000);
      },
      error: (error) => {
        this.isSubmitting = false;
        this.onSubmitMessage = 'Error adding department: ' + (error.error?.message || error.message);
      }
    });
  }

  onClose(){
    this.name = '';
    this.description = '';
    this.onSubmitMessage = '';
    this.submitted = false;
    this.isSubmitting = false;
  }
}
