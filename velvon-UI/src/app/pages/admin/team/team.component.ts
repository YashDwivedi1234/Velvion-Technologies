import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminDataService } from '../../../core/services/admin-data.service';
import { ToastService, ConfirmService, IconComponent, ToggleComponent, PaginationComponent } from '../../../shared';
import { TeamMemberDto, SaveTeamMemberDto, RoleDto } from '../../../core/models/api.models';

@Component({
  selector: 'app-team',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IconComponent, ToggleComponent, PaginationComponent],
  templateUrl: './team.component.html',
  styleUrls: ['./team.component.css']
})
export class TeamComponent implements OnInit {
  private fb = inject(FormBuilder);
  private adminDataService = inject(AdminDataService);
  private toastService = inject(ToastService);
  private confirmService = inject(ConfirmService);

  loading = signal<boolean>(false);
  teamMembers = signal<TeamMemberDto[]>([]);
  roles = signal<RoleDto[]>([]);

  // Sorting state
  sortColumn = signal<string>('fullName');
  sortDirection = signal<'asc' | 'desc'>('asc');

  // Pagination
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);

  sortedTeam = computed(() => {
    let list = this.teamMembers();
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

  paginatedTeam = computed(() => {
    const list = this.sortedTeam();
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
  currentMemberId = signal<number>(0);
  modalSubmitting = signal<boolean>(false);
  showPassword = signal<boolean>(false);
  memberForm!: FormGroup;

  // View Modal State
  isViewModalOpen = signal<boolean>(false);
  viewingMember = signal<TeamMemberDto | null>(null);

  openViewModal(member: TeamMemberDto): void {
    this.viewingMember.set(member);
    this.isViewModalOpen.set(true);
  }

  closeViewModal(): void {
    this.isViewModalOpen.set(false);
    this.viewingMember.set(null);
  }

  ngOnInit(): void {
    this.initForm();
    this.loadTeam();
    this.loadRoles();
  }

  private initForm(): void {
    this.memberForm = this.fb.group({
      fullName: ['', [Validators.required]],
      designation: ['', [Validators.required]],
      department: ['Engineering'],
      email: ['', [Validators.email]],
      mobile: [''],
      bio: [''],
      linkedInUrl: [''],
      githubUrl: [''],
      enableLoginAccess: [true],
      roleId: [null],
      password: [''],
      isActive: [true]
    });
  }

  loadRoles(): void {
    this.adminDataService.getRoles().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.roles.set(res.data);
        }
      }
    });
  }

  loadTeam(): void {
    this.loading.set(true);
    this.adminDataService.getTeamMembers().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.teamMembers.set(res.data);
        }
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  openAddModal(): void {
    this.isEditMode.set(false);
    this.currentMemberId.set(0);
    this.showPassword.set(false);

    // Default to a non-admin role or first available
    const availableRoles = this.roles();
    const defaultRole = availableRoles.find(r => /employee|team|staff|user/i.test(r.roleName)) || availableRoles[0];

    this.memberForm.reset({
      fullName: '',
      designation: '',
      department: 'Engineering',
      email: '',
      mobile: '',
      bio: '',
      linkedInUrl: '',
      githubUrl: '',
      enableLoginAccess: true,
      roleId: defaultRole ? defaultRole.id : null,
      password: '',
      isActive: true
    });
    this.isModalOpen.set(true);
  }

  openEditModal(m: TeamMemberDto): void {
    this.isEditMode.set(true);
    this.currentMemberId.set(m.id);
    this.showPassword.set(false);

    this.memberForm.reset({
      fullName: m.fullName,
      designation: m.designation,
      department: m.department || 'Engineering',
      email: m.email || '',
      mobile: m.mobile || '',
      bio: m.bio || '',
      linkedInUrl: m.linkedInUrl || '',
      githubUrl: m.githubUrl || '',
      enableLoginAccess: m.hasLoginAccess ?? !!m.userId,
      roleId: m.roleId || null,
      password: '',
      isActive: m.isActive
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  saveMember(): void {
    if (this.memberForm.invalid) {
      this.memberForm.markAllAsTouched();
      this.toastService.error('Full Name and Designation are required.', 'Form Incomplete');
      return;
    }

    this.modalSubmitting.set(true);
    const val = this.memberForm.value;

    const dto: SaveTeamMemberDto = {
      id: this.currentMemberId(),
      fullName: val.fullName.trim(),
      designation: val.designation.trim(),
      department: val.department?.trim(),
      email: val.email?.trim(),
      mobile: val.mobile?.trim(),
      bio: val.bio?.trim(),
      linkedInUrl: val.linkedInUrl?.trim(),
      githubUrl: val.githubUrl?.trim(),
      enableLoginAccess: val.enableLoginAccess,
      roleId: val.roleId ? Number(val.roleId) : undefined,
      password: val.password?.trim() || undefined,
      isActive: val.isActive
    };

    this.adminDataService.saveTeamMember(dto).subscribe({
      next: (res) => {
        this.modalSubmitting.set(false);
        if (res.success) {
          this.toastService.success(
            this.isEditMode()
              ? `Team member "${val.fullName}" updated & login credentials synced.`
              : `Team member "${val.fullName}" added & login account created in User directory.`,
            'Saved to Database'
          );
          this.closeModal();
          this.loadTeam();
        } else {
          this.toastService.error(res.message || 'Failed to save member.', 'Error');
        }
      },
      error: () => {
        this.modalSubmitting.set(false);
        this.toastService.error('Error saving team member.', 'Error');
      }
    });
  }

  async deleteMember(m: TeamMemberDto): Promise<void> {
    const confirmed = await this.confirmService.danger(
      'Delete Team Member',
      `Are you sure you want to remove "${m.fullName}" from the team directory?`
    );

    if (confirmed) {
      this.adminDataService.deleteTeamMember(m.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.success('Team member removed from database.', 'Deleted');
            this.loadTeam();
          } else {
            this.toastService.error(res.message || 'Failed to delete member', 'Error');
          }
        },
        error: () => {
          this.toastService.error('Error deleting team member', 'Error');
        }
      });
    }
  }
}
