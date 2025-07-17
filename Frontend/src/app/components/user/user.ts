import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-user',
  templateUrl: './user.html',
  styleUrl: './user.css',
})
export class User {
  constructor(private router: Router) {}

  goBack() {
    this.router.navigate(['/']);
  }
}
