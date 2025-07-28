import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EventEmitter, OnInit, Output, Input } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { IDepartment } from '../../../../models/entities/idepartment';
import { DepartmentService } from '../../../../services/departments/department-service';
import { AddDepartment } from '../add-department/add-department';
import { AddUser } from '../add-user/add-user';
import { AddLeaveRequestType } from '../add-leave-request-type/add-leave-request-type';
import { AddJobTitle } from '../add-job-title/add-job-title';

@Component({
  selector: 'app-admin-add-form',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, AddDepartment, AddUser, AddLeaveRequestType, AddJobTitle],
  templateUrl: './admin-add-form.html',
  styleUrl: './admin-add-form.css'
})
export class AddForm {
  @Output() close = new EventEmitter<void>();
  
  formType: 'user' | 'department' | 'jobTitle' | 'leaveRequestType' = 'user';    

  constructor(private formBuilder: FormBuilder, private departmentService: DepartmentService) {
  }

  setFormType(type: 'user' | 'department' | 'jobTitle' | 'leaveRequestType'): void {
    this.formType = type;
  }
}