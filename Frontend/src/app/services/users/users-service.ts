import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { IAddUser, IUpdateUser, IUser } from '../../models/entities/iuser';
import { IFilteredUsersRequest } from '../../models/requests/ifiltered-users-request';
import { IFilteredApiResponse } from '../../models/responses/ifiltered-api-response';
import { HttpParams } from '@angular/common/http';
import { IApiResponse } from '../../models/responses/iapi-response';

@Injectable({
  providedIn: 'root'
})
export class UsersService {
  private baseUrl: string = 'https://localhost:7275/api/users';

  constructor(private http: HttpClient) {}

  getAllUsersIncludeRelationships(): Observable<IApiResponse<IUser[]>> {
    return this.http.get<IApiResponse<IUser[]>>(`${this.baseUrl}/includeRelationships`);
  }

  getAllUsersFiltered(params: IFilteredUsersRequest): Observable<IApiResponse<IFilteredApiResponse<IUser>>> {
    let paramsToSend = new HttpParams();
    
    if (params?.lastName) {
      paramsToSend = paramsToSend.set('lastName', params.lastName);
    }
    
    if (params?.email) {
      paramsToSend = paramsToSend.set('email', params.email);
    }

    if (params?.department) {
      paramsToSend = paramsToSend.set('department', params.department);
    }

    if (params?.jobTitle) {
      paramsToSend = paramsToSend.set('jobTitle', params.jobTitle);
    }

    if (params?.globalSearch) {
      paramsToSend = paramsToSend.set('globalSearch', params.globalSearch);
    }
    
    if (params?.params.sortBy) {
      paramsToSend = paramsToSend.set('PagedQueryParams.SortBy', params.params.sortBy);
    }
    
    if (params?.params.sortDescending !== undefined) {
      paramsToSend = paramsToSend.set('PagedQueryParams.SortDescending', params.params.sortDescending.toString());
    }
    
    if (params?.params.page) {
      paramsToSend = paramsToSend.set('PagedQueryParams.Page', params.params.page.toString());
    }
    
    if (params?.params.pageSize) {
      paramsToSend = paramsToSend.set('PagedQueryParams.PageSize', params.params.pageSize.toString());
    }

    return this.http.get<IApiResponse<IFilteredApiResponse<IUser>>>(`${this.baseUrl}/queried`, {params : paramsToSend});
  }

  getAllAdmins() : Observable<IApiResponse<IUser[]>> {
    return this.http.get<IApiResponse<IUser[]>>(`${this.baseUrl}/admins`);
  }

  getUnassignedUsers(params: IFilteredUsersRequest) : Observable<IApiResponse<IFilteredApiResponse<IUser>>> {
    let paramsToSend = new HttpParams();

    if (params?.params.page) {
      paramsToSend = paramsToSend.set('page', params.params.page.toString());
    }
    
    if (params?.params.pageSize) {
      paramsToSend = paramsToSend.set('pageSize', params.params.pageSize.toString());
    }

    return this.http.get<IApiResponse<IFilteredApiResponse<IUser>>>(`${this.baseUrl}/unassignedUsers/queried`, { params: paramsToSend });
  }

  getUsersIncludeRelationshipsFiltered(params: IFilteredUsersRequest): Observable<IApiResponse<IFilteredApiResponse<IUser>>> {
    let paramsToSend = new HttpParams();
    
    if (params?.lastName) {
      paramsToSend = paramsToSend.set('lastName', params.lastName);
    }
    
    if (params?.email) {
      paramsToSend = paramsToSend.set('email', params.email);
    }
    
    if (params?.params.sortBy) {
      paramsToSend = paramsToSend.set('PagedQueryParams.SortBy', params.params.sortBy);
    }
    
    if (params?.params.sortDescending !== undefined) {
      paramsToSend = paramsToSend.set('PagedQueryParams.SortDescending', params.params.sortDescending.toString());
    }
    
    if (params?.params.page) {
      paramsToSend = paramsToSend.set('PagedQueryParams.Page', params.params.page.toString());
    }
    
    if (params?.params.pageSize) {
      paramsToSend = paramsToSend.set('PagedQueryParams.PageSize', params.params.pageSize.toString());
    }
    
    return this.http.get<IApiResponse<IFilteredApiResponse<IUser>>>(`${this.baseUrl}/includeRelationships/queried`, { params: paramsToSend });
  }

  updateUser(user: IUpdateUser): Observable<IApiResponse<IUser>> {
    return this.http.patch<IApiResponse<IUser>>(`${this.baseUrl}/${user.id}`, user);
  }

  restoreUser(userId: number): Observable<string> {
    return this.http.patch<any>(`${this.baseUrl}/${userId}/restore`, {})
      .pipe(map(response => response.message));
  }

  deleteUser(userId: number): Observable<string> {
    return this.http.delete<any>(`${this.baseUrl}/${userId}`)
      .pipe(map(response => response.message));
  }

  addUser(user: IAddUser): Observable<IApiResponse<IUser>> {
    return this.http.post<IApiResponse<IUser>>(this.baseUrl, user);
  }

  getAllManagersFiltered(params: IFilteredUsersRequest) : Observable<IApiResponse<IFilteredApiResponse<IUser>>> {
    let paramsToSend = new HttpParams();
        
    if (params?.lastName) {
      paramsToSend = paramsToSend.set('lastName', params.lastName);
    }
    
    if (params?.email) {
      paramsToSend = paramsToSend.set('email', params.email);
    }
    
    if (params?.params.sortBy) {
      paramsToSend = paramsToSend.set('PagedQueryParams.SortBy', params.params.sortBy);
    }
    
    if (params?.params.sortDescending !== undefined) {
      paramsToSend = paramsToSend.set('PagedQueryParams.SortDescending', params.params.sortDescending.toString());
    }
    
    if (params?.params.page) {
      paramsToSend = paramsToSend.set('PagedQueryParams.Page', params.params.page.toString());
    }
    
    if (params?.params.pageSize) {
      paramsToSend = paramsToSend.set('PagedQueryParams.PageSize', params.params.pageSize.toString());
    }
    
    return this.http.get<IApiResponse<IFilteredApiResponse<IUser>>>(`${this.baseUrl}/managers`, { params: paramsToSend });
  }
}