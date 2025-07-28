export interface IUserViewModel {
  id: number;
  name: string;
  email: string;

  jobTitle?: {
    id: number;
    name?: string;
    department?: {
      id: number;
      name?: string;
    };
  }

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