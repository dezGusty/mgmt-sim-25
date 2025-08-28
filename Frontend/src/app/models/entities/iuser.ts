export interface IUser {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  jobTitleId: number;
  jobTitleName?: string;
  roles: string[];
  departmentId?: number;
  departmentName?: string;
  subordinatesIds?: number[];
  subordinatesNames?: string[];
  subordinatesJobTitleIds?: number[];
  subordinatesJobTitles?: string[];
  subordinatesEmails?: string[];
  managersIds?: number[];
  isActive?: boolean;
  dateOfEmployment?: Date;
  employmentType?: string; // 'FullTime' or 'PartTime'
  totalAvailability?: number; // 1.0 for full-time, 0.5 for part-time
  remainingAvailability?: number;
}

export interface IAddUser {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  jobTitleId: number;
  departmentId: number;
  employeeRolesId: number[];
  dateOfEmployment: Date;
  employmentType?: string;
}

export interface IUpdateUser {
  id: number;
  email?: string;
  firstName?: string;
  lastName?: string;
  jobTitleId?: number;
  employeeRolesId?: number[];
  dateOfEmployment?: Date;
  employmentType?: string;
}