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
}

export interface IUpdateUser {
  id: number;
  email?: string;
  firstName?: string;
  lastName?: string;
  jobTitleId?: number;
  employeeRolesId?: number[];
  dateOfEmployment?: Date;
}