export interface IProject {
  id: number;
  name?: string;
  startDate: Date;
  endDate: Date;
  budgetedFTEs: number;
  isActive: boolean;
  assignedUsersCount: number;
  totalAssignedPercentage: number;
  totalAssignedFTEs: number;
  remainingFTEs: number;
  createdAt?: Date;
  deletedAt?: Date;
  modifiedAt?: Date;
}

export interface IProjectWithUsers {
  id: number;
  name?: string;
  startDate: Date;
  endDate: Date;
  budgetedFTEs: number;
  isActive: boolean;
  assignedUsers: IUserProject[];
  totalAssignedFTEs: number;
  remainingFTEs: number;
  createdAt?: Date;
  deletedAt?: Date;
  modifiedAt?: Date;
}

export interface IUserProject {
  id: number;
  userId: number;
  projectId: number;
  assignedPercentage: number;
  user?: {
    id: number;
    firstName?: string;
    lastName?: string;
    email?: string;
  };
}