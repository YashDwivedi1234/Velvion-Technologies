import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { ToastService, ConfirmService, IconComponent, ToggleComponent, PaginationComponent } from '../../../../shared';
import { DepartmentDto, SaveDepartmentDto } from '../../../../core/models/api.models';

@Component({
  selector: 'app-departments-master',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IconComponent, ToggleComponent, PaginationComponent],
  templateUrl: './departments-master.component.html',
  styleUrls: ['./departments-master.component.css']
})
export class DepartmentsMasterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private adminDataService = inject(AdminDataService);
  private toastService = inject(ToastService);
  private confirmService = inject(ConfirmService);

  loading = signal<boolean>(false);
  departments = signal<DepartmentDto[]>([]);
  searchQuery = signal<string>('');

  // Sorting state
  sortColumn = signal<string>('displayOrder');
  sortDirection = signal<'asc' | 'desc'>('asc');

  // Pagination
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);

  filteredDepartments = computed(() => {
    let list = this.departments();
    const query = this.searchQuery().toLowerCase().trim();
    if (query) {
      list = list.filter(d =>
        d.departmentName.toLowerCase().includes(query) ||
        (d.designations && d.designations.toLowerCase().includes(query)) ||
        (d.headOfDepartment && d.headOfDepartment.toLowerCase().includes(query))
      );
    }
    return list;
  });

  sortedDepartments = computed(() => {
    let list = this.filteredDepartments();
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

  paginatedDepartments = computed(() => {
    const list = this.sortedDepartments();
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
  currentDepartmentId = signal<number>(0);
  modalSubmitting = signal<boolean>(false);
  departmentForm!: FormGroup;

  // View Modal state
  isViewModalOpen = signal<boolean>(false);
  viewingDepartment = signal<DepartmentDto | null>(null);

  openViewModal(d: DepartmentDto): void {
    this.viewingDepartment.set(d);
    this.isViewModalOpen.set(true);
  }

  closeViewModal(): void {
    this.isViewModalOpen.set(false);
    this.viewingDepartment.set(null);
  }

  ngOnInit(): void {
    this.initForm();
    this.loadDepartments();
  }

  private initForm(): void {
    this.departmentForm = this.fb.group({
      departmentName: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]],
      designations: [''],
      headOfDepartment: [''],
      displayOrder: [0, [Validators.min(0)]],
      isActive: [true]
    });
  }

  loadDepartments(): void {
    this.loading.set(true);
    this.adminDataService.getDepartments().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.departments.set(res.data);
        } else {
          this.toastService.error(res.message || 'Failed to load departments', 'Database Error');
        }
      },
      error: () => {
        this.loading.set(false);
        this.toastService.error('Error connecting to Departments API', 'Network Error');
      }
    });
  }

  openAddModal(): void {
    this.isEditMode.set(false);
    this.currentDepartmentId.set(0);
    this.departmentForm.reset({
      departmentName: '',
      description: '',
      designations: '',
      headOfDepartment: '',
      displayOrder: this.departments().length + 1,
      isActive: true
    });
    this.isModalOpen.set(true);
  }

  openEditModal(dept: DepartmentDto): void {
    this.isEditMode.set(true);
    this.currentDepartmentId.set(dept.id);
    this.departmentForm.reset({
      departmentName: dept.departmentName,
      description: dept.description || '',
      designations: dept.designations || '',
      headOfDepartment: dept.headOfDepartment || '',
      displayOrder: dept.displayOrder,
      isActive: dept.isActive
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  saveDepartment(): void {
    if (this.departmentForm.invalid) {
      this.departmentForm.markAllAsTouched();
      this.toastService.error('Department Name is required.', 'Form Incomplete');
      return;
    }

    this.modalSubmitting.set(true);
    const val = this.departmentForm.value;

    const dto: SaveDepartmentDto = {
      id: this.currentDepartmentId(),
      departmentName: val.departmentName.trim(),
      description: val.description?.trim(),
      designations: val.designations?.trim(),
      headOfDepartment: val.headOfDepartment?.trim(),
      displayOrder: Number(val.displayOrder) || 0,
      isActive: val.isActive
    };

    this.adminDataService.saveDepartment(dto).subscribe({
      next: (res) => {
        this.modalSubmitting.set(false);
        if (res.success) {
          this.toastService.success(
            this.isEditMode() ? `Department "${val.departmentName}" updated.` : `Department "${val.departmentName}" created.`,
            'Saved to Database'
          );
          this.closeModal();
          this.loadDepartments();
        } else {
          this.toastService.error(res.message || 'Failed to save department.', 'Error');
        }
      },
      error: () => {
        this.modalSubmitting.set(false);
        this.toastService.error('Error saving department to database.', 'Error');
      }
    });
  }

  async deleteDepartment(dept: DepartmentDto): Promise<void> {
    const confirmed = await this.confirmService.danger(
      'Delete Department',
      `Are you sure you want to delete department "${dept.departmentName}"?`
    );

    if (confirmed) {
      this.adminDataService.deleteDepartment(dept.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.success(`Department "${dept.departmentName}" deleted.`, 'Deleted');
            this.loadDepartments();
          } else {
            this.toastService.error(res.message || 'Failed to delete department', 'Error');
          }
        },
        error: () => {
          this.toastService.error('Network error deleting department', 'Error');
        }
      });
    }
  }

  getDesignationList(designationsText?: string): string[] {
    if (!designationsText) return [];
    return designationsText.split(',').map(s => s.trim()).filter(s => s.length > 0);
  }
}
