import { Inject, Injectable, PLATFORM_ID, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { Observable, tap } from 'rxjs';
import { isPlatformBrowser } from '@angular/common';

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role?: 'Host' | 'Player';
}

export interface AuthResponse {
  id: number;
  email: string;
  token: string;
  roles: string[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'quiz_token';
  private readonly roleKey = 'quiz_role';
  private readonly isBrowser: boolean;
  readonly token = signal<string | null>(null);
  readonly role = signal<string | null>(null);

  constructor(
    private http: HttpClient,
    private router: Router,
    @Inject(PLATFORM_ID) platformId: object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
    const storedToken = this.readStorage(this.tokenKey);
    const storedRole = this.readStorage(this.roleKey);
    const hasValidStoredToken = !!storedToken && !this.isTokenExpired(storedToken);

    this.token.set(hasValidStoredToken ? storedToken : null);
    this.role.set(hasValidStoredToken ? storedRole : null);

    if (storedToken && !hasValidStoredToken) {
      this.clearSession();
    }

    if (hasValidStoredToken && !storedRole) {
      const extracted = this.extractRole(storedToken);
      if (extracted) {
        this.writeStorage(this.roleKey, extracted);
        this.role.set(extracted);
      }
    }
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiBaseUrl}/auth/login`, { email, password }).pipe(
      tap((res) => {
        const token = (res as any)?.token ?? (res as any)?.Token;
        if (token) {
          this.setSession(token, this.pickRoleFromResponse(res));
        }
      })
    );
  }

  register(payload: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiBaseUrl}/auth/register`, payload).pipe(
      tap((res) => {
        const token = (res as any)?.token ?? (res as any)?.Token;
        if (token) {
          this.setSession(token, this.pickRoleFromResponse(res));
        }
      })
    );
  }

  setSession(token: string, roleFromResponse?: string | null): void {
    this.writeStorage(this.tokenKey, token);
    this.token.set(token);

    const role = roleFromResponse || this.extractRole(token);
    if (role) {
      this.writeStorage(this.roleKey, role);
      this.role.set(role);
    } else {
      this.removeStorage(this.roleKey);
      this.role.set(null);
    }
  }

  logout(): void {
    this.removeStorage(this.tokenKey);
    this.removeStorage(this.roleKey);
    this.token.set(null);
    this.role.set(null);
    this.router.navigate(['/auth/login']);
  }

  isLoggedIn(): boolean {
    const token = this.token();
    if (!token) {
      return false;
    }

    if (this.isTokenExpired(token)) {
      this.clearSession();
      return false;
    }

    return true;
  }

  private extractRole(token: string): string | null {
    try {
      const payloadPart = token.split('.')[1];
      if (!payloadPart) return null;

      const payload = JSON.parse(this.decodeBase64Url(payloadPart));
      const roleValue = payload['role'] || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null;
      if (Array.isArray(roleValue)) {
        return roleValue[0] ?? null;
      }
      return roleValue;
    } catch {
      return null;
    }
  }

  private pickRoleFromResponse(response: AuthResponse | any): string | null {
    const roles = response?.roles ?? response?.Roles;
    if (Array.isArray(roles) && roles.length > 0) {
      return String(roles[0]);
    }
    return null;
  }

  private decodeBase64Url(value: string): string {
    const base64 = value.replace(/-/g, '+').replace(/_/g, '/');
    const padded = base64 + '='.repeat((4 - (base64.length % 4)) % 4);
    return atob(padded);
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payloadPart = token.split('.')[1];
      if (!payloadPart) return true;

      const payload = JSON.parse(this.decodeBase64Url(payloadPart));
      const exp = Number(payload?.exp);

      if (!exp || Number.isNaN(exp)) {
        return false;
      }

      const nowSeconds = Math.floor(Date.now() / 1000);
      return exp <= nowSeconds;
    } catch {
      return true;
    }
  }

  private readStorage(key: string): string | null {
    if (!this.isBrowser) return null;
    return localStorage.getItem(key);
  }

  private writeStorage(key: string, value: string): void {
    if (!this.isBrowser) return;
    localStorage.setItem(key, value);
  }

  private removeStorage(key: string): void {
    if (!this.isBrowser) return;
    localStorage.removeItem(key);
  }

  private clearSession(): void {
    this.removeStorage(this.tokenKey);
    this.removeStorage(this.roleKey);
    this.token.set(null);
    this.role.set(null);
  }
}

