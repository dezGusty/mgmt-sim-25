import { Pipe, PipeTransform } from '@angular/core';
import { IUserViewModel } from '../../view-models/user-view-model'; 

@Pipe({
  name: 'userFilter'
})
export class UserFilterPipe implements PipeTransform {

  transform(items: IUserViewModel[], role: string): any[] {
    if(role === undefined || role === null || role.trim() === '') {
      return items;
    }

    return items.filter(item => item.roles?.includes(role));
  }
}
