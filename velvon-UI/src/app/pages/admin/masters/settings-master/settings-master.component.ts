import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { ToastService, ConfirmService, IconComponent, ToggleComponent } from '../../../../shared';
import { SettingDto, SaveSettingDto } from '../../../../core/models/api.models';

@Component({
  selector: 'app-settings-master',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IconComponent, ToggleComponent],
  templateUrl: './settings-master.component.html',
  styleUrls: ['./settings-master.component.css']
})
export class SettingsMasterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private adminDataService = inject(AdminDataService);
  private toastService = inject(ToastService);
  private confirmService = inject(ConfirmService);

  loading = signal<boolean>(false);
  settings = signal<SettingDto[]>([]);
  isModalOpen = signal<boolean>(false);
  isEditMode = signal<boolean>(false);
  currentSettingId = signal<number>(0);
  modalSubmitting = signal<boolean>(false);
  settingForm!: FormGroup;

  ngOnInit(): void {
    this.initForm();
    this.loadSettings();
  }

  private initForm(): void {
    this.settingForm = this.fb.group({
      settingKey: ['', [Validators.required, Validators.maxLength(100)]],
      settingValue: ['', [Validators.required]],
      description: [''],
      isActive: [true]
    });
  }

  loadSettings(): void {
    this.loading.set(true);
    this.adminDataService.getSettings().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.settings.set(res.data);
        } else {
          this.toastService.error(res.message || 'Failed to load settings', 'Database Error');
        }
      },
      error: () => {
        this.loading.set(false);
        this.toastService.error('Error connecting to Settings API', 'Network Error');
      }
    });
  }

  openAddModal(): void {
    this.isEditMode.set(false);
    this.currentSettingId.set(0);
    this.settingForm.reset({
      settingKey: '',
      settingValue: '',
      description: '',
      isActive: true
    });
    this.isModalOpen.set(true);
  }

  openEditModal(s: SettingDto): void {
    this.isEditMode.set(true);
    this.currentSettingId.set(s.id);
    this.settingForm.reset({
      settingKey: s.settingKey,
      settingValue: s.settingValue,
      description: s.description || '',
      isActive: s.isActive
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  saveSetting(): void {
    if (this.settingForm.invalid) {
      this.settingForm.markAllAsTouched();
      this.toastService.error('Key and Value are both required.', 'Form Incomplete');
      return;
    }

    this.modalSubmitting.set(true);
    const val = this.settingForm.value;

    const dto: SaveSettingDto = {
      id: this.currentSettingId(),
      settingKey: val.settingKey.trim(),
      settingValue: val.settingValue.trim(),
      description: val.description?.trim(),
      isActive: val.isActive
    };

    this.adminDataService.saveSetting(dto).subscribe({
      next: (res) => {
        this.modalSubmitting.set(false);
        if (res.success) {
          this.toastService.success(
            this.isEditMode() ? `Setting "${val.settingKey}" updated in database.` : `Setting "${val.settingKey}" saved in database.`,
            'Saved to Database'
          );
          this.closeModal();
          this.loadSettings();
        } else {
          this.toastService.error(res.message || 'Failed to save setting.', 'Error');
        }
      },
      error: () => {
        this.modalSubmitting.set(false);
        this.toastService.error('Error saving setting to database.', 'Error');
      }
    });
  }

  async deleteSetting(s: SettingDto): Promise<void> {
    const confirmed = await this.confirmService.danger(
      'Delete Setting Record',
      `Are you sure you want to delete setting key "${s.settingKey}"?`
    );

    if (confirmed) {
      this.adminDataService.deleteSetting(s.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.success(`Setting "${s.settingKey}" removed from database.`, 'Deleted');
            this.loadSettings();
          } else {
            this.toastService.error(res.message || 'Failed to delete setting', 'Error');
          }
        },
        error: () => {
          this.toastService.error('Network error deleting setting', 'Error');
        }
      });
    }
  }
}
