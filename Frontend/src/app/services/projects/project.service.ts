import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { IProject, IProjectWithUsers } from '../../models/entities/iproject';
import { IFilteredProjectsRequest } from '../../models/requests/ifiltered-projects-request';
import { IApiResponse } from '../../models/responses/iapi-response';
import { IFilteredApiResponse } from '../../models/responses/ifiltered-api-response';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  private apiUrl = `${environment.apiUrl}/api/Projects`;

  constructor(private http: HttpClient) {}

  getAllProjects(): Observable<IApiResponse<IProject[]>> {
    return this.http.get<IApiResponse<IProject[]>>(this.apiUrl);
  }

  getFilteredProjects(request: IFilteredProjectsRequest): Observable<IApiResponse<IFilteredApiResponse<IProject>>> {
    return this.http.post<IApiResponse<IFilteredApiResponse<IProject>>>(`${this.apiUrl}/filtered`, request);
  }

  getProjectById(id: number): Observable<IApiResponse<IProjectWithUsers>> {
    return this.http.get<IApiResponse<IProjectWithUsers>>(`${this.apiUrl}/${id}`);
  }

  createProject(project: Partial<IProject>): Observable<IApiResponse<IProject>> {
    return this.http.post<IApiResponse<IProject>>(this.apiUrl, project);
  }

  updateProject(id: number, project: Partial<IProject>): Observable<IApiResponse<IProject>> {
    return this.http.put<IApiResponse<IProject>>(`${this.apiUrl}/${id}`, project);
  }

  deleteProject(id: number): Observable<IApiResponse<void>> {
    return this.http.delete<IApiResponse<void>>(`${this.apiUrl}/${id}`);
  }

  assignUserToProject(projectId: number, userId: number, assignedPercentage: number): Observable<IApiResponse<void>> {
    const payload = {
      projectId,
      userId,
      assignedPercentage
    };
    return this.http.post<IApiResponse<void>>(`${this.apiUrl}/${projectId}/assign-user`, payload);
  }

  removeUserFromProject(projectId: number, userId: number): Observable<IApiResponse<void>> {
    return this.http.delete<IApiResponse<void>>(`${this.apiUrl}/${projectId}/users/${userId}`);
  }

  updateUserAssignment(projectId: number, userId: number, assignedPercentage: number): Observable<IApiResponse<void>> {
    const payload = { assignedPercentage };
    return this.http.put<IApiResponse<void>>(`${this.apiUrl}/${projectId}/users/${userId}`, payload);
  }
}