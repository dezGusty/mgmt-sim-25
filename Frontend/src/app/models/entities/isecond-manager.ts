export interface ISecondManagerResponse {
  secondManagerEmployeeId: number;
  secondManagerEmployee: {
    id: number;
    firstName: string;
    lastName: string;
    email: string;
    jobTitleName?: string;
    departmentName?: string;
  };
  replacedManagerId: number;
  replacedManager: {
    id: number;
    firstName: string;
    lastName: string;
    email: string;
    jobTitleName?: string;
    departmentName?: string;
  };
  startDate: Date;
  endDate: Date;
  isActive: boolean;
}

export interface ISecondManagerViewModel {
  id: number;
  name: string;
  email: string;
  jobTitle?: string;
  department?: string;
  avatar?: string;
  replacedManagerId: number;
  replacedManagerName: string;
  startDate: Date;
  endDate: Date;
  isActive: boolean;
} 