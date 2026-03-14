import { CommonModule } from '@angular/common';
import { Component, HostListener, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { ThemeService } from './core/services/theme.service';
import { ConfirmDialogComponent } from './shared/confirm-dialog.component';

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, ConfirmDialogComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  readonly auth = inject(AuthService);
  readonly theme = inject(ThemeService);
  readonly mobileSidebarOpen = signal(false);

  readonly currentThemeLabel = computed(() => this.theme.isDark() ? 'Dark Mode' : 'Light Mode');
  readonly themeActionLabel = computed(() => this.theme.isDark() ? 'Switch To Light' : 'Switch To Dark');
  readonly shellTitle = computed(() => this.theme.isDark() ? 'Midnight Motion' : 'Daylight Motion');
  readonly menuButtonLabel = computed(() => this.mobileSidebarOpen() ? 'Close menu' : 'Open menu');
  readonly menuButtonText = computed(() => this.mobileSidebarOpen() ? 'Close' : 'Menu');

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
