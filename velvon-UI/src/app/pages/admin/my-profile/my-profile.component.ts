import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { AdminDataService } from '../../../core/services/admin-data.service';
import { ToastService, IconComponent } from '../../../shared';
import { UserDto, SaveUserDto } from '../../../core/models/api.models';

@Component({
  selector: 'app-my-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IconComponent],
  templateUrl: './my-profile.component.html',
  styleUrls: ['./my-profile.component.css']
})
export class MyProfileComponent implements OnInit {
  public authService = inject(AuthService);
  private adminDataService = inject(AdminDataService);
  private toastService = inject(ToastService);
  private fb = inject(FormBuilder);

  profileForm!: FormGroup;
  passwordForm!: FormGroup;

  loading = signal<boolean>(false);
  saving = signal<boolean>(false);
  passwordSaving = signal<boolean>(false);
  activeTab = signal<'basic' | 'security'>('basic');

  userDetails = signal<UserDto | null>(null);

  ngOnInit(): void {
    this.initForms();
    this.loadUserProfile();
  }

  private initForms(): void {
    const user = this.authService.currentUser();
    this.profileForm = this.fb.group({
      fullName: [user?.fullName || '', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      email: [user?.email || '', [Validators.required, Validators.email]],
      mobile: ['', [Validators.pattern('^[0-9+\\- ]{7,15}$')]],
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    });
  }

  loadUserProfile(): void {
    const user = this.authService.currentUser();
    if (!user || !user.userId) {
      return;
    }

    this.loading.set(true);
    this.adminDataService.getUserById(user.userId).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.userDetails.set(res.data);
          this.profileForm.patchValue({
            fullName: res.data.fullName,
            email: res.data.email,
            mobile: res.data.mobile || ''
          });
        }
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  saveProfile(): void {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    const currentAuth = this.authService.currentUser();
    if (!currentAuth) return;

    this.saving.set(true);
    const formVal = this.profileForm.value;

    const dto: SaveUserDto = {
      id: currentAuth.userId,
      roleId: currentAuth.roleId,
      fullName: formVal.fullName.trim(),
      email: formVal.email.trim(),
      mobile: formVal.mobile ? formVal.mobile.trim() : '',
      isActive: true,
      updatedBy: currentAuth.userId
    };

    this.adminDataService.saveUser(dto).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success && res.data) {
          // Update currentUser signal in AuthService
          const updatedAuth = {
            ...currentAuth,
            fullName: res.data.fullName,
            email: res.data.email
          };
          this.authService.currentUser.set(updatedAuth);
          if (typeof window !== 'undefined' && window.localStorage) {
            localStorage.setItem('velvion_auth_user', JSON.stringify(updatedAuth));
          }
          this.userDetails.set(res.data);
          this.toastService.success('Profile basic information updated successfully!', 'Profile Updated');
        } else {
          this.toastService.error(res.message || 'Failed to update profile.', 'Error');
        }
      },
      error: () => {
        this.saving.set(false);
        this.toastService.error('Network error while saving profile.', 'Error');
      }
    });
  }

  changePassword(): void {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    const currentAuth = this.authService.currentUser();
    if (!currentAuth) return;

    const formVal = this.passwordForm.value;
    if (formVal.newPassword !== formVal.confirmPassword) {
      this.toastService.error('New password and confirm password do not match.', 'Validation Error');
      return;
    }

    this.passwordSaving.set(true);

    const dto: SaveUserDto = {
      id: currentAuth.userId,
      roleId: currentAuth.roleId,
      fullName: currentAuth.fullName,
      email: currentAuth.email,
      password: formVal.newPassword,
      isActive: true,
      updatedBy: currentAuth.userId
    };

    this.adminDataService.saveUser(dto).subscribe({
      next: (res) => {
        this.passwordSaving.set(false);
        if (res.success) {
          this.passwordForm.reset();
          this.toastService.success('Password changed successfully.', 'Security Updated');
        } else {
          this.toastService.error(res.message || 'Failed to update password.', 'Error');
        }
      },
      error: () => {
        this.passwordSaving.set(false);
        this.toastService.error('Network error changing password.', 'Error');
      }
    });
  }
}
