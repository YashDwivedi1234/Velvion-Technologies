import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminDataService } from '../../../core/services/admin-data.service';
import { ToastService, IconComponent, DropdownComponent, DropdownOption } from '../../../shared';
import { RoleDto, MenuDto, RoleMenuPermissionDto, BulkSaveRolePermissionDto, MenuPermissionItemDto } from '../../../core/models/api.models';

export interface MatrixRow {
  menuId: number;
  menuName: string;
  routeUrl?: string;
  icon?: string;
  parentGroup: string;
  canView: boolean;
  canAdd: boolean;
  canEdit: boolean;
  canDelete: boolean;
}

export interface MenuGroup {
  groupName: string;
  groupIcon: string;
  rows: MatrixRow[];
}

interface PlatformMenuDef extends MenuDto {
  parentGroup: string;
}

const DEFAULT_PLATFORM_MENUS: PlatformMenuDef[] = [
  // 1) Dashboard
  { id: 1, menuName: 'Dashboard', routeUrl: '/admin/dashboard', icon: 'grid', isActive: true, parentGroup: 'Dashboard' },

  // 2) User Management
  { id: 2, menuName: 'Users', routeUrl: '/admin/users', icon: 'users', isActive: true, parentGroup: 'User Management' },
  { id: 3, menuName: 'Roles', routeUrl: '/admin/roles', icon: 'shield', isActive: true, parentGroup: 'User Management' },
  { id: 4, menuName: 'Role Menu Permissions', routeUrl: '/admin/permissions', icon: 'check-circle', isActive: true, parentGroup: 'User Management' },

  // 3) Masters
  { id: 6, menuName: 'Services Master', routeUrl: '/admin/masters/services', icon: 'layers', isActive: true, parentGroup: 'Masters' },
  { id: 7, menuName: 'Settings Master', routeUrl: '/admin/masters/settings', icon: 'settings', isActive: true, parentGroup: 'Masters' },

  // 4) Content & Showcase
  { id: 8, menuName: 'Blogs & Articles', routeUrl: '/admin/blogs', icon: 'edit', isActive: true, parentGroup: 'Content & Showcase' },
  { id: 9, menuName: 'Portfolios', routeUrl: '/admin/portfolios', icon: 'folder', isActive: true, parentGroup: 'Content & Showcase' },
  { id: 10, menuName: 'Team Members', routeUrl: '/admin/team', icon: 'user', isActive: true, parentGroup: 'Content & Showcase' },
  { id: 11, menuName: 'Testimonials', routeUrl: '/admin/testimonials', icon: 'star', isActive: true, parentGroup: 'Content & Showcase' },

  // 5) Operations & Logs
  { id: 12, menuName: 'Inquiries & Leads', routeUrl: '/admin/inquiries', icon: 'mail', isActive: true, parentGroup: 'Operations & Logs' },
  { id: 13, menuName: 'Audit Trail', routeUrl: '/admin/audit-logs', icon: 'activity', isActive: true, parentGroup: 'Operations & Logs' }
];

function resolveParentGroup(menuName: string): { group: string; icon: string } {
  const name = menuName.toLowerCase();
  if (name.includes('dash')) return { group: 'Dashboard', icon: 'grid' };
  if (name.includes('user') || name.includes('role') || name.includes('perm')) return { group: 'User Management', icon: 'shield' };
  if (name.includes('menu') || name.includes('service') || name.includes('setting') || name.includes('master')) return { group: 'Masters', icon: 'layers' };
  if (name.includes('blog') || name.includes('article') || name.includes('portfolio') || name.includes('team') || name.includes('testim')) return { group: 'Content & Showcase', icon: 'folder' };
  if (name.includes('inquir') || name.includes('lead') || name.includes('audit') || name.includes('log')) return { group: 'Operations & Logs', icon: 'activity' };
  return { group: 'General Modules', icon: 'grid' };
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

  // View Mode: 'list' (Roles Directory with View/Edit) | 'matrix' (Configuring Permissions for a Role)
  viewMode = signal<'list' | 'matrix'>('list');
  searchTerm = signal<string>('');

  loading = signal<boolean>(false);
  saving = signal<boolean>(false);

  roles = signal<RoleDto[]>([]);
  roleDropdownOptions = signal<DropdownOption[]>([]);
  selectedRoleId = signal<number>(1);
  selectedRole = computed(() => this.roles().find(r => r.id === this.selectedRoleId()) || null);

  // View modal state
  isViewModalOpen = signal<boolean>(false);
  viewingRole = signal<RoleDto | null>(null);
  viewModalRows = signal<MatrixRow[]>([]);
  viewModalLoading = signal<boolean>(false);

  menus = signal<PlatformMenuDef[]>([]);
  matrixRows = signal<MatrixRow[]>([]);

  filteredRoles = computed(() => {
    const list = this.roles();
    const query = this.searchTerm().trim().toLowerCase();
    if (!query) return list;
    return list.filter(r => 
      r.roleName.toLowerCase().includes(query) || 
      r.id.toString().includes(query)
    );
  });

  // Grouped Matrix Rows into Parent Menus -> Child Submenus
  menuGroups = computed<MenuGroup[]>(() => {
    const rows = this.matrixRows();
    const groupsMap = new Map<string, { icon: string; rows: MatrixRow[] }>();
    const order = ['Dashboard', 'User Management', 'Masters', 'Content & Showcase', 'Operations & Logs'];
    const groupIcons: Record<string, string> = {
      'Dashboard': 'grid',
      'User Management': 'shield',
      'Masters': 'layers',
      'Content & Showcase': 'folder',
      'Operations & Logs': 'activity'
    };

    rows.forEach(r => {
      const g = r.parentGroup || 'General Modules';
      if (!groupsMap.has(g)) {
        groupsMap.set(g, { icon: groupIcons[g] || 'layers', rows: [] });
      }
      groupsMap.get(g)!.rows.push(r);
    });

    const result: MenuGroup[] = [];
    order.forEach(gName => {
      if (groupsMap.has(gName)) {
        result.push({
          groupName: gName,
          groupIcon: groupIcons[gName] || 'layers',
          rows: groupsMap.get(gName)!.rows
        });
        groupsMap.delete(gName);
      }
    });

    groupsMap.forEach((val, gName) => {
      result.push({
        groupName: gName,
        groupIcon: val.icon,
        rows: val.rows
      });
    });

    return result;
  });

  // Grouped View Modal Rows
  viewModalGroups = computed<MenuGroup[]>(() => {
    const rows = this.viewModalRows();
    const groupsMap = new Map<string, { icon: string; rows: MatrixRow[] }>();
    const order = ['Dashboard', 'User Management', 'Masters', 'Content & Showcase', 'Operations & Logs'];
    const groupIcons: Record<string, string> = {
      'Dashboard': 'grid',
      'User Management': 'shield',
      'Masters': 'layers',
      'Content & Showcase': 'folder',
      'Operations & Logs': 'activity'
    };

    rows.forEach(r => {
      const g = r.parentGroup || 'General Modules';
      if (!groupsMap.has(g)) {
        groupsMap.set(g, { icon: groupIcons[g] || 'layers', rows: [] });
      }
      groupsMap.get(g)!.rows.push(r);
    });

    const result: MenuGroup[] = [];
    order.forEach(gName => {
      if (groupsMap.has(gName)) {
        result.push({
          groupName: gName,
          groupIcon: groupIcons[gName] || 'layers',
          rows: groupsMap.get(gName)!.rows
        });
        groupsMap.delete(gName);
      }
    });

    groupsMap.forEach((val, gName) => {
      result.push({
        groupName: gName,
        groupIcon: val.icon,
        rows: val.rows
      });
    });

    return result;
  });

  ngOnInit(): void {
    this.loadRolesAndMenus();
  }

  loadRolesAndMenus(): void {
    this.loading.set(true);

    this.adminDataService.getRoles().subscribe(rolesRes => {
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
          let allMenus = [...DEFAULT_PLATFORM_MENUS];
          if (menusRes.success && menusRes.data && menusRes.data.length > 0) {
            const dbList = menusRes.data.filter(m => 
              !m.menuName.toLowerCase().includes('menu master') && 
              !m.menuName.toLowerCase().includes('menus master') &&
              m.routeUrl !== '/admin/masters/menus'
            );
            const map = new Map<string, PlatformMenuDef>();
            allMenus.forEach(m => map.set(m.menuName.toLowerCase(), m));
            dbList.forEach(m => {
              const info = resolveParentGroup(m.menuName);
              map.set(m.menuName.toLowerCase(), {
                ...m,
                icon: m.icon || map.get(m.menuName.toLowerCase())?.icon || 'grid',
                routeUrl: m.routeUrl || map.get(m.menuName.toLowerCase())?.routeUrl,
                parentGroup: map.get(m.menuName.toLowerCase())?.parentGroup || info.group
              });
            });
            allMenus = Array.from(map.values()).filter(m => 
              !m.menuName.toLowerCase().includes('menu master') && 
              !m.menuName.toLowerCase().includes('menus master') &&
              m.routeUrl !== '/admin/masters/menus'
            );
          }
          this.menus.set(allMenus);
        });
      } else {
        this.loading.set(false);
        this.menus.set(DEFAULT_PLATFORM_MENUS);
      }
    });
  }

  // Switch to Matrix Edit mode for a specific role
  openEditPermissions(role: RoleDto): void {
    this.selectedRoleId.set(role.id);
    this.loadPermissionsForRole(role.id);
    this.viewMode.set('matrix');
  }

  // Open View Modal to inspect permissions
  openViewPermissions(role: RoleDto): void {
    this.viewingRole.set(role);
    this.viewModalLoading.set(true);
    this.isViewModalOpen.set(true);

    this.adminDataService.getPermissionsByRoleId(role.id).subscribe({
      next: (res) => {
        this.viewModalLoading.set(false);
        const existingPerms: RoleMenuPermissionDto[] = (res.success && res.data) ? res.data : [];
        const permMap = new Map<number, RoleMenuPermissionDto>();
        existingPerms.forEach(p => permMap.set(p.menuId, p));

        const rows: MatrixRow[] = this.menus().map(m => {
          const perm = permMap.get(m.id);
          return {
            menuId: m.id,
            menuName: m.menuName,
            routeUrl: m.routeUrl,
            icon: m.icon,
            parentGroup: m.parentGroup,
            canView: perm ? perm.canView : (role.id === 1 || role.id === 2),
            canAdd: perm ? perm.canAdd : (role.id === 1),
            canEdit: perm ? perm.canEdit : (role.id === 1),
            canDelete: perm ? perm.canDelete : (role.id === 1)
          };
        });
        this.viewModalRows.set(rows);
      },
      error: () => {
        this.viewModalLoading.set(false);
      }
    });
  }

  closeViewModal(): void {
    this.isViewModalOpen.set(false);
    this.viewingRole.set(null);
  }

  editFromViewModal(): void {
    const role = this.viewingRole();
    this.closeViewModal();
    if (role) {
      this.openEditPermissions(role);
    }
  }

  backToList(): void {
    this.viewMode.set('list');
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
            parentGroup: m.parentGroup,
            canView: perm ? perm.canView : true,
            canAdd: perm ? perm.canAdd : (roleId === 1 || roleId === 2),
            canEdit: perm ? perm.canEdit : (roleId === 1 || roleId === 2),
            canDelete: perm ? perm.canDelete : (roleId === 1)
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

  toggleGroupPermission(group: MenuGroup, permType: 'canView' | 'canAdd' | 'canEdit' | 'canDelete'): void {
    const allChecked = group.rows.every(r => r[permType]);
    const nextVal = !allChecked;
    group.rows.forEach(r => r[permType] = nextVal);
  }

  isGroupAllChecked(group: MenuGroup, permType: 'canView' | 'canAdd' | 'canEdit' | 'canDelete'): boolean {
    return group.rows.length > 0 && group.rows.every(r => r[permType]);
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
          const roleName = this.selectedRole()?.roleName || 'Role';
          this.toastService.success(`Permissions for "${roleName}" saved successfully in database.`, 'Permissions Saved');
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
