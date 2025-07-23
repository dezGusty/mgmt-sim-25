import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EventEmitter, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';

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
  departmentName: string;
  departmentCode: string;
  description?: string;
  departmentHead?: string;
  budget?: number;
  status?: 'active' | 'inactive';
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
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-add-form.html',
  styleUrl: './admin-add-form.css'
})
export class AddForm implements OnInit {
  @Output() close = new EventEmitter<void>();
  @Output() submit = new EventEmitter<{ type: string; data: FormData }>();

  formType: string = '';
  adminForm: FormGroup;
  isSubmitting: boolean = false;

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

  constructor(private formBuilder: FormBuilder) {
    this.adminForm = this.formBuilder.group({});
  }

  ngOnInit(): void {
    // Initialize with empty form
  }

  setFormType(type: string): void {
    this.formType = type;
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
          departmentName: ['', [Validators.required, Validators.minLength(2)]],
          departmentCode: ['', [Validators.required, Validators.maxLength(10)]],
          description: [''],
          departmentHead: [''],
          status: ['active']
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
    if (this.adminForm.valid && !this.isSubmitting) {
      this.isSubmitting = true;
      
      const formData = this.adminForm.value;
      
      // Emit the form data with type
      this.submit.emit({
        type: this.formType,
        data: formData
      });

      // Simulate API call delay
      setTimeout(() => {
        this.isSubmitting = false;
        this.close.emit();
        this.resetForm();
      }, 1000);

      console.log(`${this.formType} form submitted:`, formData);
    } else {
      // Mark all fields as touched to show validation errors
      this.markFormGroupTouched();
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.adminForm.controls).forEach(field => {
      const control = this.adminForm.get(field);
      control?.markAsTouched({ onlySelf: true });
    });
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

  // Validation helper methods
  isFieldInvalid(fieldName: string): boolean {
    const field = this.adminForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.adminForm.get(fieldName);
    if (field && field.errors) {
      if (field.errors['required']) {
        return `${this.getFieldLabel(fieldName)} is required`;
      }
      if (field.errors['email']) {
        return 'Please enter a valid email address';
      }
      if (field.errors['minlength']) {
        return `${this.getFieldLabel(fieldName)} must be at least ${field.errors['minlength'].requiredLength} characters`;
      }
      if (field.errors['maxlength']) {
        return `${this.getFieldLabel(fieldName)} must not exceed ${field.errors['maxlength'].requiredLength} characters`;
      }
      if (field.errors['min']) {
        return `${this.getFieldLabel(fieldName)} must be at least ${field.errors['min'].min}`;
      }
    }
    return '';
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      name: 'Name',
      email: 'Email',
      department: 'Department',
      jobTitle: 'Job Title',
      departmentName: 'Department Name',
      departmentCode: 'Department Code',
      title: 'Title',
      code: 'Code',
      level: 'Level',
      minSalary: 'Minimum Salary',
      maxSalary: 'Maximum Salary',
      role: 'Role',
      leaveTypeName: 'Leave Type Name',
      category: 'Category',
      maxDaysPerYear: 'Max Days Per Year',
      advanceNoticeDays: 'Advance Notice Days'
    };
    return labels[fieldName] || fieldName;
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
