export interface UserViewModel {
  id: number;
  name: string;
  email: string;
  jobTitle?: string;
  jobTitleId?: number;
  department?: string;
  departmentId?: number;
  subordinatesIds?: number[];
  subordinatesNames?: string[];
  subordinatesJobTitleIds?: number[];
  subordinatesJobTitleNames?: string[];
  managersIds?: number[];
  subordinatesEmails?: string[];
  teamSize?: number;
  status?: string;
  avatar?: string;
  roles?: string[];
}