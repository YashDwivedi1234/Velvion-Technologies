import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { AdminDataService } from '../../../core/services/admin-data.service';
import {
  ToastService,
  ConfirmService,
  IconComponent,
  DropdownComponent,
  DropdownOption,
  ToggleComponent
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
    ToggleComponent
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
  roleDropdownOptions = signal<DropdownOption[]>([]);

  // Search & Filter state
  searchQuery = signal<string>('');
  selectedRoleFilter = signal<string>('all');

  // Modal State
  isModalOpen = signal<boolean>(false);
  isEditMode = signal<boolean>(false);
  currentUserId = signal<number>(0);
  modalSubmitting = signal<boolean>(false);
  userForm!: FormGroup;

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
        const options: DropdownOption[] = res.data.map(r => ({
          label: r.roleName,
          value: r.id.toString(),
          icon: 'shield'
        }));
        this.roleDropdownOptions.set(options);
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

    this.filteredUsers.set(list);
  }

  onSearchChange(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    this.searchQuery.set(val);
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
