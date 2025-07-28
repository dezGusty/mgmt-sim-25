import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EventEmitter, OnInit, Output, Input } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { IDepartment } from '../../../models/entities/idepartment';
import { DepartmentService } from '../../../services/departments/department-service';

export interface UserFormData {
  firstName: string;
  lastName: string;
  email: string;
  department: string;
  jobTitle: string;
  role: 'admin' | 'manager' | 'employee';
  manager?: string;
  startDate?: Date;
  password?: string;
  status?: 'active' | 'inactive';
}

export interface DepartmentFormData {
  id: number;
  name: string;
  description?: string;
}

export interface JobTitleFormData {
  title: string;
  code: string;
  department: string;
  level: 'entry' | 'mid' | 'senior' | 'executive';
  minSalary?: number;
  maxSalary?: number;
  description?: string;
  requirements?: string[];
  status?: 'active' | 'inactive';
}

export interface LeaveTypeFormData {
  leaveTypeName: string;
  code: string;
  category: 'paid' | 'unpaid' | 'sick' | 'special';
  maxDaysPerYear: number; // -1 for unlimited
  advanceNoticeDays: number;
  description?: string;
  requiresApproval: boolean;
  requiresDocumentation: boolean;
  probationPeriodExcluded: boolean;
  minimumTenure?: number;
  eligibleRoles?: string[];
  status?: 'active' | 'inactive';
}

@Component({
  selector: 'app-admin-add-form',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './admin-add-form.html',
  styleUrl: './admin-add-form.css'
})
export class AddForm implements OnInit {
  @Input() formType: string = '';
  @Output() close = new EventEmitter<void>();
  @Output() submit = new EventEmitter<{ type: string; data: any }>();

  adminForm: FormGroup;
  isSubmitting: boolean = false;
  error: string = '';

  // Dropdown options
  departments = [
    { value: 'engineering', label: 'Engineering' },
    { value: 'marketing', label: 'Marketing' },
    { value: 'sales', label: 'Sales' },
    { value: 'hr', label: 'Human Resources' },
    { value: 'finance', label: 'Finance' },
    { value: 'operations', label: 'Operations' }
  ];

  jobLevels = [
    { value: 'entry', label: 'Entry Level' },
    { value: 'mid', label: 'Mid Level' },
    { value: 'senior', label: 'Senior Level' },
    { value: 'executive', label: 'Executive' }
  ];

  userRoles = [
    { value: 'admin', label: 'Admin' },
    { value: 'manager', label: 'Manager' },
    { value: 'employee', label: 'Employee' }
  ];

  leaveCategories = [
    { value: 'paid', label: 'Paid Leave' },
    { value: 'unpaid', label: 'Unpaid Leave' },
    { value: 'sick', label: 'Medical Leave' },
    { value: 'special', label: 'Special Leave' }
  ];

  // Mock data for managers
  managers = [
    { value: 'manager1', label: 'John Smith' },
    { value: 'manager2', label: 'Sarah Johnson' },
    { value: 'manager3', label: 'Michael Brown' }
  ];

  // Mock data for job titles
  jobTitles = [
    { value: 'developer', label: 'Software Developer' },
    { value: 'designer', label: 'UI/UX Designer' },
    { value: 'analyst', label: 'Business Analyst' },
    { value: 'manager', label: 'Project Manager' }
  ];

  constructor(private formBuilder: FormBuilder, private departmentService: DepartmentService) {
    this.adminForm = this.formBuilder.group({});
  }

  ngOnInit(): void {
    // Initialize with empty form
  }

  setFormType(type: string): void {
    this.formType = type;
    this.error = '';
    this.buildForm();
  }

  private buildForm(): void {
    switch (this.formType) {
      case 'manager':
        this.adminForm = this.formBuilder.group({
          firstName: ['', [Validators.required, Validators.minLength(2)]],
          lastName: ['', [Validators.required, Validators.minLength(2)]],
          email: ['', [Validators.required, Validators.email]],
          department: ['', Validators.required],
          jobTitle: ['', Validators.required],

          status: ['active']
        });
        break;

      case 'department':
        this.adminForm = this.formBuilder.group({
          name: ['', [Validators.required, Validators.minLength(2)]],
          description: ['']
        });
        break;

      case 'jobTitle':
        this.adminForm = this.formBuilder.group({
          title: ['', [Validators.required, Validators.minLength(2)]],
          code: ['', [Validators.required, Validators.maxLength(10)]],
          department: ['', Validators.required],
          level: ['', Validators.required],
          minSalary: ['', [Validators.min(0)]],
          maxSalary: ['', [Validators.min(0)]],
          status: ['active']
        });
        break;

      case 'user':
        this.adminForm = this.formBuilder.group({
          name: ['', [Validators.required, Validators.minLength(2)]],
          email: ['', [Validators.required, Validators.email]],
          role: ['', Validators.required],
          department: ['', Validators.required],
          jobTitle: [''],
          manager: [''],
          startDate: [''],
          status: ['active']
        });
        break;

      case 'leaveType':
        this.adminForm = this.formBuilder.group({
          leaveTypeName: ['', [Validators.required, Validators.minLength(2)]],
          code: ['', [Validators.required, Validators.maxLength(5)]],
          category: ['', Validators.required],
          maxDaysPerYear: ['', [Validators.min(-1)]],
          advanceNoticeDays: ['', [Validators.min(0)]],
          description: [''],
          requiresApproval: [true],
          requiresDocumentation: [false],
          probationPeriodExcluded: [false],
          status: ['active']
        });
        break;

      default:
        this.adminForm = this.formBuilder.group({});
    }
  }

  onSubmit(): void {
    if (this.adminForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.isSubmitting = true;
    this.error = '';

    switch (this.formType) {
      case 'department':
        this.submitDepartment();
        break;
    }
  }

  private submitDepartment(): void {
    const departmentData: DepartmentFormData = this.adminForm.value;

    this.departmentService.createDepartment(departmentData).subscribe({
      next: (department) => {
        console.log('Department created successfully:', department);
        this.submit.emit({ type: 'department', data: department });
        this.close.emit();
        this.isSubmitting = false;
      },
      error: (error) => {
        console.error('Error creating department:', error);
        this.error = error.error?.message || 'Failed to create department. Please try again.';
        this.isSubmitting = false;
      }
    });
  }

  private markFormGroupTouched(): void {
    Object.keys(this.adminForm.controls).forEach(key => {
      const control = this.adminForm.get(key);
      control?.markAsTouched();
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.adminForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.adminForm.get(fieldName);
    if (field && field.errors && (field.dirty || field.touched)) {
      if (field.errors['required']) {
        return `${this.getFieldLabel(fieldName)} is required`;
      }
      if (field.errors['minlength']) {
        return `${this.getFieldLabel(fieldName)} must be at least ${field.errors['minlength'].requiredLength} characters`;
      }
    }
    return '';
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      name: 'Department Name',
      description: 'Description'
    };
    return labels[fieldName] || fieldName;
  }

  onClose(): void {
    this.close.emit();
  }

  private resetForm(): void {
    this.adminForm.reset();
    this.formType = '';
  }

  getFormTypeLabel(): string {
    switch (this.formType) {
      case 'manager':
        return 'Manager';
      case 'department':
        return 'Department';
      case 'jobTitle':
        return 'Job Title';
      case 'user':
        return 'User';
      case 'leaveType':
        return 'Leave Type';
      default:
        return 'Item';
    }
  }


  // Form validation methods
  validateSalaryRange(): void {
    const minSalary = this.adminForm.get('minSalary')?.value;
    const maxSalary = this.adminForm.get('maxSalary')?.value;

    if (minSalary && maxSalary && minSalary > maxSalary) {
      this.adminForm.get('maxSalary')?.setErrors({ 'salaryRange': true });
    }
  }

  onCancel(): void {
    this.close.emit();
    this.resetForm();
  }
}
