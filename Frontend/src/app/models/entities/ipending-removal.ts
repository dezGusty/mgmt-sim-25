import { IUser } from './iuser';

export interface IPendingRemoval {
  userId: number;
  user?: IUser;
  originalPercentage: number;
}
