import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminDataService } from '../../../core/services/admin-data.service';
import { ToastService, ConfirmService, IconComponent, ToggleComponent } from '../../../shared';
import { RoleDto, SaveRoleDto } from '../../../core/models/api.models';

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IconComponent, ToggleComponent],
  templateUrl: './roles.component.html',
  styleUrls: ['./roles.component.css']
})
export class RolesComponent implements OnInit {
  private fb = inject(FormBuilder);
  private adminDataService = inject(AdminDataService);
  private toastService = inject(ToastService);
  private confirmService = inject(ConfirmService);

  loading = signal<boolean>(false);
  roles = signal<RoleDto[]>([]);
  isModalOpen = signal<boolean>(false);
  isEditMode = signal<boolean>(false);
  currentRoleId = signal<number>(0);
  modalSubmitting = signal<boolean>(false);
  roleForm!: FormGroup;

  ngOnInit(): void {
    this.initForm();
    this.loadRoles();
  }

  private initForm(): void {
    this.roleForm = this.fb.group({
      roleName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
      isActive: [true]
    });
  }

  loadRoles(): void {
    this.loading.set(true);
    this.adminDataService.getRoles().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.roles.set(res.data);
        } else {
          this.toastService.error(res.message || 'Failed to load roles', 'Database Error');
        }
      },
      error: () => {
        this.loading.set(false);
        this.toastService.error('Error connecting to MySQL Roles API', 'Network Error');
      }
    });
  }

  openAddModal(): void {
    this.isEditMode.set(false);
    this.currentRoleId.set(0);
    this.roleForm.reset({
      roleName: '',
      isActive: true
    });
    this.isModalOpen.set(true);
  }

  openEditModal(role: RoleDto): void {
    this.isEditMode.set(true);
    this.currentRoleId.set(role.id);
    this.roleForm.reset({
      roleName: role.roleName,
      isActive: role.isActive
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  saveRole(): void {
    if (this.roleForm.invalid) {
      this.roleForm.markAllAsTouched();
      this.toastService.error('Please enter a valid role name.', 'Form Incomplete');
      return;
    }

    this.modalSubmitting.set(true);
    const val = this.roleForm.value;

    const dto: SaveRoleDto = {
      id: this.currentRoleId(),
      roleName: val.roleName.trim(),
      isActive: val.isActive
    };

    this.adminDataService.saveRole(dto).subscribe({
      next: (res) => {
        this.modalSubmitting.set(false);
        if (res.success) {
          this.toastService.success(
            this.isEditMode() ? `Role "${val.roleName}" updated in database.` : `Role "${val.roleName}" created successfully.`,
            'Saved to Database'
          );
          this.closeModal();
          this.loadRoles();
        } else {
          this.toastService.error(res.message || 'Failed to save role', 'Error');
        }
      },
      error: () => {
        this.modalSubmitting.set(false);
        this.toastService.error('Connection error saving role', 'Error');
      }
    });
  }

  async deleteRole(role: RoleDto): Promise<void> {
    const confirmed = await this.confirmService.danger(
      'Delete Role',
      `Are you sure you want to delete role "${role.roleName}"? All associated permissions will be impacted.`
    );

    if (confirmed) {
      this.adminDataService.deleteRole(role.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.success(`Role "${role.roleName}" deleted from database.`, 'Deleted');
            this.loadRoles();
          } else {
            this.toastService.error(res.message || 'Failed to delete role', 'Error');
          }
        },
        error: () => {
          this.toastService.error('Connection error deleting role', 'Error');
        }
      });
    }
  }
}
