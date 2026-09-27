import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminDataService } from '../../../core/services/admin-data.service';
import { ToastService, IconComponent, DropdownComponent, DropdownOption } from '../../../shared';
import { RoleDto, MenuDto, RoleMenuPermissionDto, BulkSaveRolePermissionDto, MenuPermissionItemDto } from '../../../core/models/api.models';

interface MatrixRow {
  menuId: number;
  menuName: string;
  routeUrl?: string;
  icon?: string;
  canView: boolean;
  canAdd: boolean;
  canEdit: boolean;
  canDelete: boolean;
}

@Component({
  selector: 'app-role-permissions',
  standalone: true,
  imports: [CommonModule, FormsModule, IconComponent, DropdownComponent],
  templateUrl: './role-permissions.component.html',
  styleUrls: ['./role-permissions.component.css']
})
export class RolePermissionsComponent implements OnInit {
  private adminDataService = inject(AdminDataService);
  private toastService = inject(ToastService);

  loading = signal<boolean>(false);
  saving = signal<boolean>(false);

  roles = signal<RoleDto[]>([]);
  roleDropdownOptions = signal<DropdownOption[]>([]);
  selectedRoleId = signal<number>(1);

  menus = signal<MenuDto[]>([]);
  matrixRows = signal<MatrixRow[]>([]);

  ngOnInit(): void {
    this.loadRolesAndMenus();
  }

  loadRolesAndMenus(): void {
    this.loading.set(true);

    this.adminDataService.getRoles(true).subscribe(rolesRes => {
      if (rolesRes.success && rolesRes.data && rolesRes.data.length > 0) {
        this.roles.set(rolesRes.data);
        const options: DropdownOption[] = rolesRes.data.map(r => ({
          label: r.roleName,
          value: r.id.toString(),
          icon: 'shield'
        }));
        this.roleDropdownOptions.set(options);
        this.selectedRoleId.set(rolesRes.data[0].id);

        // Load Menus
        this.adminDataService.getMenus().subscribe(menusRes => {
          this.loading.set(false);
          if (menusRes.success && menusRes.data) {
            this.menus.set(menusRes.data);
            this.loadPermissionsForRole(this.selectedRoleId());
          }
        });
      } else {
        this.loading.set(false);
      }
    });
  }

  onRoleSelect(roleIdStr: string | string[]): void {
    const roleId = Number(roleIdStr);
    this.selectedRoleId.set(roleId);
    this.loadPermissionsForRole(roleId);
  }

  loadPermissionsForRole(roleId: number): void {
    this.loading.set(true);

    this.adminDataService.getPermissionsByRoleId(roleId).subscribe({
      next: (res) => {
        this.loading.set(false);
        const existingPerms: RoleMenuPermissionDto[] = (res.success && res.data) ? res.data : [];
        const permMap = new Map<number, RoleMenuPermissionDto>();
        existingPerms.forEach(p => permMap.set(p.menuId, p));

        // Construct matrix rows from all menus
        const rows: MatrixRow[] = this.menus().map(m => {
          const perm = permMap.get(m.id);
          return {
            menuId: m.id,
            menuName: m.menuName,
            routeUrl: m.routeUrl,
            icon: m.icon,
            canView: perm ? perm.canView : true,
            canAdd: perm ? perm.canAdd : false,
            canEdit: perm ? perm.canEdit : false,
            canDelete: perm ? perm.canDelete : false
          };
        });

        this.matrixRows.set(rows);
      },
      error: () => {
        this.loading.set(false);
        this.toastService.error('Error fetching role permissions', 'Error');
      }
    });
  }

  toggleAllColumn(permissionType: 'canView' | 'canAdd' | 'canEdit' | 'canDelete'): void {
    const current = this.matrixRows();
    const allChecked = current.every(r => r[permissionType]);
    const nextVal = !allChecked;

    this.matrixRows.set(current.map(r => ({
      ...r,
      [permissionType]: nextVal
    })));
  }

  savePermissions(): void {
    this.saving.set(true);

    const permissions: MenuPermissionItemDto[] = this.matrixRows().map(r => ({
      menuId: r.menuId,
      canView: r.canView,
      canAdd: r.canAdd,
      canEdit: r.canEdit,
      canDelete: r.canDelete
    }));

    const dto: BulkSaveRolePermissionDto = {
      roleId: this.selectedRoleId(),
      permissions
    };

    this.adminDataService.saveRolePermissions(dto).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          const roleName = this.roles().find(r => r.id === this.selectedRoleId())?.roleName || 'Role';
          this.toastService.success(`Permissions for ${roleName} updated in MySQL database.`, 'Permissions Saved');
        } else {
          this.toastService.error(res.message || 'Failed to update permissions.', 'Error');
        }
      },
      error: () => {
        this.saving.set(false);
        this.toastService.error('Network error updating permissions.', 'Error');
      }
    });
  }
}
