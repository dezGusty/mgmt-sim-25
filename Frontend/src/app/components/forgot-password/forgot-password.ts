import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-forgot-password',
  imports: [RouterModule],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.css'
})
export class ForgotPassword {
  constructor(private router: Router) { }

  goBack() {
    this.router.navigate(['/login']);
  }

  submitEmail(email: string) {
    console.log(`Password reset email sent to: ${email}`);
    this.router.navigate(['/login']);
  }
}
