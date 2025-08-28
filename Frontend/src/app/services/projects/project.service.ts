import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { IProject, IProjectWithUsers } from '../../models/entities/iproject';
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
        // If backend already returns the envelope { success, data, ... }
        if (body && typeof body === 'object' && 'success' in body && 'data' in body) {
          return body as IApiResponse<IFilteredApiResponse<IProject>>;
        }
        // Otherwise assume backend returned the paged response directly and wrap it
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
    return this.http.get<IApiResponse<IProjectWithUsers>>(`${this.apiUrl}/${id}`);
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
        
        // Handle successful responses
        if (status >= 200 && status < 300) {
          // If backend already returns the envelope { success, data, ... }
          if (body && typeof body === 'object' && 'success' in body && 'data' in body) {
            return body as IApiResponse<IProject | null>;
          }
          // If backend returns the project directly, wrap it
          return { 
            data: body as IProject || null, 
            message: 'Project updated successfully', 
            success: true, 
            timestamp: new Date() 
          } as IApiResponse<IProject | null>;
        }
        
        // Handle other status codes
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
        
        // Handle successful responses (201 Created or other 2xx)
        if (status >= 200 && status < 300) {
          // If backend already returns the envelope { success, data, ... }
          if (body && typeof body === 'object' && 'success' in body && 'data' in body) {
            return body as IApiResponse<void>;
          }
          // If backend returns the assignment directly (CreatedAtAction response), wrap it
          return { 
            data: undefined, 
            message: 'User assigned to project successfully', 
            success: true, 
            timestamp: new Date() 
          } as IApiResponse<void>;
        }
        
        // Handle other status codes
        return { 
          data: undefined, 
          message: 'Failed to assign user to project', 
          success: false, 
          timestamp: new Date() 
        } as IApiResponse<void>;
      }),
      catchError(err => {
        console.error('assignUserToProject error', err);
        return of({ 
          data: undefined, 
          message: err?.message || 'Failed to assign user to project', 
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
}