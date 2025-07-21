import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'custom-navbar',
  imports: [CommonModule],
  templateUrl: './custom-navbar.html',
})
export class CustomNavbar {
  showAddRequestForm = false;

  constructor(private router: Router) {}

  goBack() {
    this.router.navigate(['/']);
  }
}