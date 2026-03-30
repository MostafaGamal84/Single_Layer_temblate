import { CommonModule } from '@angular/common';
import { Component, HostListener, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { ThemeService } from './core/services/theme.service';
import { ConfirmDialogComponent } from './shared/confirm-dialog.component';
import { ToastComponent } from './shared/toast.component';

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, ConfirmDialogComponent, ToastComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  readonly auth = inject(AuthService);
  readonly theme = inject(ThemeService);
  readonly mobileSidebarOpen = signal(false);

  readonly currentThemeLabel = computed(() => this.theme.isDark() ? 'Dark Canvas' : 'Soft Light');
  readonly themeActionLabel = computed(() => this.theme.isDark() ? 'Switch To Light' : 'Switch To Dark');
  readonly brandModeLabel = computed(() => this.theme.isDark() ? 'Midnight Edition' : 'Studio Light');
  readonly shellKicker = computed(() => {
    const role = this.auth.role();
    if (role === 'Admin' || role === 'Host') return 'Control Deck';
    if (role === 'Player') return 'Player Flow';
    return 'Quiz Studio';
  });
  readonly shellTitle = computed(() => {
    const role = this.auth.role();
    if (role === 'Admin' || role === 'Host') return 'Mission Control';
    if (role === 'Player') return 'Momentum Lounge';
    return 'Practice Studio';
  });
  readonly workspaceSummary = computed(() => {
    const role = this.auth.role();
    if (role === 'Admin' || role === 'Host') {
      return 'Shape quizzes, manage approvals, run live rooms, and track outcomes from one polished command surface.';
    }
    if (role === 'Player') {
      return 'Move between live sessions, practice mode, and your own results with a calmer, faster workspace.';
    }
    return 'A cleaner quiz system built for focused work and smooth participation.';
  });
  readonly workspaceHeadline = computed(() => {
    const role = this.auth.role();
    if (role === 'Admin' || role === 'Host') {
      return 'Build, launch, and monitor every test from a modern workspace that stays clear under pressure.';
    }
    if (role === 'Player') {
      return 'Practice smarter, join faster, and keep your full exam journey in one sleek dashboard.';
    }
    return 'A refined home for every quiz workflow.';
  });
  readonly primaryActionLabel = computed(() => {
    const role = this.auth.role();
    return role === 'Admin' || role === 'Host' ? 'Open Quizzes' : 'Start Practice';
  });
  readonly primaryActionLink = computed(() => {
    const role = this.auth.role();
    return role === 'Admin' || role === 'Host' ? '/quizzes' : '/test-mode';
  });
  readonly primaryActionIcon = computed(() => {
    const role = this.auth.role();
    return role === 'Admin' || role === 'Host' ? 'pi pi-sparkles' : 'pi pi-play-circle';
  });
  readonly secondaryActionLabel = computed(() => {
    const role = this.auth.role();
    return role === 'Admin' || role === 'Host' ? 'Run Session' : 'View History';
  });
  readonly secondaryActionLink = computed(() => {
    const role = this.auth.role();
    return role === 'Admin' || role === 'Host' ? '/game-sessions' : '/player/history';
  });
  readonly secondaryActionIcon = computed(() => {
    const role = this.auth.role();
    return role === 'Admin' || role === 'Host' ? 'pi pi-bolt' : 'pi pi-history';
  });
  readonly menuButtonLabel = computed(() => this.mobileSidebarOpen() ? 'Close menu' : 'Open menu');
  readonly menuButtonText = computed(() => this.mobileSidebarOpen() ? 'Close' : 'Menu');
  readonly showMenu = computed(() => this.auth.isLoggedIn() && this.auth.status() === 1);

  toggleTheme(): void {
    this.theme.toggleTheme();
  }

  toggleSidebar(): void {
    this.mobileSidebarOpen.update((isOpen) => !isOpen);
  }

  closeSidebar(): void {
    this.mobileSidebarOpen.set(false);
  }

  logout(): void {
    this.closeSidebar();
    this.auth.logout();
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.mobileSidebarOpen()) {
      this.closeSidebar();
    }
  }
}
