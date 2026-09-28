import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { AdminDataService } from '../../../core/services/admin-data.service';
import {
  ToastService,
  ConfirmService,
  IconComponent,
  DropdownComponent,
  DropdownOption,
  ToggleComponent,
  PaginationComponent
} from '../../../shared';
import { UserDto, RoleDto, SaveUserDto } from '../../../core/models/api.models';

@Component({
  selector: 'app-users',
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
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css']
})
export class UsersComponent implements OnInit {
  private fb = inject(FormBuilder);
  private adminDataService = inject(AdminDataService);
  private toastService = inject(ToastService);
  private confirmService = inject(ConfirmService);

  loading = signal<boolean>(false);
  users = signal<UserDto[]>([]);
  filteredUsers = signal<UserDto[]>([]);
  roles = signal<RoleDto[]>([]);
  roleDropdownOptions = signal<DropdownOption[]>([
    { label: 'All Roles', value: 'all', icon: 'shield' }
  ]);
  modalRoleDropdownOptions = signal<DropdownOption[]>([]);

  // Search & Filter state
  searchQuery = signal<string>('');
  selectedRoleFilter = signal<string>('all');

  // Sorting state
  sortColumn = signal<string>('fullName');
  sortDirection = signal<'asc' | 'desc'>('asc');

  // Pagination state
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);

  paginatedUsers = computed(() => {
    const list = this.filteredUsers();
    const start = (this.currentPage() - 1) * this.pageSize();
    return list.slice(start, start + this.pageSize());
  });

  // Modal State
  isModalOpen = signal<boolean>(false);
  isEditMode = signal<boolean>(false);
  currentUserId = signal<number>(0);
  modalSubmitting = signal<boolean>(false);
  userForm!: FormGroup;

  // View Modal State
  isViewModalOpen = signal<boolean>(false);
  viewingUser = signal<UserDto | null>(null);

  openViewModal(user: UserDto): void {
    this.viewingUser.set(user);
    this.isViewModalOpen.set(true);
  }

  closeViewModal(): void {
    this.isViewModalOpen.set(false);
    this.viewingUser.set(null);
  }

  ngOnInit(): void {
    this.initForm();
    this.loadRoles();
    this.loadUsers();
  }

  private initForm(): void {
    this.userForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      mobile: [''],
      roleId: [1, Validators.required],
      password: [''],
      isActive: [true]
    });
  }

  loadRoles(): void {
    this.adminDataService.getRoles(true).subscribe(res => {
      if (res.success && res.data) {
        this.roles.set(res.data);
        const options: DropdownOption[] = [
          { label: 'All Roles', value: 'all', icon: 'shield' },
          ...res.data.map(r => ({
            label: r.roleName,
            value: r.id.toString(),
            icon: 'shield'
          }))
        ];
        this.roleDropdownOptions.set(options);

        const modalOptions: DropdownOption[] = res.data.map(r => ({
          label: r.roleName,
          value: r.id,
          icon: 'shield'
        }));
        this.modalRoleDropdownOptions.set(modalOptions);
      }
    });
  }

  loadUsers(): void {
    this.loading.set(true);
    this.adminDataService.getUsers().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.users.set(res.data);
          this.applyFilter();
        } else {
          this.toastService.error(res.message || 'Failed to load users', 'Database Error');
        }
      },
      error: () => {
        this.loading.set(false);
        this.toastService.error('Error connecting to MySQL Users API', 'Network Error');
      }
    });
  }

  applyFilter(): void {
    const q = this.searchQuery().toLowerCase().trim();
    const roleF = this.selectedRoleFilter();

    let list = this.users();

    if (roleF !== 'all') {
      const roleId = Number(roleF);
      list = list.filter(u => u.roleId === roleId);
    }

    if (q) {
      list = list.filter(u => 
        u.fullName.toLowerCase().includes(q) ||
        u.email.toLowerCase().includes(q) ||
        (u.mobile && u.mobile.toLowerCase().includes(q)) ||
        u.roleName.toLowerCase().includes(q)
      );
    }

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

    this.filteredUsers.set(list);
    this.currentPage.set(1);
  }

  toggleSort(column: string): void {
    if (this.sortColumn() === column) {
      this.sortDirection.update(dir => dir === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortColumn.set(column);
      this.sortDirection.set('asc');
    }
    this.applyFilter();
  }

  onRoleSelect(value: any): void {
    const val = value !== null && value !== undefined ? value.toString() : 'all';
    this.selectedRoleFilter.set(val);
    this.applyFilter();
  }

  onSearchChange(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    this.searchQuery.set(val);
    this.applyFilter();
  }

  resetFilters(): void {
    this.searchQuery.set('');
    this.selectedRoleFilter.set('all');
    this.applyFilter();
  }

  openAddModal(): void {
    this.isEditMode.set(false);
    this.currentUserId.set(0);
    this.userForm.reset({
      roleId: this.roles().length > 0 ? this.roles()[0].id : 1,
      isActive: true
    });
    this.userForm.get('password')?.setValidators([Validators.required, Validators.minLength(6)]);
    this.userForm.get('password')?.updateValueAndValidity();
    this.isModalOpen.set(true);
  }

  openEditModal(user: UserDto): void {
    this.isEditMode.set(true);
    this.currentUserId.set(user.id);
    this.userForm.reset({
      fullName: user.fullName,
      email: user.email,
      mobile: user.mobile || '',
      roleId: user.roleId,
      password: '',
      isActive: user.isActive
    });
    // Password is optional on update
    this.userForm.get('password')?.clearValidators();
    this.userForm.get('password')?.updateValueAndValidity();
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  saveUser(): void {
    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      this.toastService.error('Please fix validation errors before saving.', 'Form Incomplete');
      return;
    }

    this.modalSubmitting.set(true);
    const val = this.userForm.value;

    const dto: SaveUserDto = {
      id: this.currentUserId(),
      roleId: Number(val.roleId),
      fullName: val.fullName.trim(),
      email: val.email.trim().toLowerCase(),
      mobile: val.mobile?.trim(),
      password: val.password ? val.password : undefined,
      isActive: val.isActive
    };

    this.adminDataService.saveUser(dto).subscribe({
      next: (res) => {
        this.modalSubmitting.set(false);
        if (res.success) {
          this.toastService.success(
            this.isEditMode() ? `User "${val.fullName}" updated in MySQL.` : `New user "${val.fullName}" created in MySQL.`,
            'Saved to Database'
          );
          this.closeModal();
          this.loadUsers();
        } else {
          this.toastService.error(res.message || 'Failed to save user.', 'Save Error');
        }
      },
      error: () => {
        this.modalSubmitting.set(false);
        this.toastService.error('Server error saving user.', 'Error');
      }
    });
  }

  async deleteUser(user: UserDto): Promise<void> {
    const confirmed = await this.confirmService.danger(
      'Delete User',
      `Are you sure you want to permanently delete user "${user.fullName}" (${user.email})?`
    );

    if (confirmed) {
      this.adminDataService.deleteUser(user.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.success(`User "${user.fullName}" deleted from database.`, 'Deleted');
            this.loadUsers();
          } else {
            this.toastService.error(res.message || 'Failed to delete user.', 'Error');
          }
        },
        error: () => {
          this.toastService.error('Connection error deleting user.', 'Error');
        }
      });
    }
  }
}
