import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LeaveRequestTypeService } from '../../../../services/leave-request-type';
import { ILeaveRequestType } from '../../../../models/entities/ileave-request-type';
import { IApiResponse } from '../../../../models/responses/iapi-response';

@Component({
  selector: 'app-add-leave-request-type',
  imports: [FormsModule, CommonModule],
  templateUrl: './add-leave-request-type.html',
  styleUrl: './add-leave-request-type.css'
})
export class AddLeaveRequestType {
  leaveTypeName: string = '';                
  leaveTypeDescription: string = '';                
  isPaid: boolean = false; 

  constructor(private leaveRequestTypeService: LeaveRequestTypeService) {

  }

  isFieldInvalid(field: any): boolean {
    return field.invalid && (field.dirty || field.touched);
  }

  onSubmit(form: any) {
    if (form.valid) {
      const lrt: ILeaveRequestType = {
        id : 0,
        description : this.leaveTypeName,
        additionalDetails : this.leaveTypeDescription,
        isPaid : this.isPaid
      } ;
      let response = this.leaveRequestTypeService.postLeaveRequestType(lrt).subscribe({
        next(response : IApiResponse<ILeaveRequestType>){
            console.log("lrt added");
        }
      })
    }
  }
  

  onReset(form: any) {
    this.leaveTypeName = '';
    this.leaveTypeDescription = '';
    this.isPaid = false;
  }
}