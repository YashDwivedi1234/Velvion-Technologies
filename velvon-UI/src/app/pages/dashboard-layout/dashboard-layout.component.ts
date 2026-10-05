import { Component, OnInit, inject, signal, computed, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterOutlet, RouterLinkActive, NavigationStart, NavigationEnd, NavigationCancel, NavigationError } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ThemeService, LogoComponent, IconComponent, ToastService, ConfirmService } from '../../shared';

interface PageInfo {
  section: string;
  breadcrumb: string;
  title: string;
}

const ROUTE_MAP: Record<string, PageInfo> = {
  '/admin/dashboard': { section: 'Dashboard', breadcrumb: 'Dashboard Overview', title: 'Dashboard' },
  '/admin/users': { section: 'User Management', breadcrumb: 'Users', title: 'Users Management' },
  '/admin/roles': { section: 'User Management', breadcrumb: 'Roles', title: 'Role Management' },
  '/admin/permissions': { section: 'User Management', breadcrumb: 'Permissions', title: 'Role Permissions' },
  '/admin/masters/menus': { section: 'Masters', breadcrumb: 'Menus', title: 'Menus' },
  '/admin/masters/services': { section: 'Masters', breadcrumb: 'Service Catalog', title: 'Services' },
  '/admin/masters/categories': { section: 'Masters', breadcrumb: 'Project Verticals', title: 'Categories & Industries' },
  '/admin/masters/departments': { section: 'Masters', breadcrumb: 'Organization Hierarchy', title: 'Departments' },
  '/admin/masters/designations': { section: 'Masters', breadcrumb: 'Designations & Roles', title: 'Designations' },
  '/admin/masters/clients': { section: 'Masters', breadcrumb: 'Ecosystem Network', title: 'Clients & Partners' },
  '/admin/masters/faqs': { section: 'Masters', breadcrumb: 'Help & FAQs', title: 'FAQs & Knowledge Base' },
  '/admin/masters/settings': { section: 'Masters', breadcrumb: 'Site Settings', title: 'Settings' },
  '/admin/blogs': { section: 'Content & Media', breadcrumb: 'Blog Posts', title: 'Blog Articles' },
  '/admin/portfolios': { section: 'Content & Media', breadcrumb: 'Showcase Portfolio', title: 'Portfolio Projects' },
  '/admin/team': { section: 'Operations & HR', breadcrumb: 'Company Team', title: 'Team Members' },
  '/admin/testimonials': { section: 'Content & Media', breadcrumb: 'Client Reviews', title: 'Client Testimonials' },
  '/admin/careers/job-postings': { section: 'Careers & Hiring', breadcrumb: 'Job Openings', title: 'Job Postings' },
  '/admin/careers/job-applications': { section: 'Careers & Hiring', breadcrumb: 'Applications ATS', title: 'Candidate Applications' },
  '/admin/inquiries': { section: 'Operations & Leads', breadcrumb: 'Customer Inquiries', title: 'Customer Inquiries' },
  '/admin/audit-logs': { section: 'System & Security', breadcrumb: 'Audit Logs', title: 'Database Audit Trails' },
  '/admin/profile': { section: 'Account & Settings', breadcrumb: 'My Profile', title: 'User Profile & Settings' }
};

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
  private confirmService = inject(ConfirmService);

  currentYear = new Date().getFullYear();
  sidebarCollapsed = signal<boolean>(false);
  mobileNavOpen = signal<boolean>(false);
  userDropdownOpen = signal<boolean>(false);
  pageLoading = signal<boolean>(false);
  currentUrl = signal<string>(this.router.url);

  getInitials(name?: string | null): string {
    if (!name) return 'YD';
    const parts = name.trim().split(/\s+/).filter(Boolean);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return parts[0].substring(0, 2).toUpperCase();
  }

  currentPageInfo = computed<PageInfo>(() => {
    const cleanUrl = this.currentUrl().split('?')[0].split('#')[0];
    return ROUTE_MAP[cleanUrl] || {
      section: 'Admin Portal',
      breadcrumb: 'Control Panel',
      title: 'Velvion Dashboard'
    };
  });

  // Single active accordion section (auto-closes other sections)
  activeNavSection = signal<string | null>('user-mgmt');

  // Computed backward-compatible signals for section states
  userMgmtOpen = computed<boolean>(() => this.isSectionOpen('user-mgmt'));
  mastersOpen = computed<boolean>(() => this.isSectionOpen('masters'));
  contentOpen = computed<boolean>(() => this.isSectionOpen('content'));
  careersOpen = computed<boolean>(() => this.isSectionOpen('careers'));
  operationsOpen = computed<boolean>(() => this.isSectionOpen('operations'));

  toggleNavSection(section: string): void {
    if (this.activeNavSection() === section) {
      this.activeNavSection.set(null);
    } else {
      this.activeNavSection.set(section);
    }
  }

  isSectionOpen(section: string): boolean {
    return this.activeNavSection() === section;
  }

  private syncActiveSectionFromUrl(url: string): void {
    if (url.includes('/admin/users') || url.includes('/admin/roles') || url.includes('/admin/permissions')) {
      this.activeNavSection.set('user-mgmt');
    } else if (url.includes('/admin/masters')) {
      this.activeNavSection.set('masters');
    } else if (url.includes('/admin/blogs') || url.includes('/admin/portfolios') || url.includes('/admin/team') || url.includes('/admin/testimonials')) {
      this.activeNavSection.set('content');
    } else if (url.includes('/admin/careers')) {
      this.activeNavSection.set('careers');
    } else if (url.includes('/admin/inquiries') || url.includes('/admin/audit-logs')) {
      this.activeNavSection.set('operations');
    }
  }

  ngOnInit(): void {
    this.currentUrl.set(this.router.url);
    this.syncActiveSectionFromUrl(this.router.url);

    this.router.events.subscribe((event) => {
      if (event instanceof NavigationStart) {
        this.pageLoading.set(true);
      } else if (
        event instanceof NavigationEnd ||
        event instanceof NavigationCancel ||
        event instanceof NavigationError
      ) {
        if (event instanceof NavigationEnd) {
          const url = event.urlAfterRedirects || event.url;
          this.currentUrl.set(url);
          this.syncActiveSectionFromUrl(url);
        }
        setTimeout(() => {
          this.pageLoading.set(false);
        }, 220);
      }
    });

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

  toggleUserDropdown(event?: MouseEvent): void {
    if (event) {
      event.stopPropagation();
    }
    this.userDropdownOpen.set(!this.userDropdownOpen());
  }

  closeUserDropdown(): void {
    this.userDropdownOpen.set(false);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.user-header-profile-container')) {
      this.userDropdownOpen.set(false);
    }
  }

  async logout(): Promise<void> {
    this.userDropdownOpen.set(false);
    const confirmed = await this.confirmService.confirm({
      title: 'Logout Confirmation',
      message: 'Are you sure you want to logout ?',
      confirmText: 'Logout',
      cancelText: 'Cancel',
      type: 'warning',
      icon: 'power'
    });

    if (confirmed) {
      this.toastService.info('You have logged out securely.', 'Session Ended');
      this.authService.logout();
    }
  }
}
