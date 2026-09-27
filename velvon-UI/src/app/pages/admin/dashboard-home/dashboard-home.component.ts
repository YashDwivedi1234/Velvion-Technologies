import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AdminDataService } from '../../../core/services/admin-data.service';
import { IconComponent, ToastService } from '../../../shared';
import { UserDto, ContactInquiryDto, AllMastersDto } from '../../../core/models/api.models';

@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  imports: [CommonModule, RouterLink, IconComponent],
  templateUrl: './dashboard-home.component.html',
  styleUrls: ['./dashboard-home.component.css']
})
export class DashboardHomeComponent implements OnInit {
  private adminDataService = inject(AdminDataService);
  private toastService = inject(ToastService);

  loading = signal<boolean>(true);
  users = signal<UserDto[]>([]);
  inquiries = signal<ContactInquiryDto[]>([]);
  allMasters = signal<AllMastersDto | null>(null);

  totalUsersCount = signal<number>(0);
  totalRolesCount = signal<number>(0);
  totalMenusCount = signal<number>(0);
  totalServicesCount = signal<number>(0);
  totalInquiriesCount = signal<number>(0);

  ngOnInit(): void {
    this.fetchDashboardData();
  }

  fetchDashboardData(): void {
    this.loading.set(true);

    // 1. Fetch Users
    this.adminDataService.getUsers().subscribe(res => {
      if (res.success && res.data) {
        this.users.set(res.data);
        this.totalUsersCount.set(res.data.length);
      }
    });

    // 2. Fetch Inquiries
    this.adminDataService.getInquiries().subscribe(res => {
      if (res.success && res.data) {
        this.inquiries.set(res.data);
        this.totalInquiriesCount.set(res.data.length);
      }
    });

    // 3. Fetch All Masters
    this.adminDataService.getAllMasters().subscribe(res => {
      this.loading.set(false);
      if (res.success && res.data) {
        this.allMasters.set(res.data);
        this.totalRolesCount.set(res.data.roles?.length || 0);
        this.totalMenusCount.set(res.data.menus?.length || 0);
        this.totalServicesCount.set(res.data.services?.length || 0);
      }
    });
  }

  refreshDashboard(): void {
    this.fetchDashboardData();
    this.toastService.info('Dashboard synced with MySQL database', 'Real-time Refresh');
  }
}
