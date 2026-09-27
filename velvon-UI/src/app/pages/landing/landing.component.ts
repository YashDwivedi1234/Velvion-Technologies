import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AdminDataService } from '../../core/services/admin-data.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastService, LogoComponent, IconComponent, ThemeService } from '../../shared';
import { ServiceMasterDto, PortfolioDto, TestimonialDto } from '../../core/models/api.models';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    LogoComponent,
    IconComponent
  ],
  templateUrl: './landing.component.html',
  styleUrls: ['./landing.component.css']
})
export class LandingComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private adminDataService = inject(AdminDataService);
  public authService = inject(AuthService);
  private toastService = inject(ToastService);
  public themeService = inject(ThemeService);

  inquiryForm!: FormGroup;
  inquirySubmitting = signal<boolean>(false);

  services = signal<ServiceMasterDto[]>([]);
  portfolios = signal<PortfolioDto[]>([]);
  testimonials = signal<TestimonialDto[]>([]);

  stats = [
    { value: '99.99%', label: 'Cloud Uptime SLA', icon: 'shield' },
    { value: '< 15ms', label: 'Microservice Latency', icon: 'zap' },
    { value: '500K+', label: 'Concurrent Transactions', icon: 'layers' },
    { value: '100%', label: 'REST & OpenAPI Standard', icon: 'check-circle' }
  ];

  features = [
    {
      title: 'Fullstack .NET & Angular',
      description: 'Engineered with C# 13, .NET 10 Web API, Entity Framework Core, MySQL, and Angular 19 Signals.',
      icon: 'layers'
    },
    {
      title: 'Dynamic RBAC Security',
      description: 'Granular permissions matrix per role for menu access, create, edit, and deletion capabilities.',
      icon: 'shield'
    },
    {
      title: 'Comprehensive Masters System',
      description: 'Centralized master controllers and responsive UI forms for roles, menus, services, and system settings.',
      icon: 'grid'
    },
    {
      title: 'Real-time Database Sync',
      description: 'End-to-end relational data integrity with Pomelo MySQL EF Core and responsive state notifications.',
      icon: 'database'
    }
  ];

  ngOnInit(): void {
    this.initForm();
    this.loadData();
  }

  private initForm(): void {
    this.inquiryForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      phone: [''],
      subject: ['Enterprise Architecture Consultation', Validators.required],
      message: ['', [Validators.required, Validators.minLength(10)]]
    });
  }

  private loadData(): void {
    // Load Services
    this.adminDataService.getServices(true).subscribe(res => {
      if (res.success && res.data && res.data.length > 0) {
        this.services.set(res.data);
      } else {
        // Fallback default services
        this.services.set([
          {
            id: 1,
            serviceName: 'Enterprise Cloud Architecture',
            description: 'Scalable distributed systems, microservices orchestration, and high-availability cloud infrastructure.',
            iconUrl: 'cloud',
            isActive: true,
            createdAt: '',
            updatedAt: ''
          },
          {
            id: 2,
            serviceName: 'AI & Data Engineering',
            description: 'Predictive intelligence pipelines, LLM integration, and big data real-time streaming analytics.',
            iconUrl: 'zap',
            isActive: true,
            createdAt: '',
            updatedAt: ''
          },
          {
            id: 3,
            serviceName: 'Modern Full-Stack Engineering',
            description: 'High performance web applications using Angular 19 Signals, ASP.NET Core, and robust relational schemas.',
            iconUrl: 'layers',
            isActive: true,
            createdAt: '',
            updatedAt: ''
          },
          {
            id: 4,
            serviceName: 'Cybersecurity & Zero Trust RBAC',
            description: 'End-to-end cryptographic hashing, role-based permission matrices, and compliant audit trails.',
            iconUrl: 'shield',
            isActive: true,
            createdAt: '',
            updatedAt: ''
          }
        ]);
      }
    });

    // Load Portfolios
    this.adminDataService.getPortfolios(true).subscribe(res => {
      if (res.success && res.data && res.data.length > 0) {
        this.portfolios.set(res.data);
      }
    });

    // Load Testimonials
    this.adminDataService.getTestimonials(true).subscribe(res => {
      if (res.success && res.data && res.data.length > 0) {
        this.testimonials.set(res.data);
      }
    });
  }

  onSubmitInquiry(): void {
    if (this.inquiryForm.invalid) {
      this.inquiryForm.markAllAsTouched();
      this.toastService.error('Please fill in your name, email, and message correctly.', 'Form Incomplete');
      return;
    }

    this.inquirySubmitting.set(true);
    const val = this.inquiryForm.value;

    this.adminDataService.saveInquiry({
      fullName: val.fullName.trim(),
      email: val.email.trim(),
      phone: val.phone?.trim(),
      subject: val.subject,
      message: val.message.trim()
    }).subscribe({
      next: (res) => {
        this.inquirySubmitting.set(false);
        if (res.success) {
          this.toastService.success('Thank you! Your inquiry has been recorded in the database. Our team will contact you shortly.', 'Inquiry Sent');
          this.inquiryForm.reset({
            subject: 'Enterprise Architecture Consultation'
          });
        } else {
          this.toastService.error(res.message || 'Failed to submit inquiry.', 'Error');
        }
      },
      error: () => {
        this.inquirySubmitting.set(false);
        this.toastService.error('Connection error while sending inquiry.', 'Network Error');
      }
    });
  }

  navigateToLogin(): void {
    this.router.navigate(['/auth'], { queryParams: { mode: 'login' } });
  }

  navigateToRegister(): void {
    this.router.navigate(['/auth'], { queryParams: { mode: 'register' } });
  }
}
