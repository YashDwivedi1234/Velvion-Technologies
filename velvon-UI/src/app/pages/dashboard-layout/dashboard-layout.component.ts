import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterOutlet, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ThemeService, LogoComponent, IconComponent, ToastService } from '../../shared';

interface NavSection {
  title: string;
  isOpen: boolean;
  items: NavItem[];
}

interface NavItem {
  label: string;
  route: string;
  icon: string;
  badge?: string;
  badgeType?: 'primary' | 'success' | 'warning' | 'info';
}

@Component({
  selector: 'app-dashboard-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    LogoComponent,
    IconComponent
  ],
  templateUrl: './dashboard-layout.component.html',
  styleUrls: ['./dashboard-layout.component.css']
})
export class DashboardLayoutComponent implements OnInit {
  public authService = inject(AuthService);
  public themeService = inject(ThemeService);
  private router = inject(Router);
  private toastService = inject(ToastService);

  sidebarCollapsed = signal<boolean>(false);
  mobileNavOpen = signal<boolean>(false);

  // Nav sections with accordions
  userMgmtOpen = signal<boolean>(true);
  mastersOpen = signal<boolean>(true);
  contentOpen = signal<boolean>(true);
  operationsOpen = signal<boolean>(true);

  ngOnInit(): void {
    // If not logged in, we can either redirect to auth or provide a default preview session
    if (!this.authService.isLoggedIn()) {
      // Auto-set admin session for smooth seamless developer experience
      this.authService.currentUser.set({
        userId: 1,
        fullName: 'Yash Dwivedi',
        email: 'admin@velvion.com',
        roleId: 1,
        roleName: 'Super Admin'
      });
    }
  }

  toggleSidebar(): void {
    this.sidebarCollapsed.set(!this.sidebarCollapsed());
  }

  toggleMobileNav(): void {
    this.mobileNavOpen.set(!this.mobileNavOpen());
  }

  logout(): void {
    this.toastService.info('You have logged out securely.', 'Session Ended');
    this.authService.logout();
  }
}
