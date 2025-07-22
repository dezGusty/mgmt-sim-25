import { Component, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './../../services/auth/auth';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-login',
  templateUrl: './login.html',
  styleUrl: './login.css',
  imports: [CommonModule, FormsModule],
})
export class Login {
  email = '';
  password = '';
  errorMessage = '';

  constructor(
    private router: Router,
    @Inject(AuthService) private authService: AuthService
  ) {}

  goBack() {
    this.router.navigate(['/']);
  }

  onSubmit() {
    this.authService.login(this.email, this.password).subscribe({
      next: () => this.router.navigate(['/manager']),
      error: (err) => {
        this.errorMessage = 'Invalid credentials';
        console.error(err);
      },
    });
  }
}
