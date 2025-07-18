import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [RouterModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  constructor(private router: Router) { }

  goBack() {
    this.router.navigate(['/']);
  }

  goToForgotPassword() {
    this.router.navigate(['/forgot-password']);
  }
}
