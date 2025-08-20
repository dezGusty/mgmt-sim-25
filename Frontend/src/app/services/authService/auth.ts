import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
  import { environment } from '../../../environments/environment';

export interface UserInfo {
  userId: string;
  email: string;
  roles: string[];
  originalRoles: string[];
  isActingAsSecondManager: boolean;
  isTemporarilyReplaced: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class Auth {
  private apiUrl = `${environment.apiUrl}/auth`;
  private currentUser: UserInfo | null = null;

  constructor(private http: HttpClient) { }

  login(email: string, password: string) {
    return this.http.post(`${this.apiUrl}/login`, { email, password },
      { withCredentials: true }
    );
  }

  logout() {
    return this.http.post(`${this.apiUrl}/logout`, {},
      { withCredentials: true }
    );
  }

  me() {
    return this.http.get<UserInfo>(`${this.apiUrl}/me`,
      { withCredentials: true }
    );
  }

  getCurrentUser(): UserInfo | null {
    return this.currentUser;
  }

  updateCurrentUser(user: UserInfo): void {
    this.currentUser = user;
  }

  isActingAsSecondManager(): boolean {
    return this.currentUser?.isActingAsSecondManager || false;
  }

  isTemporarilyReplaced(): boolean {
    return this.currentUser?.isTemporarilyReplaced || false;
  }

  hasEffectiveRole(role: string): boolean {
    return this.currentUser?.roles.includes(role) || false;
  }

  sendResetCode(email: string) {
    return this.http.post(`${this.apiUrl}/send-reset-code`, { email },
      { withCredentials: true }
    );
  }

  resetPassword(verificationCode: string, newPassword: string, confirmPassword: string) {
    return this.http.post(`${this.apiUrl}/reset-password`, {
      verificationCode,
      newPassword,
      confirmPassword
    }, { withCredentials: true });
  }
}