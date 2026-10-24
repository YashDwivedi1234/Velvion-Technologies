import { Component, OnInit, OnDestroy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AdminDataService } from '../../../core/services/admin-data.service';
import { AuthService } from '../../../core/services/auth.service';
import { IconComponent, ToastService } from '../../../shared';
import { UserDto, ContactInquiryDto, AllMastersDto } from '../../../core/models/api.models';

export interface ChartPoint {
  x: number;
  y: number;
  val: string;
  label: string;
}

export interface ChartDataset {
  areaPath: string;
  strokePath: string;
  labels: string[];
  points: ChartPoint[];
  highlights: { label: string; value: string }[];
}

@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  imports: [CommonModule, RouterLink, IconComponent],
  templateUrl: './dashboard-home.component.html',
  styleUrls: ['./dashboard-home.component.css']
})
export class DashboardHomeComponent implements OnInit, OnDestroy {
  private adminDataService = inject(AdminDataService);
  public authService = inject(AuthService);
  private toastService = inject(ToastService);

  loading = signal<boolean>(true);
  isSyncing = signal<boolean>(false);
  lastSyncTime = signal<string>('Just now');
  currentTime = signal<string>('');
  private timeIntervalId: any = null;

  users = signal<UserDto[]>([]);
  inquiries = signal<ContactInquiryDto[]>([]);
  allMasters = signal<AllMastersDto | null>(null);

  totalUsersCount = signal<number>(0);
  totalRolesCount = signal<number>(0);
  totalMenusCount = signal<number>(0);
  totalServicesCount = signal<number>(0);
  totalInquiriesCount = signal<number>(0);

  // Filters & Interactivity
  userSearchQuery = signal<string>('');
  userRoleFilter = signal<string>('all');
  timeRange = signal<'7d' | '30d' | '90d'>('30d');
  hoveredPoint = signal<ChartPoint | null>(null);
  selectedInquiry = signal<ContactInquiryDto | null>(null);

  // Role-Aware Computed Signals
  isSuperAdmin = computed(() => this.authService.isSuperAdmin() || this.authService.isAdmin());
  isDeveloper = computed(() => this.authService.isDeveloper());
  isManager = computed(() => this.authService.isManager());
  isStandardUser = computed(() => !this.authService.isSuperAdmin() && !this.authService.isAdmin() && !this.authService.isDeveloper() && !this.authService.isManager());
  currentRole = computed(() => this.authService.currentUser()?.roleName || 'Super Admin');

  availableRoles = ['Super Admin', 'Developer', 'HR Manager', 'User'];

  switchRole(role: string): void {
    let roleId = 1;
    if (role === 'Developer') roleId = 2;
    if (role === 'HR Manager') roleId = 3;
    if (role === 'User') roleId = 4;
    this.authService.switchRole(role, roleId);
    this.toastService.info(`Dashboard role adapted to: ${role}`, 'Role Adapted');
  }



  roleShortcuts = computed(() => {
    if (this.isDeveloper()) {
      return [
        { title: 'Services Catalog', desc: 'Enterprise Offerings & APIs', icon: 'layers', link: '/admin/masters/services', color: 'cyan' },
        { title: 'Menu Endpoints', desc: 'Active Navigation Routes', icon: 'grid', link: '/admin/permissions', color: 'purple' },
        { title: 'Site Settings', desc: 'Configuration Parameters', icon: 'settings', link: '/admin/masters/settings', color: 'blue' },
        { title: 'Audit Trail Logs', desc: 'Activity & Query Tracking', icon: 'activity', link: '/admin/audit-logs', color: 'amber' }
      ];
    } else if (this.isManager()) {
      return [
        { title: 'Team Directory', desc: 'Staff & Organizational Chart', icon: 'user', link: '/admin/team', color: 'purple' },
        { title: 'Job Postings', desc: 'Career Listings & Openings', icon: 'briefcase', link: '/admin/careers/job-postings', color: 'blue' },
        { title: 'Applications ATS', desc: 'Candidate Resumes & Hiring', icon: 'users', link: '/admin/careers/job-applications', color: 'cyan' },
        { title: 'Customer Inquiries', desc: 'Leads & Communications', icon: 'mail', link: '/admin/inquiries', color: 'amber' }
      ];
    } else if (this.isStandardUser()) {
      return [
        { title: 'Services', desc: 'Explore Enterprise Catalog', icon: 'layers', link: '/admin/masters/services', color: 'cyan' },
        { title: 'Showcase Portfolios', desc: 'Our Finished Solutions', icon: 'folder', link: '/admin/portfolios', color: 'purple' },
        { title: 'FAQs & Help', desc: 'Knowledge Base & Guides', icon: 'help-circle', link: '/admin/masters/faqs', color: 'blue' },
        { title: 'My Profile', desc: 'Manage Credentials & Info', icon: 'user', link: '/admin/profile', color: 'amber' }
      ];
    } else {
      return [
        { title: 'Users Directory', desc: 'Add, Edit & Filter Members', icon: 'users', link: '/admin/users', color: 'cyan' },
        { title: 'Role Management', desc: 'Access Control & Security', icon: 'shield', link: '/admin/roles', color: 'purple' },
        { title: 'Permission Matrix', desc: 'Bulk RBAC Capabilities', icon: 'check-circle', link: '/admin/permissions', color: 'blue' },
        { title: 'Services Catalog', desc: 'Enterprise Service Offerings', icon: 'layers', link: '/admin/masters/services', color: 'amber' }
      ];
    }
  });

  // Computed Metrics
  activeUsersCount = computed(() => this.users().filter(u => u.isActive).length);
  unresolvedInquiriesCount = computed(() => this.inquiries().filter(i => !i.isResolved).length);

  filteredUsers = computed(() => {
    let list = this.users();
    const query = this.userSearchQuery().trim().toLowerCase();
    const role = this.userRoleFilter().toLowerCase();

    if (role !== 'all') {
      list = list.filter(u => (u.roleName || '').toLowerCase().includes(role));
    }

    if (query) {
      list = list.filter(u =>
        u.fullName.toLowerCase().includes(query) ||
        u.email.toLowerCase().includes(query) ||
        (u.roleName || '').toLowerCase().includes(query)
      );
    }

    return list;
  });

  chartData = computed<ChartDataset>(() => {
    const range = this.timeRange();
    if (range === '7d') {
      return {
        areaPath: 'M 0 160 C 50 140, 100 80, 150 95 C 200 110, 250 40, 300 55 C 350 70, 400 30, 450 20 L 450 200 L 0 200 Z',
        strokePath: 'M 0 160 C 50 140, 100 80, 150 95 C 200 110, 250 40, 300 55 C 350 70, 400 30, 450 20',
        labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
        points: [
          { x: 0, y: 160, val: '420 req/s', label: 'Mon' },
          { x: 75, y: 125, val: '650 req/s', label: 'Tue' },
          { x: 150, y: 95, val: '580 req/s', label: 'Wed' },
          { x: 225, y: 70, val: '890 req/s', label: 'Thu' },
          { x: 300, y: 55, val: '760 req/s', label: 'Fri' },
          { x: 375, y: 35, val: '950 req/s', label: 'Sat' },
          { x: 450, y: 20, val: '1,120 req/s', label: 'Sun' }
        ],
        highlights: [
          { label: 'Throughput', value: '1,120 req/s' },
          { label: 'Avg Latency', value: '14ms' },
          { label: 'SLA Uptime', value: '100%' }
        ]
      };
    } else if (range === '30d') {
      return {
        areaPath: 'M 0 170 C 60 150, 120 120, 180 80 C 240 40, 300 70, 360 45 C 400 30, 430 15, 450 10 L 450 200 L 0 200 Z',
        strokePath: 'M 0 170 C 60 150, 120 120, 180 80 C 240 40, 300 70, 360 45 C 400 30, 430 15, 450 10',
        labels: ['Week 1', 'Week 2', 'Week 3', 'Week 4'],
        points: [
          { x: 30, y: 160, val: '2.4k inquiries', label: 'W1' },
          { x: 150, y: 105, val: '5.8k inquiries', label: 'W2' },
          { x: 280, y: 60, val: '8.2k inquiries', label: 'W3' },
          { x: 420, y: 20, val: '12.4k inquiries', label: 'W4' }
        ],
        highlights: [
          { label: 'Monthly Growth', value: '+34.2%' },
          { label: 'Lead Velocity', value: '4.8h' },
          { label: 'Conversion', value: '88.5%' }
        ]
      };
    } else {
      return {
        areaPath: 'M 0 180 C 80 160, 150 90, 225 70 C 300 50, 375 25, 450 5 L 450 200 L 0 200 Z',
        strokePath: 'M 0 180 C 80 160, 150 90, 225 70 C 300 50, 375 25, 450 5',
        labels: ['Month 1', 'Month 2', 'Month 3'],
        points: [
          { x: 40, y: 170, val: 'Q1 Launch', label: 'M1' },
          { x: 225, y: 70, val: 'Scaling Tier', label: 'M2' },
          { x: 430, y: 10, val: 'Global Live', label: 'M3' }
        ],
        highlights: [
          { label: 'Quarterly Volume', value: '38.6k' },
          { label: 'Security Breaches', value: '0' },
          { label: 'High Availability', value: '99.99%' }
        ]
      };
    }
  });

  ngOnInit(): void {
    this.updateCurrentTime();
    if (typeof window !== 'undefined') {
      this.timeIntervalId = setInterval(() => this.updateCurrentTime(), 1000);
    }
    this.fetchDashboardData();
  }

  ngOnDestroy(): void {
    if (this.timeIntervalId) {
      clearInterval(this.timeIntervalId);
    }
  }

  updateCurrentTime(): void {
    const now = new Date();
    this.currentTime.set(now.toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: true
    }));
  }

  fetchDashboardData(): void {
    this.loading.set(true);
    this.isSyncing.set(true);

    // 1. Fetch Users
    this.adminDataService.getUsers().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.users.set(res.data);
          this.totalUsersCount.set(res.data.length);
        }
      }
    });

    // 2. Fetch Inquiries
    this.adminDataService.getInquiries().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.inquiries.set(res.data);
          this.totalInquiriesCount.set(res.data.length);
        }
      }
    });

    // 3. Fetch All Masters
    this.adminDataService.getAllMasters().subscribe({
      next: (res) => {
        this.loading.set(false);
        this.isSyncing.set(false);
        const now = new Date();
        this.lastSyncTime.set(now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }));
        if (res.success && res.data) {
          this.allMasters.set(res.data);
          this.totalRolesCount.set(res.data.roles?.length || 0);
          this.totalMenusCount.set(res.data.menus?.length || 0);
          this.totalServicesCount.set(res.data.services?.length || 0);
        }
      },
      error: () => {
        this.loading.set(false);
        this.isSyncing.set(false);
      }
    });

    // 4. Try Dashboard Stats API for additional synchronization
    this.adminDataService.getDashboardStats().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          if (res.data.totalUsers) this.totalUsersCount.set(res.data.totalUsers);
          if (res.data.totalRoles) this.totalRolesCount.set(res.data.totalRoles);
          if (res.data.totalInquiries) this.totalInquiriesCount.set(res.data.totalInquiries);
        }
      },
      error: () => {}
    });
  }

  refreshDashboard(): void {
    this.fetchDashboardData();
    this.toastService.info('Dashboard synced with MySQL database in real time', 'Database Synced');
  }

  onSearchInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.userSearchQuery.set(input.value || '');
  }

  setUserRoleFilter(role: string): void {
    this.userRoleFilter.set(role);
  }

  setTimeRange(range: '7d' | '30d' | '90d'): void {
    this.timeRange.set(range);
    this.hoveredPoint.set(null);
  }

  getGreetingTime(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good morning';
    if (hour < 18) return 'Good afternoon';
    return 'Good evening';
  }

  getFormattedDate(): string {
    return new Date().toLocaleDateString('en-US', {
      weekday: 'long',
      month: 'short',
      day: 'numeric',
      year: 'numeric'
    });
  }

  getUserInitials(name?: string | null): string {
    if (!name) return 'U';
    const parts = name.trim().split(/\s+/).filter(Boolean);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return parts[0].substring(0, 2).toUpperCase();
  }

  getRoleBadgeClass(roleName?: string): string {
    const r = (roleName || '').toLowerCase();
    if (r.includes('super')) return 'role-super';
    if (r.includes('dev')) return 'role-dev';
    if (r.includes('admin')) return 'role-admin';
    return 'role-default';
  }

  openInquiry(inq: ContactInquiryDto): void {
    this.selectedInquiry.set(inq);
  }

  closeInquiry(): void {
    this.selectedInquiry.set(null);
  }
}
