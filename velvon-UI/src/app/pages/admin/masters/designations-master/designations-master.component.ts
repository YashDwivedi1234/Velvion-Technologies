import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import {
  ToastService,
  ConfirmService,
  IconComponent,
  DropdownComponent,
  DropdownOption,
  ToggleComponent,
  PaginationComponent
} from '../../../../shared';
import { DesignationDto, SaveDesignationDto, DepartmentDto } from '../../../../core/models/api.models';

@Component({
  selector: 'app-designations-master',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IconComponent,
    DropdownComponent,
    ToggleComponent,
    PaginationComponent
  ],
  templateUrl: './designations-master.component.html',
  styleUrls: ['./designations-master.component.css']
})
export class DesignationsMasterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private adminDataService = inject(AdminDataService);
  private toastService = inject(ToastService);
  private confirmService = inject(ConfirmService);

  loading = signal<boolean>(false);
  designations = signal<DesignationDto[]>([]);
  departments = signal<DepartmentDto[]>([]);
  searchQuery = signal<string>('');
  selectedDepartmentFilter = signal<string>('all');

  // Department Dropdown Options for Filter
  departmentFilterOptions = signal<DropdownOption[]>([
    { label: 'All Departments', value: 'all', icon: 'briefcase' }
  ]);

  // Department Dropdown Options for Form Modal
  modalDepartmentOptions = signal<DropdownOption[]>([
    { label: 'No Department (Global)', value: 0, icon: 'briefcase' }
  ]);

  // Sorting state
  sortColumn = signal<string>('displayOrder');
  sortDirection = signal<'asc' | 'desc'>('asc');

  // Pagination
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);

  filteredDesignations = computed(() => {
    let list = this.designations();
    const query = this.searchQuery().toLowerCase().trim();
    const deptFilter = this.selectedDepartmentFilter();

    if (deptFilter !== 'all') {
      const deptId = Number(deptFilter);
      list = list.filter(d => d.departmentId === deptId);
    }

    if (query) {
      list = list.filter(d =>
        d.designationName.toLowerCase().includes(query) ||
        (d.departmentName && d.departmentName.toLowerCase().includes(query)) ||
        (d.description && d.description.toLowerCase().includes(query))
      );
    }
    return list;
  });

  sortedDesignations = computed(() => {
    let list = this.filteredDesignations();
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

  paginatedDesignations = computed(() => {
    const list = this.sortedDesignations();
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

  onDepartmentFilterChange(value: any): void {
    const val = value !== null && value !== undefined ? value.toString() : 'all';
    this.selectedDepartmentFilter.set(val);
    this.currentPage.set(1);
  }

  isModalOpen = signal<boolean>(false);
  isEditMode = signal<boolean>(false);
  currentDesignationId = signal<number>(0);
  modalSubmitting = signal<boolean>(false);
  designationForm!: FormGroup;

  // View Modal state
  isViewModalOpen = signal<boolean>(false);
  viewingDesignation = signal<DesignationDto | null>(null);

  openViewModal(d: DesignationDto): void {
    this.viewingDesignation.set(d);
    this.isViewModalOpen.set(true);
  }

  closeViewModal(): void {
    this.isViewModalOpen.set(false);
    this.viewingDesignation.set(null);
  }

  ngOnInit(): void {
    this.initForm();
    this.loadDepartments();
    this.loadDesignations();
  }

  private initForm(): void {
    this.designationForm = this.fb.group({
      designationName: ['', [Validators.required, Validators.maxLength(100)]],
      departmentId: [0],
      description: ['', [Validators.maxLength(500)]],
      displayOrder: [0, [Validators.min(0)]],
      isActive: [true]
    });
  }

  loadDepartments(): void {
    this.adminDataService.getDepartments(true).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.departments.set(res.data);
          
          this.departmentFilterOptions.set([
            { label: 'All Departments', value: 'all', icon: 'briefcase' },
            ...res.data.map(d => ({
              label: d.departmentName,
              value: d.id.toString(),
              icon: 'briefcase'
            }))
          ]);

          this.modalDepartmentOptions.set([
            { label: 'No Department (Global)', value: 0, icon: 'globe' },
            ...res.data.map(d => ({
              label: d.departmentName,
              value: d.id,
              icon: 'briefcase'
            }))
          ]);
        }
      }
    });
  }

  loadDesignations(): void {
    this.loading.set(true);
    this.adminDataService.getDesignations().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.designations.set(res.data);
        } else {
          this.toastService.error(res.message || 'Failed to load designations', 'Database Error');
        }
      },
      error: () => {
        this.loading.set(false);
        this.toastService.error('Error connecting to Designations API', 'Network Error');
      }
    });
  }

  openAddModal(): void {
    this.isEditMode.set(false);
    this.currentDesignationId.set(0);
    this.designationForm.reset({
      designationName: '',
      departmentId: this.departments().length > 0 ? this.departments()[0].id : 0,
      description: '',
      displayOrder: this.designations().length + 1,
      isActive: true
    });
    this.isModalOpen.set(true);
  }

  openEditModal(desig: DesignationDto): void {
    this.isEditMode.set(true);
    this.currentDesignationId.set(desig.id);
    this.designationForm.reset({
      designationName: desig.designationName,
      departmentId: desig.departmentId || 0,
      description: desig.description || '',
      displayOrder: desig.displayOrder,
      isActive: desig.isActive
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  saveDesignation(): void {
    if (this.designationForm.invalid) {
      this.designationForm.markAllAsTouched();
      this.toastService.error('Designation Name is required.', 'Form Incomplete');
      return;
    }

    this.modalSubmitting.set(true);
    const val = this.designationForm.value;

    const dto: SaveDesignationDto = {
      id: this.currentDesignationId(),
      designationName: val.designationName.trim(),
      departmentId: Number(val.departmentId) > 0 ? Number(val.departmentId) : undefined,
      description: val.description?.trim(),
      displayOrder: Number(val.displayOrder) || 0,
      isActive: val.isActive
    };

    this.adminDataService.saveDesignation(dto).subscribe({
      next: (res) => {
        this.modalSubmitting.set(false);
        if (res.success) {
          this.toastService.success(
            this.isEditMode() ? `Designation "${val.designationName}" updated.` : `Designation "${val.designationName}" created.`,
            'Saved to Database'
          );
          this.closeModal();
          this.loadDesignations();
        } else {
          this.toastService.error(res.message || 'Failed to save designation.', 'Error');
        }
      },
      error: () => {
        this.modalSubmitting.set(false);
        this.toastService.error('Error saving designation to database.', 'Error');
      }
    });
  }

  async deleteDesignation(desig: DesignationDto): Promise<void> {
    const confirmed = await this.confirmService.danger(
      'Delete Designation',
      `Are you sure you want to delete designation "${desig.designationName}"?`
    );

    if (confirmed) {
      this.adminDataService.deleteDesignation(desig.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.success(`Designation "${desig.designationName}" deleted.`, 'Deleted');
            this.loadDesignations();
          } else {
            this.toastService.error(res.message || 'Failed to delete designation', 'Error');
          }
        },
        error: () => {
          this.toastService.error('Network error deleting designation', 'Error');
        }
      });
    }
  }
}
