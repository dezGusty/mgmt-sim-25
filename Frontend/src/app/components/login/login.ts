import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  constructor(private router: Router) { }

  goBack() {
    this.router.navigate(['/']);
  }

  forgotPassword() {
    this.router.navigate(['/forgot-password']);
  }

}
