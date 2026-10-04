import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { ToastService, ConfirmService, IconComponent, ToggleComponent, PaginationComponent } from '../../../../shared';
import { FaqDto, SaveFaqDto } from '../../../../core/models/api.models';

@Component({
  selector: 'app-faqs-master',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IconComponent, ToggleComponent, PaginationComponent],
  templateUrl: './faqs-master.component.html',
  styleUrls: ['./faqs-master.component.css']
})
export class FaqsMasterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private adminDataService = inject(AdminDataService);
  private toastService = inject(ToastService);
  private confirmService = inject(ConfirmService);

  loading = signal<boolean>(false);
  faqs = signal<FaqDto[]>([]);
  searchQuery = signal<string>('');
  selectedCategory = signal<string>('All');

  // Sorting state
  sortColumn = signal<string>('displayOrder');
  sortDirection = signal<'asc' | 'desc'>('asc');

  // Pagination
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);

  categories = computed(() => {
    const list = this.faqs().map(f => f.category || 'General');
    return ['All', ...Array.from(new Set(list))];
  });

  filteredFaqs = computed(() => {
    let list = this.faqs();
    const cat = this.selectedCategory();
    if (cat !== 'All') {
      list = list.filter(f => (f.category || 'General').toLowerCase() === cat.toLowerCase());
    }
    const query = this.searchQuery().toLowerCase().trim();
    if (query) {
      list = list.filter(f =>
        f.question.toLowerCase().includes(query) ||
        f.answer.toLowerCase().includes(query) ||
        (f.category && f.category.toLowerCase().includes(query))
      );
    }
    return list;
  });

  sortedFaqs = computed(() => {
    let list = this.filteredFaqs();
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

  paginatedFaqs = computed(() => {
    const list = this.sortedFaqs();
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
  currentFaqId = signal<number>(0);
  modalSubmitting = signal<boolean>(false);
  faqForm!: FormGroup;

  // View Modal state
  isViewModalOpen = signal<boolean>(false);
  viewingFaq = signal<FaqDto | null>(null);

  openViewModal(f: FaqDto): void {
    this.viewingFaq.set(f);
    this.isViewModalOpen.set(true);
  }

  closeViewModal(): void {
    this.isViewModalOpen.set(false);
    this.viewingFaq.set(null);
  }

  ngOnInit(): void {
    this.initForm();
    this.loadFaqs();
  }

  private initForm(): void {
    this.faqForm = this.fb.group({
      question: ['', [Validators.required, Validators.maxLength(300)]],
      answer: ['', [Validators.required]],
      category: ['General'],
      displayOrder: [0, [Validators.min(0)]],
      isActive: [true]
    });
  }

  loadFaqs(): void {
    this.loading.set(true);
    this.adminDataService.getFaqs().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.faqs.set(res.data);
        } else {
          this.toastService.error(res.message || 'Failed to load FAQs', 'Database Error');
        }
      },
      error: () => {
        this.loading.set(false);
        this.toastService.error('Error connecting to FAQs API', 'Network Error');
      }
    });
  }

  openAddModal(): void {
    this.isEditMode.set(false);
    this.currentFaqId.set(0);
    this.faqForm.reset({
      question: '',
      answer: '',
      category: 'General',
      displayOrder: this.faqs().length + 1,
      isActive: true
    });
    this.isModalOpen.set(true);
  }

  openEditModal(faq: FaqDto): void {
    this.isEditMode.set(true);
    this.currentFaqId.set(faq.id);
    this.faqForm.reset({
      question: faq.question,
      answer: faq.answer,
      category: faq.category || 'General',
      displayOrder: faq.displayOrder,
      isActive: faq.isActive
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  saveFaq(): void {
    if (this.faqForm.invalid) {
      this.faqForm.markAllAsTouched();
      this.toastService.error('Question and Answer are required.', 'Form Incomplete');
      return;
    }

    this.modalSubmitting.set(true);
    const val = this.faqForm.value;

    const dto: SaveFaqDto = {
      id: this.currentFaqId(),
      question: val.question.trim(),
      answer: val.answer.trim(),
      category: val.category?.trim() || 'General',
      displayOrder: Number(val.displayOrder) || 0,
      isActive: val.isActive
    };

    this.adminDataService.saveFaq(dto).subscribe({
      next: (res) => {
        this.modalSubmitting.set(false);
        if (res.success) {
          this.toastService.success(
            this.isEditMode() ? 'FAQ entry updated.' : 'FAQ entry created.',
            'Saved to Database'
          );
          this.closeModal();
          this.loadFaqs();
        } else {
          this.toastService.error(res.message || 'Failed to save FAQ.', 'Error');
        }
      },
      error: () => {
        this.modalSubmitting.set(false);
        this.toastService.error('Error saving FAQ to database.', 'Error');
      }
    });
  }

  async deleteFaq(faq: FaqDto): Promise<void> {
    const confirmed = await this.confirmService.danger(
      'Delete FAQ Entry',
      'Are you sure you want to delete this FAQ entry from the knowledge base?'
    );

    if (confirmed) {
      this.adminDataService.deleteFaq(faq.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.success('FAQ entry deleted.', 'Deleted');
            this.loadFaqs();
          } else {
            this.toastService.error(res.message || 'Failed to delete FAQ', 'Error');
          }
        },
        error: () => {
          this.toastService.error('Network error deleting FAQ', 'Error');
        }
      });
    }
  }
}
