import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-admin-main-page',
  imports: [CommonModule],
  templateUrl: './main-page.html',
})
export class AdminMainPage {
  showAddRequestForm = false;

  constructor(private router: Router) {}

  goBack() {
    this.router.navigate(['/']);
  }
}