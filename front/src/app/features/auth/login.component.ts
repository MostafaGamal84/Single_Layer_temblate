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

            @if (rejectedStatus) {
              <div class="rejected-notice">
                <div class="rejected-icon">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <circle cx="12" cy="12" r="10"></circle>
                    <line x1="15" y1="9" x2="9" y2="15"></line>
                    <line x1="9" y1="9" x2="15" y2="15"></line>
                  </svg>
                </div>
                <h4>Registration Rejected</h4>
                <p>Your registration request has been rejected.</p>
                <p class="rejected-note">Please contact the administrator for more information.</p>
              </div>
            }

            @if (error) { <div class="alert">{{ error }}</div> }

            <p class="auth-switch">
              New here?
              <a routerLink="/auth/register">Create account</a>
            </p>
          </div>
        </section>
      </div>
    </div>
  `,
  styles: [`
    .rejected-notice {
      margin-top: 20px;
      padding: 20px;
      background: var(--danger-tint);
      border: 1px solid var(--danger-border);
      border-radius: 14px;
      text-align: center;
    }

    .rejected-icon {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 48px;
      height: 48px;
      border-radius: 50%;
      background: var(--danger);
      margin-bottom: 12px;
    }

    .rejected-icon svg {
      width: 24px;
      height: 24px;
      color: white;
    }

    .rejected-notice h4 {
      margin: 0 0 8px;
      color: var(--danger);
      font-size: 1.1rem;
    }

    .rejected-notice p {
      margin: 0 0 8px;
      color: var(--text);
    }

    .rejected-note {
      font-size: 0.9rem;
      color: var(--muted) !important;
    }
  `]
})
export class LoginComponent {
  email = '';
  password = '';
  loading = false;
  error = '';
  rejectedStatus = false;

  constructor(private auth: AuthService, private router: Router) {}

  login(): void {
    this.loading = true;
    this.error = '';
    this.rejectedStatus = false;

    this.auth.login(this.email, this.password).subscribe({
      next: (res) => {
        this.loading = false;
        const userStatus = res?.status ?? this.auth.status();
        
        if (userStatus === 0) {
          this.router.navigate(['/auth/pending-status']);
          return;
        }
        
        if (userStatus === 2) {
          this.rejectedStatus = true;
          this.auth.logout();
          return;
        }

        const role = this.auth.role();
        if (role === 'Player') {
          this.router.navigate(['/player/history']);
        } else if (role === 'Admin' || role === 'Host') {
          this.router.navigate(['/questions']);
        } else {
          this.router.navigate(['/test-mode']);
        }
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message || 'Login failed';
      }
    });
  }
}
