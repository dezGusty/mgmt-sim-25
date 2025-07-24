import { Pipe, PipeTransform } from '@angular/core';
import { UserViewModel } from '../../view-models/UserViewModel';
import { User } from '../../components/user/user';

@Pipe({
  name: 'unassignedUsersPipe'
})
export class UnassignedUsersPipe implements PipeTransform {

  transform(users: UserViewModel[]): UserViewModel[] {
    return users.filter(u => u.managersIds && u.managersIds.length === 0);
  }

}
