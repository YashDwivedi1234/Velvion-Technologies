import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/landing/landing.component').then(m => m.LandingComponent)
  },
  {
    path: 'auth',
    loadComponent: () => import('./pages/auth/auth.component').then(m => m.AuthComponent)
  },
  {
    path: 'login',
    redirectTo: 'auth',
    pathMatch: 'full'
  },
  {
    path: 'register',
    redirectTo: 'auth',
    pathMatch: 'full'
  },
  {
    path: 'admin',
    loadComponent: () => import('./pages/dashboard-layout/dashboard-layout.component').then(m => m.DashboardLayoutComponent),
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./pages/admin/dashboard-home/dashboard-home.component').then(m => m.DashboardHomeComponent)
      },
      {
        path: 'users',
        loadComponent: () => import('./pages/admin/users/users.component').then(m => m.UsersComponent)
      },
      {
        path: 'roles',
        loadComponent: () => import('./pages/admin/roles/roles.component').then(m => m.RolesComponent)
      },
      {
        path: 'permissions',
        loadComponent: () => import('./pages/admin/role-permissions/role-permissions.component').then(m => m.RolePermissionsComponent)
      },
      {
        path: 'masters/menus',
        loadComponent: () => import('./pages/admin/masters/menus-master/menus-master.component').then(m => m.MenusMasterComponent)
      },
      {
        path: 'masters/services',
        loadComponent: () => import('./pages/admin/masters/services-master/services-master.component').then(m => m.ServicesMasterComponent)
      },
      {
        path: 'masters/settings',
        loadComponent: () => import('./pages/admin/masters/settings-master/settings-master.component').then(m => m.SettingsMasterComponent)
      },
      {
        path: 'blogs',
        loadComponent: () => import('./pages/admin/blogs/blogs.component').then(m => m.BlogsComponent)
      },
      {
        path: 'portfolios',
        loadComponent: () => import('./pages/admin/portfolios/portfolios.component').then(m => m.PortfoliosComponent)
      },
      {
        path: 'team',
        loadComponent: () => import('./pages/admin/team/team.component').then(m => m.TeamComponent)
      },
      {
        path: 'testimonials',
        loadComponent: () => import('./pages/admin/testimonials/testimonials.component').then(m => m.TestimonialsComponent)
      },
      {
        path: 'inquiries',
        loadComponent: () => import('./pages/admin/inquiries/inquiries.component').then(m => m.InquiriesComponent)
      },
      {
        path: 'audit-logs',
        loadComponent: () => import('./pages/admin/audit-logs/audit-logs.component').then(m => m.AuditLogsComponent)
      },
      {
        path: 'careers/job-postings',
        loadComponent: () => import('./pages/admin/careers/job-postings/job-postings.component').then(m => m.JobPostingsComponent)
      },
      {
        path: 'careers/job-applications',
        loadComponent: () => import('./pages/admin/careers/job-applications/job-applications.component').then(m => m.JobApplicationsComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./pages/admin/my-profile/my-profile.component').then(m => m.MyProfileComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
