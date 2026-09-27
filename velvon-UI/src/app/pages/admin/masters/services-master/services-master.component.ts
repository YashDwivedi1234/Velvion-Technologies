import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { ToastService, ConfirmService, IconComponent, ToggleComponent } from '../../../../shared';
import { ServiceMasterDto, SaveServiceMasterDto } from '../../../../core/models/api.models';

@Component({
  selector: 'app-services-master',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IconComponent, ToggleComponent],
  templateUrl: './services-master.component.html',
  styleUrls: ['./services-master.component.css']
})
export class ServicesMasterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private adminDataService = inject(AdminDataService);
  private toastService = inject(ToastService);
  private confirmService = inject(ConfirmService);

  loading = signal<boolean>(false);
  services = signal<ServiceMasterDto[]>([]);
  isModalOpen = signal<boolean>(false);
  isEditMode = signal<boolean>(false);
  currentServiceId = signal<number>(0);
  modalSubmitting = signal<boolean>(false);
  serviceForm!: FormGroup;

  ngOnInit(): void {
    this.initForm();
    this.loadServices();
  }

  private initForm(): void {
    this.serviceForm = this.fb.group({
      serviceName: ['', [Validators.required, Validators.maxLength(100)]],
      description: [''],
      iconUrl: ['layers'],
      isActive: [true]
    });
  }

  loadServices(): void {
    this.loading.set(true);
    this.adminDataService.getServices().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.services.set(res.data);
        } else {
          this.toastService.error(res.message || 'Failed to load services', 'Database Error');
        }
      },
      error: () => {
        this.loading.set(false);
        this.toastService.error('Error connecting to Services API', 'Network Error');
      }
    });
  }

  openAddModal(): void {
    this.isEditMode.set(false);
    this.currentServiceId.set(0);
    this.serviceForm.reset({
      serviceName: '',
      description: '',
      iconUrl: 'layers',
      isActive: true
    });
    this.isModalOpen.set(true);
  }

  openEditModal(service: ServiceMasterDto): void {
    this.isEditMode.set(true);
    this.currentServiceId.set(service.id);
    this.serviceForm.reset({
      serviceName: service.serviceName,
      description: service.description || '',
      iconUrl: service.iconUrl || 'layers',
      isActive: service.isActive
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  saveService(): void {
    if (this.serviceForm.invalid) {
      this.serviceForm.markAllAsTouched();
      this.toastService.error('Service Name is required.', 'Form Incomplete');
      return;
    }

    this.modalSubmitting.set(true);
    const val = this.serviceForm.value;

    const dto: SaveServiceMasterDto = {
      id: this.currentServiceId(),
      serviceName: val.serviceName.trim(),
      description: val.description?.trim(),
      iconUrl: val.iconUrl?.trim(),
      isActive: val.isActive
    };

    this.adminDataService.saveService(dto).subscribe({
      next: (res) => {
        this.modalSubmitting.set(false);
        if (res.success) {
          this.toastService.success(
            this.isEditMode() ? `Service "${val.serviceName}" updated in database.` : `Service "${val.serviceName}" created in database.`,
            'Saved to Database'
          );
          this.closeModal();
          this.loadServices();
        } else {
          this.toastService.error(res.message || 'Failed to save service.', 'Error');
        }
      },
      error: () => {
        this.modalSubmitting.set(false);
        this.toastService.error('Error saving service to database.', 'Error');
      }
    });
  }

  async deleteService(service: ServiceMasterDto): Promise<void> {
    const confirmed = await this.confirmService.danger(
      'Delete Service Item',
      `Are you sure you want to delete service "${service.serviceName}"?`
    );

    if (confirmed) {
      this.adminDataService.deleteService(service.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.success(`Service "${service.serviceName}" deleted from database.`, 'Deleted');
            this.loadServices();
          } else {
            this.toastService.error(res.message || 'Failed to delete service', 'Error');
          }
        },
        error: () => {
          this.toastService.error('Network error deleting service', 'Error');
        }
      });
    }
  }
}
