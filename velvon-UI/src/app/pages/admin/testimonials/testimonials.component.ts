import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminDataService } from '../../../core/services/admin-data.service';
import { ToastService, ConfirmService, IconComponent, ToggleComponent, PaginationComponent } from '../../../shared';
import { TestimonialDto, SaveTestimonialDto } from '../../../core/models/api.models';

@Component({
  selector: 'app-testimonials',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IconComponent, ToggleComponent, PaginationComponent],
  templateUrl: './testimonials.component.html',
  styleUrls: ['./testimonials.component.css']
})
export class TestimonialsComponent implements OnInit {
  private fb = inject(FormBuilder);
  private adminDataService = inject(AdminDataService);
  private toastService = inject(ToastService);
  private confirmService = inject(ConfirmService);

  loading = signal<boolean>(false);
  testimonials = signal<TestimonialDto[]>([]);

  // Sorting state
  sortColumn = signal<string>('clientName');
  sortDirection = signal<'asc' | 'desc'>('asc');

  // Pagination
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);

  sortedTestimonials = computed(() => {
    let list = this.testimonials();
    if (this.sortColumn()) {
      const col = this.sortColumn();
      const dir = this.sortDirection() === 'asc' ? 1 : -1;
      list = [...list].sort((a: any, b: any) => {
        const valA = a[col] ?? '';
        const valB = b[col] ?? '';
        if (typeof valA === 'number' && typeof valB === 'number') {
          return (valA - valB) * dir;
        }
        if (typeof valA === 'boolean' && typeof valB === 'boolean') {
          return (valA === valB ? 0 : valA ? 1 : -1) * dir;
        }
        return valA.toString().localeCompare(valB.toString(), undefined, { numeric: true, sensitivity: 'base' }) * dir;
      });
    }
    return list;
  });

  paginatedTestimonials = computed(() => {
    const list = this.sortedTestimonials();
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
  isModalOpen = signal<boolean>(false);
  isEditMode = signal<boolean>(false);
  currentTestimonialId = signal<number>(0);
  modalSubmitting = signal<boolean>(false);
  testimonialForm!: FormGroup;

  // View Modal state
  isViewModalOpen = signal<boolean>(false);
  viewingTestimonial = signal<TestimonialDto | null>(null);

  openViewModal(t: TestimonialDto): void {
    this.viewingTestimonial.set(t);
    this.isViewModalOpen.set(true);
  }

  closeViewModal(): void {
    this.isViewModalOpen.set(false);
    this.viewingTestimonial.set(null);
  }

  ngOnInit(): void {
    this.initForm();
    this.loadTestimonials();
  }

  private initForm(): void {
    this.testimonialForm = this.fb.group({
      clientName: ['', [Validators.required]],
      clientDesignation: ['CTO / VP of Engineering'],
      companyName: [''],
      rating: [5, [Validators.required, Validators.min(1), Validators.max(5)]],
      feedbackText: ['', [Validators.required, Validators.minLength(10)]],
      isActive: [true]
    });
  }

  loadTestimonials(): void {
    this.loading.set(true);
    this.adminDataService.getTestimonials().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.testimonials.set(res.data);
        }
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  openAddModal(): void {
    this.isEditMode.set(false);
    this.currentTestimonialId.set(0);
    this.testimonialForm.reset({
      clientName: '',
      clientDesignation: 'CTO / VP of Engineering',
      companyName: '',
      rating: 5,
      feedbackText: '',
      isActive: true
    });
    this.isModalOpen.set(true);
  }

  openEditModal(t: TestimonialDto): void {
    this.isEditMode.set(true);
    this.currentTestimonialId.set(t.id);
    this.testimonialForm.reset({
      clientName: t.clientName,
      clientDesignation: t.clientDesignation || '',
      companyName: t.companyName || '',
      rating: t.rating || 5,
      feedbackText: t.feedbackText,
      isActive: t.isActive
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  saveTestimonial(): void {
    if (this.testimonialForm.invalid) {
      this.testimonialForm.markAllAsTouched();
      this.toastService.error('Client Name and Feedback text are required.', 'Form Incomplete');
      return;
    }

    this.modalSubmitting.set(true);
    const val = this.testimonialForm.value;

    const dto: SaveTestimonialDto = {
      id: this.currentTestimonialId(),
      clientName: val.clientName.trim(),
      clientDesignation: val.clientDesignation?.trim(),
      companyName: val.companyName?.trim(),
      rating: Number(val.rating) || 5,
      feedbackText: val.feedbackText.trim(),
      isActive: val.isActive
    };

    this.adminDataService.saveTestimonial(dto).subscribe({
      next: (res) => {
        this.modalSubmitting.set(false);
        if (res.success) {
          this.toastService.success(
            this.isEditMode() ? `Testimonial from ${val.clientName} updated.` : `New testimonial recorded in database.`,
            'Saved to Database'
          );
          this.closeModal();
          this.loadTestimonials();
        } else {
          this.toastService.error(res.message || 'Failed to save testimonial.', 'Error');
        }
      },
      error: () => {
        this.modalSubmitting.set(false);
        this.toastService.error('Error saving testimonial.', 'Error');
      }
    });
  }

  async deleteTestimonial(t: TestimonialDto): Promise<void> {
    const confirmed = await this.confirmService.danger(
      'Delete Testimonial',
      `Are you sure you want to delete feedback from "${t.clientName}"?`
    );

    if (confirmed) {
      this.adminDataService.deleteTestimonial(t.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.success('Testimonial removed from database.', 'Deleted');
            this.loadTestimonials();
          } else {
            this.toastService.error(res.message || 'Failed to delete testimonial', 'Error');
          }
        },
        error: () => {
          this.toastService.error('Error deleting testimonial', 'Error');
        }
      });
    }
  }
}
