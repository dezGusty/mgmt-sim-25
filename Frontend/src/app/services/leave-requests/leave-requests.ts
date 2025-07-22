import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LeaveRequests {
  async fetchByManager() {
    const url = `https://localhost:7275/api/LeaveRequests/by-manager`;
    try {
      const response = await fetch(url, {
        credentials: 'include',
      });
      const data = await response.json();
      console.log(data);
      return data;
    } catch (err) {
      console.error('Fetch error:', err);
      return null;
    }
  }
}
