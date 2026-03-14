import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  standalone: true,
  selector: 'app-register',
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="auth-shell">
      <div class="auth-panel">
        <section class="auth-hero">
          <p class="auth-eyebrow">New account</p>
          <h1>Create your place inside Quiz Live.</h1>
          <p class="auth-subtitle">
            Set up a host or player account with the same dark visual system used across live sessions and reports.
          </p>

          <div class="auth-chip-row">
            <span>Host dashboard</span>
            <span>Player access</span>
            <span>Fast onboarding</span>
          </div>

          <div class="auth-glass-card">
            <span class="auth-card-label">What you get</span>
            <h3>One platform, two flows</h3>
            <p><strong>Hosts</strong> control sessions, publish quizzes, and monitor the room live.</p>
            <p><strong>Players</strong> join games, track history, and practice in test mode.</p>
          </div>
        </section>

        <section class="auth-form-wrap">
          <div class="auth-form auth-form-wide">
            <p class="auth-form-kicker">Onboarding</p>
            <h2>Create account</h2>
            <p class="auth-form-sub">Enter your details, choose a role, and continue into the platform.</p>

            <div class="split-fields">
              <div class="field">
                <label for="register-first-name">First name</label>
                <input id="register-first-name" name="registerFirstName" [(ngModel)]="firstName" placeholder="First name" />
              </div>

              <div class="field">
                <label for="register-last-name">Last name</label>
                <input id="register-last-name" name="registerLastName" [(ngModel)]="lastName" placeholder="Last name" />
              </div>
            </div>

            <div class="field">
              <label for="register-email">Email</label>
              <input id="register-email" name="registerEmail" [(ngModel)]="email" type="email" placeholder="Email" />
            </div>

            <div class="field">
              <label for="register-password">Password</label>
              <input id="register-password" name="registerPassword" [(ngModel)]="password" type="password" placeholder="Password (min 6 chars)" />
            </div>

            <div class="field">
              <label for="register-confirm-password">Confirm password</label>
              <input id="register-confirm-password" name="registerConfirmPassword" [(ngModel)]="confirmPassword" type="password" placeholder="Confirm password" />
            </div>

            <div class="field">
              <label for="register-role">Role</label>
              <select id="register-role" name="registerRole" [(ngModel)]="role">
                <option value="Host">Host</option>
                <option value="Player">Player</option>
              </select>
            </div>

            <button class="submit-btn" [disabled]="loading" (click)="register()">
              {{ loading ? 'Loading...' : 'Register' }}
            </button>

            @if (success) { <div class="success">{{ success }}</div> }
            @if (error) { <div class="alert">{{ error }}</div> }

            <p class="auth-switch">
              Already have an account?
              <a routerLink="/auth/login">Login</a>
            </p>
          </div>
        </section>
      </div>
    </div>
  `
})
export class RegisterComponent {
  firstName = '';
  lastName = '';
  email = '';
  password = '';
  confirmPassword = '';
  role: 'Host' | 'Player' = 'Host';
  loading = false;
  error = '';
  success = '';

  constructor(private auth: AuthService, private router: Router) {}

  register(): void {
    this.error = '';
    this.success = '';

    if (!this.firstName.trim() || !this.lastName.trim() || !this.email.trim() || !this.password.trim()) {
      this.error = 'All fields are required';
      return;
    }

    if (this.password.length < 6) {
      this.error = 'Password must be at least 6 characters';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.error = 'Password confirmation does not match';
      return;
    }

    this.loading = true;

    this.auth.register({
      email: this.email,
      password: this.password,
      firstName: this.firstName,
      lastName: this.lastName,
      role: this.role
    }).subscribe({
      next: () => {
        this.loading = false;
        this.success = 'Registration successful';
        this.router.navigate([this.role === 'Player' ? '/player/history' : '/questions']);
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message || 'Registration failed';
      }
    });
  }
}
