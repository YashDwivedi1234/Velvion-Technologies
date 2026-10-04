import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { ToastService, ConfirmService, IconComponent, ToggleComponent, PaginationComponent } from '../../../../shared';
import { CategoryDto, SaveCategoryDto } from '../../../../core/models/api.models';

@Component({
  selector: 'app-categories-master',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IconComponent, ToggleComponent, PaginationComponent],
  templateUrl: './categories-master.component.html',
  styleUrls: ['./categories-master.component.css']
})
export class CategoriesMasterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private adminDataService = inject(AdminDataService);
  private toastService = inject(ToastService);
  private confirmService = inject(ConfirmService);

  loading = signal<boolean>(false);
  categories = signal<CategoryDto[]>([]);
  searchQuery = signal<string>('');

  // Sorting state
  sortColumn = signal<string>('displayOrder');
  sortDirection = signal<'asc' | 'desc'>('asc');

  // Pagination
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);

  filteredCategories = computed(() => {
    let list = this.categories();
    const query = this.searchQuery().toLowerCase().trim();
    if (query) {
      list = list.filter(c =>
        c.categoryName.toLowerCase().includes(query) ||
        (c.slug && c.slug.toLowerCase().includes(query)) ||
        (c.description && c.description.toLowerCase().includes(query))
      );
    }
    return list;
  });

  sortedCategories = computed(() => {
    let list = this.filteredCategories();
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

  paginatedCategories = computed(() => {
    const list = this.sortedCategories();
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
  currentCategoryId = signal<number>(0);
  modalSubmitting = signal<boolean>(false);
  categoryForm!: FormGroup;

  // View Modal state
  isViewModalOpen = signal<boolean>(false);
  viewingCategory = signal<CategoryDto | null>(null);

  openViewModal(c: CategoryDto): void {
    this.viewingCategory.set(c);
    this.isViewModalOpen.set(true);
  }

  closeViewModal(): void {
    this.isViewModalOpen.set(false);
    this.viewingCategory.set(null);
  }

  ngOnInit(): void {
    this.initForm();
    this.loadCategories();
  }

  private initForm(): void {
    this.categoryForm = this.fb.group({
      categoryName: ['', [Validators.required, Validators.maxLength(100)]],
      slug: [''],
      description: [''],
      icon: ['folder'],
      displayOrder: [0, [Validators.min(0)]],
      isActive: [true]
    });
  }

  loadCategories(): void {
    this.loading.set(true);
    this.adminDataService.getCategories().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.categories.set(res.data);
        } else {
          this.toastService.error(res.message || 'Failed to load categories', 'Database Error');
        }
      },
      error: () => {
        this.loading.set(false);
        this.toastService.error('Error connecting to Categories API', 'Network Error');
      }
    });
  }

  openAddModal(): void {
    this.isEditMode.set(false);
    this.currentCategoryId.set(0);
    this.categoryForm.reset({
      categoryName: '',
      slug: '',
      description: '',
      icon: 'folder',
      displayOrder: this.categories().length + 1,
      isActive: true
    });
    this.isModalOpen.set(true);
  }

  openEditModal(category: CategoryDto): void {
    this.isEditMode.set(true);
    this.currentCategoryId.set(category.id);
    this.categoryForm.reset({
      categoryName: category.categoryName,
      slug: category.slug || '',
      description: category.description || '',
      icon: category.icon || 'folder',
      displayOrder: category.displayOrder,
      isActive: category.isActive
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  saveCategory(): void {
    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      this.toastService.error('Category Name is required.', 'Form Incomplete');
      return;
    }

    this.modalSubmitting.set(true);
    const val = this.categoryForm.value;

    const dto: SaveCategoryDto = {
      id: this.currentCategoryId(),
      categoryName: val.categoryName.trim(),
      slug: val.slug?.trim() || val.categoryName.trim().toLowerCase().replace(/\s+/g, '-'),
      description: val.description?.trim(),
      icon: val.icon?.trim() || 'folder',
      displayOrder: Number(val.displayOrder) || 0,
      isActive: val.isActive
    };

    this.adminDataService.saveCategory(dto).subscribe({
      next: (res) => {
        this.modalSubmitting.set(false);
        if (res.success) {
          this.toastService.success(
            this.isEditMode() ? `Category "${val.categoryName}" updated.` : `Category "${val.categoryName}" created.`,
            'Saved to Database'
          );
          this.closeModal();
          this.loadCategories();
        } else {
          this.toastService.error(res.message || 'Failed to save category.', 'Error');
        }
      },
      error: () => {
        this.modalSubmitting.set(false);
        this.toastService.error('Error saving category to database.', 'Error');
      }
    });
  }

  async deleteCategory(category: CategoryDto): Promise<void> {
    const confirmed = await this.confirmService.danger(
      'Delete Project Category',
      `Are you sure you want to delete category "${category.categoryName}"?`
    );

    if (confirmed) {
      this.adminDataService.deleteCategory(category.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.success(`Category "${category.categoryName}" deleted.`, 'Deleted');
            this.loadCategories();
          } else {
            this.toastService.error(res.message || 'Failed to delete category', 'Error');
          }
        },
        error: () => {
          this.toastService.error('Network error deleting category', 'Error');
        }
      });
    }
  }
}
