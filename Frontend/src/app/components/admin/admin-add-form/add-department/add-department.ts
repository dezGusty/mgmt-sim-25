import { Component } from '@angular/core';
import { DepartmentService } from '../../../../services/departments/department-service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-add-department',
  imports: [FormsModule],
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

    

  }

  onClose(){

  }
}
