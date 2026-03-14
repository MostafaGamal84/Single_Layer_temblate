import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  standalone: true,
  selector: 'app-login',
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="auth-shell">
      <div class="auth-panel">
        <section class="auth-hero">
          <p class="auth-eyebrow">Midnight Control Suite</p>
          <h1>Welcome back to the control room.</h1>
          <p class="auth-subtitle">
            A sharper dark interface for live sessions, player flow, and fast host decisions without visual clutter.
          </p>

          <div class="auth-chip-row">
            <span>Realtime orchestration</span>
            <span>Focused analytics</span>
            <span>Fast player access</span>
          </div>

          <div class="auth-glass-card">
            <span class="auth-card-label">Quick access</span>
            <h3>Quick Test Accounts</h3>
            <p><strong>Admin:</strong> admin@quiz.local / Admin@12345</p>
            <p><strong>Host:</strong> host@quiz.local / Host@12345</p>
          </div>
        </section>

        <section class="auth-form-wrap">
          <div class="auth-form auth-form-wide">
            <p class="auth-form-kicker">Secure access</p>
            <h2>Sign in</h2>
            <p class="auth-form-sub">Continue to your dark dashboard and resume where you left off.</p>

            <div class="field">
              <label for="login-email">Email</label>
              <input
                id="login-email"
                name="loginEmail"
                [(ngModel)]="email"
                type="email"
                autocomplete="email"
                placeholder="you@example.com"
              />
            </div>

            <div class="field">
              <label for="login-password">Password</label>
              <input
                id="login-password"
                name="loginPassword"
                [(ngModel)]="password"
                type="password"
                autocomplete="current-password"
                placeholder="Enter your password"
              />
            </div>

            <button class="submit-btn" [disabled]="loading" (click)="login()">
              {{ loading ? 'Signing in...' : 'Login' }}
            </button>

            @if (error) { <div class="alert">{{ error }}</div> }

            <p class="auth-switch">
              New here?
              <a routerLink="/auth/register">Create account</a>
            </p>
          </div>
        </section>
      </div>
    </div>
  `
})
export class LoginComponent {
  email = '';
  password = '';
  loading = false;
  error = '';

  constructor(private auth: AuthService, private router: Router) {}

  login(): void {
    this.loading = true;
    this.error = '';

    this.auth.login(this.email, this.password).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate([this.auth.role() === 'Player' ? '/player/history' : '/questions']);
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message || 'Login failed';
      }
    });
  }
}
