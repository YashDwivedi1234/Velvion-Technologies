import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { ToastService, ConfirmService, IconComponent, ToggleComponent, PaginationComponent } from '../../../../shared';
import { JobPostingDto, SaveJobPostingDto } from '../../../../core/models/api.models';

@Component({
  selector: 'app-job-postings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IconComponent, ToggleComponent, PaginationComponent],
  templateUrl: './job-postings.component.html',
  styleUrls: ['./job-postings.component.css']
})
export class JobPostingsComponent implements OnInit {
  private fb = inject(FormBuilder);
  private adminDataService = inject(AdminDataService);
  private toastService = inject(ToastService);
  private confirmService = inject(ConfirmService);

  loading = signal<boolean>(false);
  jobs = signal<JobPostingDto[]>([]);

  sortColumn = signal<string>('jobTitle');
  sortDirection = signal<'asc' | 'desc'>('asc');
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);

  isModalOpen = signal<boolean>(false);
  isEditMode = signal<boolean>(false);
  currentJobId = signal<number>(0);
  modalSubmitting = signal<boolean>(false);
  isViewModalOpen = signal<boolean>(false);
  viewingJob = signal<JobPostingDto | null>(null);

  jobForm!: FormGroup;

  readonly jobTypes = ['Full-time', 'Part-time', 'Remote', 'Contract', 'Internship'];
  readonly experienceLevels = ['Fresher', '1-2 years', '2-4 years', '4-6 years', '6+ years'];
  readonly departments = ['Engineering', 'Design', 'Product', 'Sales & Marketing', 'DevOps & Cloud', 'QA', 'HR & Operations', 'Finance'];

  sortedJobs = computed(() => {
    let list = this.jobs();
    const col = this.sortColumn();
    const dir = this.sortDirection() === 'asc' ? 1 : -1;
    if (col) {
      list = [...list].sort((a: any, b: any) => {
        const valA = a[col] ?? '';
        const valB = b[col] ?? '';
        if (typeof valA === 'number' && typeof valB === 'number') return (valA - valB) * dir;
        if (typeof valA === 'boolean' && typeof valB === 'boolean') return (valA === valB ? 0 : valA ? 1 : -1) * dir;
        return valA.toString().localeCompare(valB.toString(), undefined, { numeric: true, sensitivity: 'base' }) * dir;
      });
    }
    return list;
  });

  paginatedJobs = computed(() => {
    const list = this.sortedJobs();
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
    this.initForm();
    this.loadJobs();
  }

  private initForm(): void {
    this.jobForm = this.fb.group({
      jobTitle: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(150)]],
      department: ['Engineering'],
      location: ['Ahmedabad, Gujarat'],
      jobType: ['Full-time'],
      experienceLevel: ['1-2 years'],
      description: ['', Validators.required],
      requiredSkills: [''],
      salaryRange: [''],
      lastDateToApply: [''],
      isActive: [true]
    });
  }

  loadJobs(): void {
    this.loading.set(true);
    this.adminDataService.getJobPostings().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.jobs.set(res.data);
        } else {
          this.toastService.error(res.message || 'Failed to load job postings', 'Error');
        }
      },
      error: () => {
        this.loading.set(false);
        this.toastService.error('Error connecting to API', 'Network Error');
      }
    });
  }

  openViewModal(job: JobPostingDto): void {
    this.viewingJob.set(job);
    this.isViewModalOpen.set(true);
  }

  closeViewModal(): void {
    this.isViewModalOpen.set(false);
    this.viewingJob.set(null);
  }

  openAddModal(): void {
    this.isEditMode.set(false);
    this.currentJobId.set(0);
    this.jobForm.reset({
      jobTitle: '', department: 'Engineering', location: 'Ahmedabad, Gujarat',
      jobType: 'Full-time', experienceLevel: '1-2 years', description: '',
      requiredSkills: '', salaryRange: '', lastDateToApply: '', isActive: true
    });
    this.isModalOpen.set(true);
  }

  openEditModal(job: JobPostingDto): void {
    this.isEditMode.set(true);
    this.currentJobId.set(job.id);
    this.jobForm.reset({
      jobTitle: job.jobTitle, department: job.department || 'Engineering',
      location: job.location || '', jobType: job.jobType || 'Full-time',
      experienceLevel: job.experienceLevel || '1-2 years',
      description: job.description || '', requiredSkills: job.requiredSkills || '',
      salaryRange: job.salaryRange || '',
      lastDateToApply: job.lastDateToApply ? job.lastDateToApply.substring(0, 10) : '',
      isActive: job.isActive
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  saveJob(): void {
    if (this.jobForm.invalid) {
      this.jobForm.markAllAsTouched();
      this.toastService.error('Please fill in required fields.', 'Validation Error');
      return;
    }

    this.modalSubmitting.set(true);
    const val = this.jobForm.value;

    const dto: SaveJobPostingDto = {
      id: this.currentJobId(),
      jobTitle: val.jobTitle.trim(),
      department: val.department?.trim(),
      location: val.location?.trim(),
      jobType: val.jobType,
      experienceLevel: val.experienceLevel,
      description: val.description,
      requiredSkills: val.requiredSkills?.trim(),
      salaryRange: val.salaryRange?.trim(),
      lastDateToApply: val.lastDateToApply || undefined,
      isActive: val.isActive
    };

    this.adminDataService.saveJobPosting(dto).subscribe({
      next: (res) => {
        this.modalSubmitting.set(false);
        if (res.success) {
          this.toastService.success(
            this.isEditMode() ? `Job "${val.jobTitle}" updated.` : `Job "${val.jobTitle}" posted successfully.`,
            'Saved to Database'
          );
          this.closeModal();
          this.loadJobs();
        } else {
          this.toastService.error(res.message || 'Failed to save job posting', 'Error');
        }
      },
      error: () => {
        this.modalSubmitting.set(false);
        this.toastService.error('Connection error saving job posting', 'Error');
      }
    });
  }

  async deleteJob(job: JobPostingDto): Promise<void> {
    const confirmed = await this.confirmService.danger(
      'Delete Job Posting',
      `Are you sure you want to delete "${job.jobTitle}"? All ${job.applicationCount} applications for this job will also be removed.`
    );

    if (confirmed) {
      this.adminDataService.deleteJobPosting(job.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.success(`Job posting deleted successfully.`, 'Deleted');
            this.loadJobs();
          } else {
            this.toastService.error(res.message || 'Failed to delete job', 'Error');
          }
        },
        error: () => {
          this.toastService.error('Network error deleting job posting', 'Error');
        }
      });
    }
  }

  getJobTypeClass(jobType?: string): string {
    if (!jobType) return 'type-fulltime';
    return 'type-' + jobType.toLowerCase().replace(/[^a-z0-9]/g, '');
  }
}

