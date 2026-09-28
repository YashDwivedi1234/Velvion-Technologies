import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminDataService } from '../../../core/services/admin-data.service';
import { ToastService, ConfirmService, IconComponent, ToggleComponent, PaginationComponent } from '../../../shared';
import { BlogDto, SaveBlogDto } from '../../../core/models/api.models';

@Component({
  selector: 'app-blogs',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IconComponent, ToggleComponent, PaginationComponent],
  templateUrl: './blogs.component.html',
  styleUrls: ['./blogs.component.css']
})
export class BlogsComponent implements OnInit {
  private fb = inject(FormBuilder);
  private adminDataService = inject(AdminDataService);
  private toastService = inject(ToastService);
  private confirmService = inject(ConfirmService);

  loading = signal<boolean>(false);
  blogs = signal<BlogDto[]>([]);

  // Sorting state
  sortColumn = signal<string>('title');
  sortDirection = signal<'asc' | 'desc'>('asc');

  // Pagination
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);

  sortedBlogs = computed(() => {
    let list = this.blogs();
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

  paginatedBlogs = computed(() => {
    const list = this.sortedBlogs();
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
  currentBlogId = signal<number>(0);
  modalSubmitting = signal<boolean>(false);
  blogForm!: FormGroup;

  // View Modal state
  isViewModalOpen = signal<boolean>(false);
  viewingBlog = signal<BlogDto | null>(null);

  openViewModal(b: BlogDto): void {
    this.viewingBlog.set(b);
    this.isViewModalOpen.set(true);
  }

  closeViewModal(): void {
    this.isViewModalOpen.set(false);
    this.viewingBlog.set(null);
  }

  ngOnInit(): void {
    this.initForm();
    this.loadBlogs();
  }

  private initForm(): void {
    this.blogForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(4)]],
      slug: [''],
      category: ['Engineering & AI'],
      summary: [''],
      content: ['', [Validators.required]],
      tags: ['Angular, .NET 10, MySQL'],
      isPublished: [true]
    });
  }

  loadBlogs(): void {
    this.loading.set(true);
    this.adminDataService.getBlogs().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.blogs.set(res.data);
        } else {
          this.toastService.error(res.message || 'Failed to load blogs', 'Error');
        }
      },
      error: () => {
        this.loading.set(false);
        this.toastService.error('Error fetching blogs from MySQL', 'Error');
      }
    });
  }

  openAddModal(): void {
    this.isEditMode.set(false);
    this.currentBlogId.set(0);
    this.blogForm.reset({
      title: '',
      slug: '',
      category: 'Engineering & AI',
      summary: '',
      content: '',
      tags: 'Architecture, Cloud',
      isPublished: true
    });
    this.isModalOpen.set(true);
  }

  openEditModal(b: BlogDto): void {
    this.isEditMode.set(true);
    this.currentBlogId.set(b.id);
    this.blogForm.reset({
      title: b.title,
      slug: b.slug || '',
      category: b.category || 'General',
      summary: b.summary || '',
      content: b.content,
      tags: b.tags || '',
      isPublished: b.isPublished
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  saveBlog(): void {
    if (this.blogForm.invalid) {
      this.blogForm.markAllAsTouched();
      this.toastService.error('Please fill in required fields (Title, Content).', 'Validation Error');
      return;
    }

    this.modalSubmitting.set(true);
    const val = this.blogForm.value;

    const dto: SaveBlogDto = {
      id: this.currentBlogId(),
      title: val.title.trim(),
      slug: val.slug?.trim() || val.title.toLowerCase().replace(/[^a-z0-9]+/g, '-'),
      category: val.category?.trim(),
      summary: val.summary?.trim(),
      content: val.content,
      tags: val.tags?.trim(),
      isPublished: val.isPublished
    };

    this.adminDataService.saveBlog(dto).subscribe({
      next: (res) => {
        this.modalSubmitting.set(false);
        if (res.success) {
          this.toastService.success(
            this.isEditMode() ? `Blog "${val.title}" updated in database.` : `Blog "${val.title}" published to database.`,
            'Saved'
          );
          this.closeModal();
          this.loadBlogs();
        } else {
          this.toastService.error(res.message || 'Failed to save blog.', 'Error');
        }
      },
      error: () => {
        this.modalSubmitting.set(false);
        this.toastService.error('Connection error saving blog.', 'Error');
      }
    });
  }

  async deleteBlog(b: BlogDto): Promise<void> {
    const confirmed = await this.confirmService.danger(
      'Delete Blog Post',
      `Are you sure you want to delete "${b.title}"?`
    );

    if (confirmed) {
      this.adminDataService.deleteBlog(b.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.success(`Blog post deleted from database.`, 'Deleted');
            this.loadBlogs();
          } else {
            this.toastService.error(res.message || 'Failed to delete blog', 'Error');
          }
        },
        error: () => {
          this.toastService.error('Network error deleting blog', 'Error');
        }
      });
    }
  }
}
