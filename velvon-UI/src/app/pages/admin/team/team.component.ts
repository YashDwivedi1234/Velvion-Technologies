import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminDataService } from '../../../core/services/admin-data.service';
import { ToastService, ConfirmService, IconComponent, ToggleComponent } from '../../../shared';
import { TeamMemberDto, SaveTeamMemberDto } from '../../../core/models/api.models';

@Component({
  selector: 'app-team',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IconComponent, ToggleComponent],
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
  isModalOpen = signal<boolean>(false);
  isEditMode = signal<boolean>(false);
  currentMemberId = signal<number>(0);
  modalSubmitting = signal<boolean>(false);
  memberForm!: FormGroup;

  ngOnInit(): void {
    this.initForm();
    this.loadTeam();
  }

  private initForm(): void {
    this.memberForm = this.fb.group({
      fullName: ['', [Validators.required]],
      designation: ['', [Validators.required]],
      department: ['Engineering'],
      email: ['', [Validators.email]],
      bio: [''],
      linkedInUrl: [''],
      githubUrl: [''],
      isActive: [true]
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
    this.memberForm.reset({
      fullName: '',
      designation: '',
      department: 'Engineering',
      email: '',
      bio: '',
      linkedInUrl: '',
      githubUrl: '',
      isActive: true
    });
    this.isModalOpen.set(true);
  }

  openEditModal(m: TeamMemberDto): void {
    this.isEditMode.set(true);
    this.currentMemberId.set(m.id);
    this.memberForm.reset({
      fullName: m.fullName,
      designation: m.designation,
      department: m.department || 'Engineering',
      email: m.email || '',
      bio: m.bio || '',
      linkedInUrl: m.linkedInUrl || '',
      githubUrl: m.githubUrl || '',
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
      bio: val.bio?.trim(),
      linkedInUrl: val.linkedInUrl?.trim(),
      githubUrl: val.githubUrl?.trim(),
      isActive: val.isActive
    };

    this.adminDataService.saveTeamMember(dto).subscribe({
      next: (res) => {
        this.modalSubmitting.set(false);
        if (res.success) {
          this.toastService.success(
            this.isEditMode() ? `Team member "${val.fullName}" updated.` : `Team member "${val.fullName}" added.`,
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
