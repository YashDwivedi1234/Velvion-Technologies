import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { ToastService, ConfirmService, IconComponent, PaginationComponent } from '../../../../shared';
import { JobApplicationDto, JobPostingDto } from '../../../../core/models/api.models';

@Component({
  selector: 'app-job-applications',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IconComponent, PaginationComponent],
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
  sortColumn = signal<string>('createdAt');
  sortDirection = signal<'asc' | 'desc'>('desc');
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);

  countApplied = computed(() => this.applications().filter(a => a.status === 'Applied').length);
  countShortlisted = computed(() => this.applications().filter(a => a.status === 'Shortlisted').length);
  countHired = computed(() => this.applications().filter(a => a.status === 'Hired').length);

  isStatusModalOpen = signal<boolean>(false);
  isViewModalOpen = signal<boolean>(false);
  currentApplication = signal<JobApplicationDto | null>(null);
  statusSubmitting = signal<boolean>(false);
  statusForm!: FormGroup;

  readonly statusOptions = ['Applied', 'Shortlisted', 'Interview Scheduled', 'Rejected', 'Hired'];

  readonly statusColors: Record<string, string> = {
    'Applied': 'status-applied',
    'Shortlisted': 'status-shortlisted',
    'Interview Scheduled': 'status-interview',
    'Rejected': 'status-rejected',
    'Hired': 'status-hired'
  };

  sortedApplications = computed(() => {
    let list = this.applications();
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
    this.initStatusForm();
    this.loadJobPostings();
    this.loadApplications();
  }

  private initStatusForm(): void {
    this.statusForm = this.fb.group({
      status: ['', Validators.required],
      notes: ['']
    });
  }

  loadJobPostings(): void {
    this.adminDataService.getJobPostings().subscribe({
      next: (res) => {
        if (res.success && res.data) this.jobPostings.set(res.data);
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
