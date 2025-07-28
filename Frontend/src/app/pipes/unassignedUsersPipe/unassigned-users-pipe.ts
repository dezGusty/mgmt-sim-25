import { Pipe, PipeTransform } from '@angular/core';
import { IUserViewModel } from '../../view-models/user-view-model';
import { User } from '../../components/user/user';

@Pipe({
  name: 'unassignedUsersPipe'
})
export class UnassignedUsersPipe implements PipeTransform {

  transform(users: IUserViewModel[]): IUserViewModel[] {
    return users.filter(u => u.managersIds && u.managersIds.length === 0);
  }

}
