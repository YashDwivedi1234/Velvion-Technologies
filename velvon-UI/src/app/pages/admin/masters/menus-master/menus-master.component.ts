import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { ToastService, ConfirmService, IconComponent, ToggleComponent } from '../../../../shared';
import { MenuDto, SaveMenuDto } from '../../../../core/models/api.models';

@Component({
  selector: 'app-menus-master',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IconComponent, ToggleComponent],
  templateUrl: './menus-master.component.html',
  styleUrls: ['./menus-master.component.css']
})
export class MenusMasterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private adminDataService = inject(AdminDataService);
  private toastService = inject(ToastService);
  private confirmService = inject(ConfirmService);

  loading = signal<boolean>(false);
  menus = signal<MenuDto[]>([]);
  isModalOpen = signal<boolean>(false);
  isEditMode = signal<boolean>(false);
  currentMenuId = signal<number>(0);
  modalSubmitting = signal<boolean>(false);
  menuForm!: FormGroup;

  ngOnInit(): void {
    this.initForm();
    this.loadMenus();
  }

  private initForm(): void {
    this.menuForm = this.fb.group({
      menuName: ['', [Validators.required, Validators.maxLength(100)]],
      routeUrl: [''],
      icon: ['grid'],
      parentMenuId: [0],
      isActive: [true]
    });
  }

  loadMenus(): void {
    this.loading.set(true);
    this.adminDataService.getMenus().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.menus.set(res.data);
        } else {
          this.toastService.error(res.message || 'Failed to load menus', 'Database Error');
        }
      },
      error: () => {
        this.loading.set(false);
        this.toastService.error('Error fetching menus from MySQL', 'Network Error');
      }
    });
  }

  openAddModal(): void {
    this.isEditMode.set(false);
    this.currentMenuId.set(0);
    this.menuForm.reset({
      menuName: '',
      routeUrl: '',
      icon: 'grid',
      parentMenuId: 0,
      isActive: true
    });
    this.isModalOpen.set(true);
  }

  openEditModal(menu: MenuDto): void {
    this.isEditMode.set(true);
    this.currentMenuId.set(menu.id);
    this.menuForm.reset({
      menuName: menu.menuName,
      routeUrl: menu.routeUrl || '',
      icon: menu.icon || 'grid',
      parentMenuId: menu.parentMenuId || 0,
      isActive: menu.isActive
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  saveMenu(): void {
    if (this.menuForm.invalid) {
      this.menuForm.markAllAsTouched();
      this.toastService.error('Menu Name is required.', 'Form Incomplete');
      return;
    }

    this.modalSubmitting.set(true);
    const val = this.menuForm.value;

    const dto: SaveMenuDto = {
      id: this.currentMenuId(),
      menuName: val.menuName.trim(),
      routeUrl: val.routeUrl?.trim(),
      icon: val.icon?.trim(),
      parentMenuId: Number(val.parentMenuId) || 0,
      isActive: val.isActive
    };

    this.adminDataService.saveMenu(dto).subscribe({
      next: (res) => {
        this.modalSubmitting.set(false);
        if (res.success) {
          this.toastService.success(
            this.isEditMode() ? `Menu "${val.menuName}" updated in database.` : `Menu "${val.menuName}" created in database.`,
            'Saved to Database'
          );
          this.closeModal();
          this.loadMenus();
        } else {
          this.toastService.error(res.message || 'Failed to save menu.', 'Error');
        }
      },
      error: () => {
        this.modalSubmitting.set(false);
        this.toastService.error('Error saving menu to database.', 'Error');
      }
    });
  }

  async deleteMenu(menu: MenuDto): Promise<void> {
    const confirmed = await this.confirmService.danger(
      'Delete Menu Item',
      `Are you sure you want to delete menu "${menu.menuName}" (${menu.routeUrl})?`
    );

    if (confirmed) {
      this.adminDataService.deleteMenu(menu.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.success(`Menu "${menu.menuName}" removed from database.`, 'Deleted');
            this.loadMenus();
          } else {
            this.toastService.error(res.message || 'Failed to delete menu', 'Error');
          }
        },
        error: () => {
          this.toastService.error('Network error deleting menu', 'Error');
        }
      });
    }
  }
}
