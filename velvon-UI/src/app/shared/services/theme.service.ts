import { Injectable, signal, computed, effect, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export type AppTheme = 'dark' | 'light' | 'midnight';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private platformId = inject(PLATFORM_ID);
  public currentTheme = signal<AppTheme>('dark');
  public isDark = computed(() => this.currentTheme() !== 'light');

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      const saved = localStorage.getItem('velvion_theme') as AppTheme;
      if (saved && ['dark', 'light', 'midnight'].includes(saved)) {
        this.currentTheme.set(saved);
      }
      this.applyTheme(this.currentTheme());
    }

    effect(() => {
      const theme = this.currentTheme();
      if (isPlatformBrowser(this.platformId)) {
        this.applyTheme(theme);
        localStorage.setItem('velvion_theme', theme);
      }
    });
  }

  public setTheme(theme: AppTheme): void {
    this.currentTheme.set(theme);
  }

  public toggleTheme(): void {
    const current = this.currentTheme();
    if (current === 'dark') {
      this.currentTheme.set('light');
    } else if (current === 'light') {
      this.currentTheme.set('midnight');
    } else {
      this.currentTheme.set('dark');
    }
  }

  private applyTheme(theme: AppTheme): void {
    document.documentElement.setAttribute('data-theme', theme);
  }
}
