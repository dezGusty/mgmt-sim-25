import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class Auth {
  private apiUrl = 'https://localhost:7275/api/auth';

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
    return this.http.get(`${this.apiUrl}/me`,
      { withCredentials: true }
    );
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