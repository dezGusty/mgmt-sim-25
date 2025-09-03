import { IUserProject } from './iproject';
import { IUser } from './iuser';

export interface IProjectUser extends IUserProject {
  user?: IUser;
  timeAllocatedPerProject?: number;
  timePercentagePerProject: number;
}
