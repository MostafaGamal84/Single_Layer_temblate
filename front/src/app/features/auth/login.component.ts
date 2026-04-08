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
          <div class="auth-brand">
            <img src="/logo.jpeg" alt="GOUDAPREP logo" class="auth-brand-logo" />
            <div class="auth-brand-copy">
              <p class="auth-eyebrow">GOUDAPREP</p>
              <h1>Welcome back to GOUDAPREP.</h1>
            </div>
          </div>
          <p class="auth-subtitle">
            Sign in to continue your prep, manage tests, and keep every session organized in one place.
          </p>

          <div class="auth-chip-row">
            <span>Test management</span>
            <span>Student access</span>
            <span>Focused practice</span>
          </div>
        </section>

        <section class="auth-form-wrap">
          <div class="auth-form auth-form-wide">
            <p class="auth-form-kicker">GOUDAPREP</p>
            <h2>Sign in</h2>
            <p class="auth-form-sub">Continue to your dashboard and pick up where you left off.</p>

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
    .auth-brand {
      display: grid;
      gap: 16px;
      width: 100%;
      margin-bottom: 10px;
    }

    .auth-brand-logo {
      display: block;
      width: 100%;
      height: auto;
      border-radius: 24px;
      object-fit: contain;
      box-shadow: var(--shadow-card);
      border: 1px solid var(--border);
      background: #fff;
      padding: 14px;
      box-sizing: border-box;
    }

    .auth-brand-copy {
      width: 100%;
    }

    .auth-brand-copy h1 {
      margin: 0;
    }

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

    @media (max-width: 520px) {
      .auth-brand-logo {
        border-radius: 18px;
        padding: 10px;
      }
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
