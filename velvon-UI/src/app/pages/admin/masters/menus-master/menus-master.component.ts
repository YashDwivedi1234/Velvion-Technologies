import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { ToastService, ConfirmService, IconComponent, ToggleComponent, PaginationComponent } from '../../../../shared';
import { MenuDto, SaveMenuDto } from '../../../../core/models/api.models';

@Component({
  selector: 'app-menus-master',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IconComponent, ToggleComponent, PaginationComponent],
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

  // Sorting state
  sortColumn = signal<string>('menuName');
  sortDirection = signal<'asc' | 'desc'>('asc');

  // Pagination
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);

  sortedMenus = computed(() => {
    let list = this.menus();
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

  paginatedMenus = computed(() => {
    const list = this.sortedMenus();
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
  currentMenuId = signal<number>(0);
  modalSubmitting = signal<boolean>(false);
  menuForm!: FormGroup;

  // View Modal state
  isViewModalOpen = signal<boolean>(false);
  viewingMenu = signal<MenuDto | null>(null);

  openViewModal(m: MenuDto): void {
    this.viewingMenu.set(m);
    this.isViewModalOpen.set(true);
  }

  closeViewModal(): void {
    this.isViewModalOpen.set(false);
    this.viewingMenu.set(null);
  }

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
        const DEFAULT_PLATFORM_MENUS: MenuDto[] = [
          { id: 1, menuName: 'Dashboard', routeUrl: '/admin/dashboard', icon: 'grid', isActive: true },
          { id: 2, menuName: 'Users', routeUrl: '/admin/users', icon: 'users', isActive: true },
          { id: 3, menuName: 'Roles', routeUrl: '/admin/roles', icon: 'shield', isActive: true },
          { id: 4, menuName: 'Role Menu Permissions', routeUrl: '/admin/permissions', icon: 'check-circle', isActive: true },
          { id: 5, menuName: 'Menus Master', routeUrl: '/admin/masters/menus', icon: 'menu', isActive: true },
          { id: 6, menuName: 'Services Master', routeUrl: '/admin/masters/services', icon: 'layers', isActive: true },
          { id: 7, menuName: 'Settings Master', routeUrl: '/admin/masters/settings', icon: 'settings', isActive: true },
          { id: 8, menuName: 'Blogs & Articles', routeUrl: '/admin/blogs', icon: 'edit', isActive: true },
          { id: 9, menuName: 'Portfolios', routeUrl: '/admin/portfolios', icon: 'folder', isActive: true },
          { id: 10, menuName: 'Team Members', routeUrl: '/admin/team', icon: 'user', isActive: true },
          { id: 11, menuName: 'Testimonials', routeUrl: '/admin/testimonials', icon: 'star', isActive: true },
          { id: 12, menuName: 'Inquiries & Leads', routeUrl: '/admin/inquiries', icon: 'mail', isActive: true },
          { id: 13, menuName: 'Audit Trail', routeUrl: '/admin/audit-logs', icon: 'activity', isActive: true }
        ];

        let allMenus = [...DEFAULT_PLATFORM_MENUS];
        if (res.success && res.data && res.data.length > 0) {
          const dbList = res.data;
          const map = new Map<string, MenuDto>();
          allMenus.forEach(m => map.set(m.menuName.toLowerCase(), m));
          dbList.forEach(m => map.set(m.menuName.toLowerCase(), {
            ...m,
            icon: m.icon || map.get(m.menuName.toLowerCase())?.icon || 'grid',
            routeUrl: m.routeUrl || map.get(m.menuName.toLowerCase())?.routeUrl
          }));
          allMenus = Array.from(map.values());
        }
        this.menus.set(allMenus);
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
