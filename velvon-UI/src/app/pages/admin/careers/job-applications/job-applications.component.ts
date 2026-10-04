import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, FormsModule, Validators } from '@angular/forms';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { ToastService, ConfirmService, IconComponent, PaginationComponent } from '../../../../shared';
import { JobApplicationDto, JobPostingDto, SaveJobApplicationDto } from '../../../../core/models/api.models';

@Component({
  selector: 'app-job-applications',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, IconComponent, PaginationComponent],
  templateUrl: './job-applications.component.html',
  styleUrls: ['./job-applications.component.css']
})
export class JobApplicationsComponent implements OnInit {
  private adminDataService = inject(AdminDataService);
  private toastService = inject(ToastService);
  private confirmService = inject(ConfirmService);
  private fb = inject(FormBuilder);

  loading = signal<boolean>(false);
  applications = signal<JobApplicationDto[]>([]);
  jobPostings = signal<JobPostingDto[]>([]);

  selectedJobId = signal<number | null>(null);
  searchQuery = signal<string>('');
  sortColumn = signal<string>('createdAt');
  sortDirection = signal<'asc' | 'desc'>('desc');
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);

  // Computed counters based on loaded applications
  countApplied = computed(() => this.applications().filter(a => a.status === 'Applied').length);
  countShortlisted = computed(() => this.applications().filter(a => a.status === 'Shortlisted').length);
  countInterview = computed(() => this.applications().filter(a => a.status === 'Interview Scheduled').length);
  countHired = computed(() => this.applications().filter(a => a.status === 'Hired').length);

  isStatusModalOpen = signal<boolean>(false);
  isViewModalOpen = signal<boolean>(false);
  isAddModalOpen = signal<boolean>(false);
  currentApplication = signal<JobApplicationDto | null>(null);
  
  statusSubmitting = signal<boolean>(false);
  addSubmitting = signal<boolean>(false);
  
  statusForm!: FormGroup;
  addForm!: FormGroup;

  readonly statusOptions = ['Applied', 'Shortlisted', 'Interview Scheduled', 'Rejected', 'Hired'];

  readonly statusColors: Record<string, string> = {
    'Applied': 'status-applied',
    'Shortlisted': 'status-shortlisted',
    'Interview Scheduled': 'status-interview',
    'Rejected': 'status-rejected',
    'Hired': 'status-hired'
  };

  filteredApplications = computed(() => {
    let list = this.applications();
    const query = this.searchQuery().trim().toLowerCase();

    if (query) {
      list = list.filter(a =>
        a.applicantName.toLowerCase().includes(query) ||
        a.email.toLowerCase().includes(query) ||
        (a.phone && a.phone.toLowerCase().includes(query)) ||
        (a.jobTitle && a.jobTitle.toLowerCase().includes(query)) ||
        (a.status && a.status.toLowerCase().includes(query))
      );
    }
    return list;
  });

  sortedApplications = computed(() => {
    let list = this.filteredApplications();
    const col = this.sortColumn();
    const dir = this.sortDirection() === 'asc' ? 1 : -1;
    if (col) {
      list = [...list].sort((a: any, b: any) => {
        const valA = a[col] ?? '';
        const valB = b[col] ?? '';
        if (typeof valA === 'boolean' && typeof valB === 'boolean') return (valA === valB ? 0 : valA ? 1 : -1) * dir;
        return valA.toString().localeCompare(valB.toString(), undefined, { numeric: true, sensitivity: 'base' }) * dir;
      });
    }
    return list;
  });

  paginatedApplications = computed(() => {
    const list = this.sortedApplications();
    const start = (this.currentPage() - 1) * this.pageSize();
    return list.slice(start, start + this.pageSize());
  });

  toggleSort(column: string): void {
    if (this.sortColumn() === column) {
      this.sortDirection.update(dir => dir === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortColumn.set(column);
      this.sortDirection.set('asc');
    }
  }

  ngOnInit(): void {
    this.initForms();
    this.loadJobPostings();
    this.loadApplications();
  }

  private initForms(): void {
    this.statusForm = this.fb.group({
      status: ['', Validators.required],
      notes: ['']
    });

    this.addForm = this.fb.group({
      jobPostingId: ['', Validators.required],
      applicantName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
      phone: ['', [Validators.maxLength(20)]],
      resumeUrl: ['', [Validators.maxLength(255)]],
      coverLetter: [''],
      status: ['Applied', Validators.required],
      notes: ['']
    });
  }

  loadJobPostings(): void {
    this.adminDataService.getJobPostings().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.jobPostings.set(res.data);
          // Auto select first job posting in add form if available
          if (res.data.length > 0 && !this.addForm.get('jobPostingId')?.value) {
            this.addForm.patchValue({ jobPostingId: res.data[0].id });
          }
        }
      }
    });
  }

  loadApplications(): void {
    this.loading.set(true);
    const jobId = this.selectedJobId();
    this.adminDataService.getJobApplications(jobId ?? undefined).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.applications.set(res.data);
          this.currentPage.set(1);
        } else {
          this.toastService.error(res.message || 'Failed to load applications', 'Error');
        }
      },
      error: () => {
        this.loading.set(false);
        this.toastService.error('Error connecting to API', 'Network Error');
      }
    });
  }

  filterByJob(jobId: string): void {
    this.selectedJobId.set(jobId ? Number(jobId) : null);
    this.loadApplications();
  }

  onSearchChange(text: string): void {
    this.searchQuery.set(text);
    this.currentPage.set(1);
  }

  openAddModal(): void {
    const defaultJobId = this.selectedJobId() || (this.jobPostings().length > 0 ? this.jobPostings()[0].id : '');
    this.addForm.reset({
      jobPostingId: defaultJobId,
      applicantName: '',
      email: '',
      phone: '',
      resumeUrl: '',
      coverLetter: '',
      status: 'Applied',
      notes: ''
    });
    this.isAddModalOpen.set(true);
  }

  closeAddModal(): void {
    this.isAddModalOpen.set(false);
    this.addForm.reset();
  }

  saveApplication(): void {
    if (this.addForm.invalid) {
      this.addForm.markAllAsTouched();
      this.toastService.warning('Please fill in all required fields accurately.', 'Validation Warning');
      return;
    }

    this.addSubmitting.set(true);
    const formVal = this.addForm.value;
    const dto: SaveJobApplicationDto = {
      id: 0,
      jobPostingId: Number(formVal.jobPostingId),
      applicantName: formVal.applicantName.trim(),
      email: formVal.email.trim(),
      phone: formVal.phone?.trim() || null,
      resumeUrl: formVal.resumeUrl?.trim() || null,
      coverLetter: formVal.coverLetter?.trim() || null,
      status: formVal.status,
      notes: formVal.notes?.trim() || null
    };

    this.adminDataService.saveJobApplication(dto).subscribe({
      next: (res) => {
        this.addSubmitting.set(false);
        if (res.success) {
          this.toastService.success(`Application for "${dto.applicantName}" added successfully!`, 'Success');
          this.closeAddModal();
          this.loadApplications();
          this.loadJobPostings();
        } else {
          this.toastService.error(res.message || 'Failed to save application', 'Error');
        }
      },
      error: () => {
        this.addSubmitting.set(false);
        this.toastService.error('Error connecting to server while saving application', 'Network Error');
      }
    });
  }

  openViewModal(app: JobApplicationDto): void {
    this.currentApplication.set(app);
    this.isViewModalOpen.set(true);
  }

  closeViewModal(): void {
    this.isViewModalOpen.set(false);
    this.currentApplication.set(null);
  }

  openStatusModal(app: JobApplicationDto): void {
    this.currentApplication.set(app);
    this.statusForm.reset({ status: app.status, notes: app.notes || '' });
    this.isStatusModalOpen.set(true);
  }

  closeStatusModal(): void {
    this.isStatusModalOpen.set(false);
    this.currentApplication.set(null);
  }

  updateStatus(): void {
    if (this.statusForm.invalid) return;
    const app = this.currentApplication();
    if (!app) return;

    this.statusSubmitting.set(true);
    const val = this.statusForm.value;

    this.adminDataService.updateApplicationStatus(app.id, val.status, val.notes).subscribe({
      next: (res) => {
        this.statusSubmitting.set(false);
        if (res.success) {
          this.toastService.success(`Status updated to "${val.status}"`, 'Updated');
          this.closeStatusModal();
          this.loadApplications();
          this.loadJobPostings();
        } else {
          this.toastService.error(res.message || 'Failed to update status', 'Error');
        }
      },
      error: () => {
        this.statusSubmitting.set(false);
        this.toastService.error('Connection error updating status', 'Error');
      }
    });
  }

  async deleteApplication(app: JobApplicationDto): Promise<void> {
    const confirmed = await this.confirmService.danger(
      'Delete Application',
      `Are you sure you want to delete the application from "${app.applicantName}"?`
    );

    if (confirmed) {
      this.adminDataService.deleteJobApplication(app.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.success('Application deleted successfully.', 'Deleted');
            this.loadApplications();
            this.loadJobPostings();
          } else {
            this.toastService.error(res.message || 'Failed to delete application', 'Error');
          }
        },
        error: () => {
          this.toastService.error('Network error deleting application', 'Error');
        }
      });
    }
  }

  getStatusClass(status: string): string {
    return this.statusColors[status] || 'status-applied';
  }
}
