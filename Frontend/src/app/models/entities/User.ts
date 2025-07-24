export interface User {
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
}