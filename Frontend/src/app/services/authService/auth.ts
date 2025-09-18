import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
  import { environment } from '../../../environments/environment';
  import { BehaviorSubject } from 'rxjs';

export interface UserInfo {
  userId: string;
  email: string;
  roles: string[];
  originalRoles: string[];
  isActingAsSecondManager: boolean;
  isTemporarilyReplaced: boolean;
  isImpersonating?: boolean;
  impersonatedUserId?: string;
  originalUserId?: string;
  hasValidImpersonationToken?: boolean;
  shouldUseImpersonationToken?: boolean;
}

export interface ImpersonationInfo {
  name: string;
  roles: string[];
}

@Injectable({
  providedIn: 'root'
})
export class Auth {
  private apiUrl = `${environment.apiUrl}/auth`;
  private currentUser: UserInfo | null = null;

  private impersonationSubject = new BehaviorSubject<ImpersonationInfo | null>(null);
  impersonation$ = this.impersonationSubject.asObservable();

  // Subject to track impersonation state changes for UI updates
  private impersonationStateSubject = new BehaviorSubject<boolean>(false);
  impersonationState$ = this.impersonationStateSubject.asObservable();

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
    // Add cache busting for impersonation scenarios
    const headers = {
      'Cache-Control': 'no-cache, no-store, must-revalidate',
      'Pragma': 'no-cache',
      'Expires': '0'
    };
    
    return this.http.get<UserInfo>(`${this.apiUrl}/me`, { 
      withCredentials: true,
      headers: headers
    });
  }

  // Get original admin user info when impersonating
  getOriginalUser() {
    const headers = {
      'Cache-Control': 'no-cache, no-store, must-revalidate',
      'Pragma': 'no-cache',
      'Expires': '0'
    };
    
    return this.http.get<UserInfo>(`${this.apiUrl}/me/original`, { 
      withCredentials: true,
      headers: headers
    });
  }

  getCurrentUser(): UserInfo | null {
    return this.currentUser;
  }

  updateCurrentUser(user: UserInfo): void {
    const wasImpersonating = this.currentUser?.isImpersonating || false;
    const isNowImpersonating = user.isImpersonating || false;
    
    this.currentUser = user;
    
    // Emit state change if impersonation status changed
    if (wasImpersonating !== isNowImpersonating) {
      this.impersonationStateSubject.next(isNowImpersonating);
    }
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

  setImpersonation(info: ImpersonationInfo): void {
    this.impersonationSubject.next(info);
    this.impersonationStateSubject.next(true);
  }

  clearImpersonation(): void {
    this.impersonationSubject.next(null);
    this.impersonationStateSubject.next(false);
  }

  stopImpersonation() {
    return this.http.post(`${this.apiUrl}/stop-impersonation`, {},
      { withCredentials: true }
    );
  }

  getImpersonation(): ImpersonationInfo | null {
    return this.impersonationSubject.value;
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

  impersonate(userId: number) {
    return this.http.post(`${this.apiUrl}/impersonate`, { userId },
      { withCredentials: true }
    );
  }

  /**
   * Determines whether to use the impersonated user's token/context or fall back to admin's token
   * Returns true if impersonated user's token should be used, false if admin's token should be used
   */
  shouldUseImpersonationContext(): boolean {
    const user = this.getCurrentUser();
    
    // Use impersonated user's token if:
    // 1. User is currently impersonating
    // 2. The impersonated user has a valid token
    return user?.shouldUseImpersonationToken === true;
  }

  /**
   * Gets the effective user ID that should be used for authentication
   * Returns impersonated user ID if impersonation token should be used, otherwise returns admin ID
   */
  getEffectiveUserId(): string | null {
    const user = this.getCurrentUser();
    
    if (!user) return null;
    
    // If we should use impersonation context and we have an impersonated user ID, use it
    if (this.shouldUseImpersonationContext() && user.impersonatedUserId) {
      return user.impersonatedUserId;
    }
    
    // Otherwise, use the original user ID (admin's ID when impersonating, or current user ID normally)
    return user.originalUserId || user.userId;
  }

  /**
   * Checks if impersonation token is available and valid
   */
  hasValidImpersonationToken(): boolean {
    return this.getCurrentUser()?.hasValidImpersonationToken === true;
  }

  /**
   * Gets the current authentication context information for debugging/logging
   */
  getAuthenticationContext(): { isImpersonating: boolean; effectiveUserId: string | null; useImpersonationToken: boolean } {
    const user = this.getCurrentUser();
    return {
      isImpersonating: user?.isImpersonating === true,
      effectiveUserId: this.getEffectiveUserId(),
      useImpersonationToken: this.shouldUseImpersonationContext()
    };
  }
}