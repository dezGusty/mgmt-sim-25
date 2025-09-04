import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { IProject, IProjectWithUsers, IUserProject } from '../../models/entities/iproject';
import { IUser } from '../../models/entities/iuser';
import { IFilteredProjectsRequest } from '../../models/requests/ifiltered-projects-request';
import { IApiResponse } from '../../models/responses/iapi-response';
import { IFilteredApiResponse } from '../../models/responses/ifiltered-api-response';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  private apiUrl: string = (() => {
    const base = (environment.apiUrl || '').replace(/\/+$/g, '');
    return base.endsWith('/api') ? `${base}/Projects` : `${base}/api/Projects`;
  })();

  constructor(private http: HttpClient) {}

  getAllProjects(): Observable<IApiResponse<IProject[]>> {
    return this.http.get<IApiResponse<IProject[]>>(this.apiUrl);
  }

  getFilteredProjects(request: IFilteredProjectsRequest): Observable<IApiResponse<IFilteredApiResponse<IProject>>> {
    return this.http.post<any>(`${this.apiUrl}/filtered`, request, { observe: 'response' }).pipe(
      map(resp => {
        const body = resp.body;
        if (body && typeof body === 'object' && 'success' in body && 'data' in body) {
          return body as IApiResponse<IFilteredApiResponse<IProject>>;
        }
        return {
          data: body as IFilteredApiResponse<IProject>,
          success: true,
          message: '',
          timestamp: new Date()
        } as IApiResponse<IFilteredApiResponse<IProject>>;
      }),
      catchError(err => {
        console.error('getFilteredProjects error', err);
        const empty: IFilteredApiResponse<IProject> = { data: [], page: 1, pageSize: 10, totalPages: 0, totalCount: 0, hasNextPage: false, hasPreviousPage: false };
        return of({ data: empty, success: false, message: err?.message || 'Error', timestamp: new Date() } as IApiResponse<IFilteredApiResponse<IProject>>);
      })
    );
  }

  getProjectById(id: number): Observable<IApiResponse<IProjectWithUsers>> {
    const url = `${this.apiUrl}/${id}`;
    return this.http.get<any>(url).pipe(
      map(response => {
        if (response && typeof response === 'object' && 'success' in response && 'data' in response) {
          return response as IApiResponse<IProjectWithUsers>;
        }
        if (response && typeof response === 'object' && 'id' in response) {
          return {
            data: response as IProjectWithUsers,
            success: true,
            message: 'Project loaded successfully',
            timestamp: new Date()
          } as IApiResponse<IProjectWithUsers>;
        }
        return {
          data: null as any,
          success: false,
          message: 'Invalid response format',
          timestamp: new Date()
        } as IApiResponse<IProjectWithUsers>;
      }),
      catchError(err => {
        console.error('getProjectById error', err);
        return of({
          data: null as any,
          success: false,
          message: err?.message || 'Error loading project',
          timestamp: new Date()
        } as IApiResponse<IProjectWithUsers>);
      })
    );
  }

  getProjectWithUsers(id: number): Observable<IApiResponse<IProjectWithUsers>> {
    const url = `${this.apiUrl}/${id}/with-users?_t=${Date.now()}`;  // Add timestamp to prevent caching
    console.log('Making API call to:', url);
    return this.http.get<any>(url).pipe(
      map(response => {
        if (response && typeof response === 'object' && 'success' in response && 'data' in response) {
          return response as IApiResponse<IProjectWithUsers>;
        }
        if (response && typeof response === 'object') {
          return {
            data: response as IProjectWithUsers,
            success: true,
            message: 'Project loaded successfully',
            timestamp: new Date()
          } as IApiResponse<IProjectWithUsers>;
        }
        return {
          data: null as any,
          success: false,
          message: 'Invalid response format',
          timestamp: new Date()
        } as IApiResponse<IProjectWithUsers>;
      }),
      catchError(err => {
        console.error('getProjectWithUsers error', err);
        return of({
          data: null as any,
          success: false,
          message: err?.message || 'Error loading project with users',
          timestamp: new Date()
        } as IApiResponse<IProjectWithUsers>);
      })
    );
  }

  getProjectUsers(projectId: number): Observable<IApiResponse<IUserProject[]>> {
    const url = `${this.apiUrl}/${projectId}/users`;
    return this.http.get<any>(url).pipe(
      map(response => {

        if (response && typeof response === 'object' && 'success' in response && 'data' in response) {
          return response as IApiResponse<IUserProject[]>;
        }
        if (Array.isArray(response)) {
          return {
            data: response as IUserProject[],
            success: true,
            message: 'Project users loaded successfully',
            timestamp: new Date()
          } as IApiResponse<IUserProject[]>;
        }

        return {
          data: [],
          success: false,
          message: 'Invalid response format',
          timestamp: new Date()
        } as IApiResponse<IUserProject[]>;
      }),
      catchError(err => {
        console.error('getProjectUsers error', err);
        return of({
          data: [],
          success: false,
          message: err?.message || 'Error loading project users',
          timestamp: new Date()
        } as IApiResponse<IUserProject[]>);
      })
    );
  }

  createProject(project: Partial<IProject>): Observable<IApiResponse<IProject | null>> {
    return this.http.post<any>(this.apiUrl, project, { observe: 'response' }).pipe(
      map(resp => {
        const status = resp.status;
        const body = resp.body;
        if ((status === 204) || (status >= 200 && status < 300 && (!body || (Object.keys(body).length === 0)))) {
          return { data: null, message: 'Created', success: true, timestamp: new Date() } as IApiResponse<IProject | null>;
        }

        if (body && typeof body === 'object' && ('success' in body) && ('data' in body)) {
          return body as IApiResponse<IProject | null>;
        }
        return { data: body as IProject || null, message: 'Created', success: true, timestamp: new Date() } as IApiResponse<IProject | null>;
      }),
      catchError(err => {
        console.error('createProject error', err);
        return of({ data: null, message: err?.message || 'Error', success: false, timestamp: new Date() } as IApiResponse<IProject | null>);
      })
    );
  }

  updateProject(id: number, project: Partial<IProject>): Observable<IApiResponse<IProject | null>> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, project, { observe: 'response' }).pipe(
      map(resp => {
        const status = resp.status;
        const body = resp.body;
        
        if (status >= 200 && status < 300) {
          if (body && typeof body === 'object' && 'success' in body && 'data' in body) {
            return body as IApiResponse<IProject | null>;
          }
          return { 
            data: body as IProject || null, 
            message: 'Project updated successfully', 
            success: true, 
            timestamp: new Date() 
          } as IApiResponse<IProject | null>;
        }
        
        return { 
          data: null, 
          message: 'Failed to update project', 
          success: false, 
          timestamp: new Date() 
        } as IApiResponse<IProject | null>;
      }),
      catchError(err => {
        console.error('updateProject error', err);
        return of({ 
          data: null, 
          message: err?.message || 'Failed to update project', 
          success: false, 
          timestamp: new Date() 
        } as IApiResponse<IProject | null>);
      })
    );
  }

  deleteProject(id: number): Observable<IApiResponse<void>> {
    return this.http.delete<IApiResponse<void>>(`${this.apiUrl}/${id}`);
  }

  assignUserToProject(projectId: number, userId: number, assignedPercentage: number): Observable<IApiResponse<void>> {
    const payload = {
      userId,
      timePercentagePerProject: assignedPercentage
    };
    return this.http.post<any>(`${this.apiUrl}/${projectId}/users`, payload, { observe: 'response' }).pipe(
      map(resp => {
        const status = resp.status;
        const body = resp.body;
        
        if (status >= 200 && status < 300) {
          if (body && typeof body === 'object' && 'success' in body && 'data' in body) {
            return body as IApiResponse<void>;
          }
          return { 
            data: undefined, 
            message: 'User assigned to project successfully', 
            success: true, 
            timestamp: new Date() 
          } as IApiResponse<void>;
        }
        
        return { 
          data: undefined, 
          message: 'Failed to assign user to project', 
          success: false, 
          timestamp: new Date() 
        } as IApiResponse<void>;
      }),
      catchError(err => {
        console.error('assignUserToProject error', err);
        
        let errorMessage = 'Failed to assign user to project';
        
        if (err.status === 409) {
          if (err.error && typeof err.error === 'string') {
            errorMessage = err.error;
          } else if (err.error && err.error.message) {
            errorMessage = err.error.message;
          } else if (err.error && err.error.title) {
            errorMessage = err.error.title;
          } else {
            errorMessage = 'Employee is already assigned to this project';
          }
        } else if (err.error && err.error.message) {
          errorMessage = err.error.message;
        } else if (err.message) {
          errorMessage = err.message;
        }
        
        return of({ 
          data: undefined, 
          message: errorMessage, 
          success: false, 
          timestamp: new Date() 
        } as IApiResponse<void>);
      })
    );
  }

  removeUserFromProject(projectId: number, userId: number): Observable<IApiResponse<void>> {
    return this.http.delete<IApiResponse<void>>(`${this.apiUrl}/${projectId}/users/${userId}`);
  }

  updateUserAssignment(projectId: number, userId: number, assignedPercentage: number): Observable<IApiResponse<void>> {
    const payload = { assignedPercentage };
    return this.http.put<IApiResponse<void>>(`${this.apiUrl}/${projectId}/users/${userId}`, payload);
  }

  getAvailableUsersForProject(projectId: number, page: number = 1, pageSize: number = 10, search: string = ''): Observable<IApiResponse<IFilteredApiResponse<IUser>>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    if (search) {
      params = params.set('search', search);
    }
    
    return this.http.get<any>(`${this.apiUrl}/${projectId}/available-users`, { params }).pipe(
      map(response => {
        if (response && typeof response === 'object' && 'success' in response && 'data' in response) {
          return response as IApiResponse<IFilteredApiResponse<IUser>>;
        }
        if (response && typeof response === 'object' && 'data' in response) {
          return {
            data: response as IFilteredApiResponse<IUser>,
            success: true,
            message: 'Available users loaded successfully',
            timestamp: new Date()
          } as IApiResponse<IFilteredApiResponse<IUser>>;
        }
        return {
          data: {
            data: [],
            page: 1,
            pageSize: pageSize,
            totalPages: 0,
            totalCount: 0,
            hasNextPage: false,
            hasPreviousPage: false
          } as IFilteredApiResponse<IUser>,
          success: false,
          message: 'Invalid response format',
          timestamp: new Date()
        } as IApiResponse<IFilteredApiResponse<IUser>>;
      }),
      catchError(err => {
        console.error('getAvailableUsersForProject error', err);
        const empty: IFilteredApiResponse<IUser> = {
          data: [],
          page: 1,
          pageSize: pageSize,
          totalPages: 0,
          totalCount: 0,
          hasNextPage: false,
          hasPreviousPage: false
        };
        return of({
          data: empty,
          success: false,
          message: err?.message || 'Error loading available users',
          timestamp: new Date()
        } as IApiResponse<IFilteredApiResponse<IUser>>);
      })
    );
  }
}