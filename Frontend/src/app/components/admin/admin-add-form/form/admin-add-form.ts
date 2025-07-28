import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EventEmitter, OnInit, Output, Input } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { IDepartment } from '../../../../models/entities/idepartment';
import { DepartmentService } from '../../../../services/departments/department-service';
import { AddDepartment } from '../add-department/add-department';

@Component({
  selector: 'app-admin-add-form',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, AddDepartment],
  templateUrl: './admin-add-form.html',
  styleUrl: './admin-add-form.css'
})
export class AddForm {
  formType: 'user' | 'department' | 'jobTitle' | 'leaveRequestType' = 'user';    

  constructor(private formBuilder: FormBuilder, private departmentService: DepartmentService) {

  }
}
