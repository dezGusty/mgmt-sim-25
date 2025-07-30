export interface IEditUser {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  dateOfEmployment: Date;
  jobTitleId: number;
  leaveDaysLeftCurrentYear: number;
  isAdmin: boolean;
  isManager: boolean;
  isEmployee: boolean;
}