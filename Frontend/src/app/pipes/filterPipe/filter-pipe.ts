import { Pipe, PipeTransform } from '@angular/core';
import { UserViewModel } from '../../view-models/UserViewModel'; 

@Pipe({
  name: 'userFilter'
})
export class UserFilterPipe implements PipeTransform {

  transform(items: UserViewModel[], role: string): any[] {
    if(role === undefined || role === null || role.trim() === '') {
      return items;
    }

    return items.filter(item => item.roles?.includes(role));
  }
}
