import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  constructor(private router: Router) {}

  navigateToAdmin() {
    this.router.navigate(['/admin']);
  }

  navigateToLogin() {
    this.router.navigate(['/login']);
  }

  navigateToUser() {
    this.router.navigate(['/user']);
  }
}
