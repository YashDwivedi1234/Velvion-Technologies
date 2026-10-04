import { Component, OnInit, inject, signal, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, FormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AdminDataService } from '../../core/services/admin-data.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastService, LogoComponent, IconComponent, ThemeService } from '../../shared';
import { ServiceMasterDto, PortfolioDto, TestimonialDto, JobPostingDto, SaveJobApplicationDto } from '../../core/models/api.models';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
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
  private platformId = inject(PLATFORM_ID);

  // Mobile menu toggle
  mobileMenuOpen = signal<boolean>(false);

  // Forms
  inquiryForm!: FormGroup;
  inquirySubmitting = signal<boolean>(false);
  newsletterEmail = signal<string>('');
  newsletterSubmitting = signal<boolean>(false);

  // Job Application Modal Form
  applyForm!: FormGroup;
  applySubmitting = signal<boolean>(false);
  isApplyModalOpen = signal<boolean>(false);
  selectedJobForApply = signal<JobPostingDto | null>(null);

  // Dynamic Data Signals
  services = signal<ServiceMasterDto[]>([]);
  portfolios = signal<PortfolioDto[]>([]);
  testimonials = signal<TestimonialDto[]>([]);
  jobPostings = signal<JobPostingDto[]>([]);

  // Key Company Stats
  companyStats = [
    { value: '150+', label: 'Enterprise Deliveries', sublabel: 'Across Global Markets', icon: 'award' },
    { value: '99.9%', label: 'Client Satisfaction', sublabel: 'Long-term Partnerships', icon: 'shield' },
    { value: '50+', label: 'Technical Specialists', sublabel: 'Senior Engineers & Leads', icon: 'users' },
    { value: '45%', label: 'Operational Cost Reduction', sublabel: 'Through Intelligent Tech', icon: 'zap' }
  ];

  // Why Choose Us Pillars
  whyChooseUs = [
    {
      title: 'Architectural Excellence',
      description: 'We engineer robust, scalable systems built for high-throughput and zero-downtime enterprise operations.',
      icon: 'layers'
    },
    {
      title: 'Zero-Trust Security & RBAC',
      description: 'Enterprise governance with dynamic role authorization, cryptographic data encryption, and full auditability.',
      icon: 'shield'
    },
    {
      title: 'Agile Dedicated Teams',
      description: 'Senior engineering pods that integrate seamlessly with your organization to deliver rapid, measurable results.',
      icon: 'zap'
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
      phone: ['', [Validators.pattern(/^[0-9+() -]{7,20}$/)]],
      subject: ['Enterprise Digital Transformation Consultation', Validators.required],
      message: ['', [Validators.required, Validators.minLength(10)]]
    });

    this.applyForm = this.fb.group({
      jobPostingId: ['', Validators.required],
      applicantName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
      phone: ['', [Validators.maxLength(20)]],
      resumeUrl: ['', [Validators.maxLength(255)]],
      coverLetter: ['']
    });
  }

  private loadData(): void {
    // 1. Services
    this.adminDataService.getServices(true).subscribe({
      next: (res) => {
        if (res.success && res.data && res.data.length > 0) {
          this.services.set(res.data);
        } else {
          this.setDefaultServices();
        }
      },
      error: () => this.setDefaultServices()
    });

    // 2. Portfolios
    this.adminDataService.getPortfolios(true).subscribe({
      next: (res) => {
        if (res.success && res.data && res.data.length > 0) {
          this.portfolios.set(res.data);
        } else {
          this.setDefaultPortfolios();
        }
      },
      error: () => this.setDefaultPortfolios()
    });

    // 3. Testimonials
    this.adminDataService.getTestimonials(true).subscribe({
      next: (res) => {
        if (res.success && res.data && res.data.length > 0) {
          this.testimonials.set(res.data);
        } else {
          this.setDefaultTestimonials();
        }
      },
      error: () => this.setDefaultTestimonials()
    });

    // 4. Active Job Postings
    this.adminDataService.getJobPostings(true).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.jobPostings.set(res.data);
        }
      },
      error: () => {
        // Fallback demo job if offline
        this.jobPostings.set([]);
      }
    });
  }

  private setDefaultServices(): void {
    this.services.set([
      {
        id: 1,
        serviceName: 'Enterprise Custom Software Development',
        description: 'Bespoke, scalable digital platforms designed to streamline mission-critical business workflows and drive growth.',
        iconUrl: 'layers',
        isActive: true,
        createdAt: '',
        updatedAt: ''
      },
      {
        id: 2,
        serviceName: 'Cloud Infrastructure & DevOps',
        description: 'Multi-region cloud architectures, containerized microservices, CI/CD automation, and 99.99% high-availability SLA.',
        iconUrl: 'cloud',
        isActive: true,
        createdAt: '',
        updatedAt: ''
      },
      {
        id: 3,
        serviceName: 'AI & Business Process Automation',
        description: 'Intelligent decision pipelines, automated document processing, and predictive analytics for operational efficiency.',
        iconUrl: 'zap',
        isActive: true,
        createdAt: '',
        updatedAt: ''
      },
      {
        id: 4,
        serviceName: 'Legacy Modernization & Migration',
        description: 'Phased migration of legacy systems to modern, agile microservices with zero downtime or business disruption.',
        iconUrl: 'refresh',
        isActive: true,
        createdAt: '',
        updatedAt: ''
      }
    ]);
  }

  private setDefaultPortfolios(): void {
    this.portfolios.set([
      {
        id: 1,
        title: 'Apex Financial Core Modernization',
        description: 'Transformed core transaction ecosystem processing $400M+ in daily volume with 99.999% uptime.',
        thumbnailUrl: 'https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=800&auto=format&fit=crop&q=80',
        projectUrl: 'https://velvion.com',
        clientName: 'Apex Financial Services',
        category: 'FinTech Platform',
        technologies: 'Microservices, Real-Time Sync, Zero-Trust RBAC',
        completionDate: '2026-01-15',
        isActive: true,
        createdAt: '',
        updatedAt: ''
      },
      {
        id: 2,
        title: 'Vitalis Health Telemetry Hub',
        description: 'HIPAA-compliant healthcare portal coordinating 250,000+ active patients and medical staff across 40 hospitals.',
        thumbnailUrl: 'https://images.unsplash.com/photo-1576091160399-112ba8d25d1d?w=800&auto=format&fit=crop&q=80',
        projectUrl: 'https://velvion.com',
        clientName: 'Vitalis Health Network',
        category: 'HealthTech Platform',
        technologies: 'HIPAA Cloud, EHR Masters, Real-Time Telemetry',
        completionDate: '2026-02-20',
        isActive: true,
        createdAt: '',
        updatedAt: ''
      },
      {
        id: 3,
        title: 'OmniChain Global Supply Logistics',
        description: 'Supply chain command center orchestrating 120+ international distribution hubs with automated dispatch.',
        thumbnailUrl: 'https://images.unsplash.com/photo-1586528116311-ad8dd3c8310d?w=800&auto=format&fit=crop&q=80',
        projectUrl: 'https://velvion.com',
        clientName: 'OmniLogistics Corp',
        category: 'Supply Chain & IoT',
        technologies: 'IoT Telematics, Multi-Hub ERP, Dispatch Engine',
        completionDate: '2026-03-01',
        isActive: true,
        createdAt: '',
        updatedAt: ''
      }
    ]);
  }

  private setDefaultTestimonials(): void {
    this.testimonials.set([
      {
        id: 1,
        clientName: 'Dr. Sarah Jenkins',
        clientDesignation: 'Chief Technology Officer',
        companyName: 'Apex Financial Services',
        feedbackText: 'Velvion Technologies transformed our core architecture, reducing transaction latency by 65% while providing bulletproof security governance.',
        avatarUrl: '',
        rating: 5,
        isActive: true,
        createdAt: '',
        updatedAt: ''
      },
      {
        id: 2,
        clientName: 'Vikram Malhotra',
        clientDesignation: 'VP of Engineering',
        companyName: 'CloudScale Enterprises',
        feedbackText: 'The architectural precision and delivery discipline at Velvion are outstanding. Their team delivered our enterprise platform ahead of schedule.',
        avatarUrl: '',
        rating: 5,
        isActive: true,
        createdAt: '',
        updatedAt: ''
      },
      {
        id: 3,
        clientName: 'Elena Rostova',
        clientDesignation: 'Director of Digital Innovation',
        companyName: 'Nexus Global Logistics',
        feedbackText: 'Velvion brought unmatched domain expertise to our supply chain transformation. We have seen a 40% gain in operational efficiency.',
        avatarUrl: '',
        rating: 5,
        isActive: true,
        createdAt: '',
        updatedAt: ''
      }
    ]);
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update(v => !v);
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
  }

  selectServiceForInquiry(serviceName: string): void {
    this.inquiryForm.patchValue({
      subject: `Enterprise Consultation: ${serviceName}`
    });
    this.scrollToSection('contact');
    this.toastService.info(`Selected "${serviceName}". Complete your project details below.`, 'Service Selected');
  }

  scrollToSection(sectionId: string): void {
    this.closeMobileMenu();
    if (isPlatformBrowser(this.platformId) && typeof document !== 'undefined') {
      const element = document.getElementById(sectionId);
      if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    }
  }

  scrollToTop(): void {
    if (isPlatformBrowser(this.platformId) && typeof window !== 'undefined') {
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  onSubmitInquiry(): void {
    if (this.inquiryForm.invalid) {
      this.inquiryForm.markAllAsTouched();
      this.toastService.error('Please fill in your name, corporate email, and project requirements.', 'Form Incomplete');
      return;
    }

    this.inquirySubmitting.set(true);
    const val = this.inquiryForm.value;

    this.adminDataService.saveInquiry({
      fullName: val.fullName.trim(),
      email: val.email.trim(),
      phone: val.phone?.trim() || '',
      subject: val.subject,
      message: val.message.trim()
    }).subscribe({
      next: (res) => {
        this.inquirySubmitting.set(false);
        if (res.success) {
          this.toastService.success(
            'Thank you! Your inquiry has been received. Our team will contact you shortly.',
            'Inquiry Received'
          );
          this.inquiryForm.reset({
            subject: 'Enterprise Digital Transformation Consultation'
          });
        } else {
          this.toastService.error(res.message || 'Failed to submit inquiry.', 'Submission Error');
        }
      },
      error: () => {
        this.inquirySubmitting.set(false);
        this.toastService.error('Connection timeout to server. Please try again.', 'Network Error');
      }
    });
  }

  onSubmitNewsletter(): void {
    const email = this.newsletterEmail().trim();
    if (!email || !email.includes('@')) {
      this.toastService.error('Please provide a valid corporate email address.', 'Invalid Email');
      return;
    }

    this.newsletterSubmitting.set(true);
    setTimeout(() => {
      this.newsletterSubmitting.set(false);
      this.newsletterEmail.set('');
      this.toastService.success('Thank you for subscribing to Velvion Enterprise Insights!', 'Subscribed');
    }, 600);
  }

  openApplyModal(job: JobPostingDto): void {
    this.selectedJobForApply.set(job);
    this.applyForm.reset({
      jobPostingId: job.id,
      applicantName: '',
      email: '',
      phone: '',
      resumeUrl: '',
      coverLetter: ''
    });
    this.isApplyModalOpen.set(true);
  }

  closeApplyModal(): void {
    this.isApplyModalOpen.set(false);
    this.selectedJobForApply.set(null);
    this.applyForm.reset();
  }

  onSubmitJobApplication(): void {
    if (this.applyForm.invalid) {
      this.applyForm.markAllAsTouched();
      this.toastService.warning('Please provide your full name, email, and required details.', 'Validation Warning');
      return;
    }

    this.applySubmitting.set(true);
    const formVal = this.applyForm.value;
    const dto: SaveJobApplicationDto = {
      id: 0,
      jobPostingId: Number(formVal.jobPostingId),
      applicantName: formVal.applicantName.trim(),
      email: formVal.email.trim(),
      phone: formVal.phone?.trim() || null,
      resumeUrl: formVal.resumeUrl?.trim() || null,
      coverLetter: formVal.coverLetter?.trim() || null,
      status: 'Applied'
    };

    this.adminDataService.saveJobApplication(dto).subscribe({
      next: (res) => {
        this.applySubmitting.set(false);
        if (res.success) {
          this.toastService.success(
            `Thank you, ${dto.applicantName}! Your application has been submitted successfully. Our talent team will review it shortly.`,
            'Application Submitted!'
          );
          this.closeApplyModal();
        } else {
          this.toastService.error(res.message || 'Failed to submit application', 'Error');
        }
      },
      error: () => {
        this.applySubmitting.set(false);
        this.toastService.error('Error connecting to server. Please try again later.', 'Network Error');
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

