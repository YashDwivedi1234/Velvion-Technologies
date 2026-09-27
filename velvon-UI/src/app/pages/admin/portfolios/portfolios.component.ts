import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminDataService } from '../../../core/services/admin-data.service';
import { ToastService, ConfirmService, IconComponent, ToggleComponent } from '../../../shared';
import { PortfolioDto, SavePortfolioDto } from '../../../core/models/api.models';

@Component({
  selector: 'app-portfolios',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IconComponent, ToggleComponent],
  templateUrl: './portfolios.component.html',
  styleUrls: ['./portfolios.component.css']
})
export class PortfoliosComponent implements OnInit {
  private fb = inject(FormBuilder);
  private adminDataService = inject(AdminDataService);
  private toastService = inject(ToastService);
  private confirmService = inject(ConfirmService);

  loading = signal<boolean>(false);
  portfolios = signal<PortfolioDto[]>([]);
  isModalOpen = signal<boolean>(false);
  isEditMode = signal<boolean>(false);
  currentPortfolioId = signal<number>(0);
  modalSubmitting = signal<boolean>(false);
  portfolioForm!: FormGroup;

  ngOnInit(): void {
    this.initForm();
    this.loadPortfolios();
  }

  private initForm(): void {
    this.portfolioForm = this.fb.group({
      title: ['', [Validators.required]],
      clientName: [''],
      category: ['Enterprise Cloud & Web'],
      projectUrl: [''],
      technologies: ['C#, Angular 19, MySQL'],
      description: [''],
      isActive: [true]
    });
  }

  loadPortfolios(): void {
    this.loading.set(true);
    this.adminDataService.getPortfolios().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.portfolios.set(res.data);
        }
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  openAddModal(): void {
    this.isEditMode.set(false);
    this.currentPortfolioId.set(0);
    this.portfolioForm.reset({
      title: '',
      clientName: '',
      category: 'Enterprise Cloud & Web',
      projectUrl: '',
      technologies: 'Angular 19, .NET 10, MySQL',
      description: '',
      isActive: true
    });
    this.isModalOpen.set(true);
  }

  openEditModal(p: PortfolioDto): void {
    this.isEditMode.set(true);
    this.currentPortfolioId.set(p.id);
    this.portfolioForm.reset({
      title: p.title,
      clientName: p.clientName || '',
      category: p.category || 'General',
      projectUrl: p.projectUrl || '',
      technologies: p.technologies || '',
      description: p.description || '',
      isActive: p.isActive
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  savePortfolio(): void {
    if (this.portfolioForm.invalid) {
      this.portfolioForm.markAllAsTouched();
      this.toastService.error('Project Title is required.', 'Form Incomplete');
      return;
    }

    this.modalSubmitting.set(true);
    const val = this.portfolioForm.value;

    const dto: SavePortfolioDto = {
      id: this.currentPortfolioId(),
      title: val.title.trim(),
      clientName: val.clientName?.trim(),
      category: val.category?.trim(),
      projectUrl: val.projectUrl?.trim(),
      technologies: val.technologies?.trim(),
      description: val.description?.trim(),
      isActive: val.isActive
    };

    this.adminDataService.savePortfolio(dto).subscribe({
      next: (res) => {
        this.modalSubmitting.set(false);
        if (res.success) {
          this.toastService.success(
            this.isEditMode() ? `Project "${val.title}" updated in database.` : `Project "${val.title}" saved in database.`,
            'Saved'
          );
          this.closeModal();
          this.loadPortfolios();
        } else {
          this.toastService.error(res.message || 'Failed to save portfolio.', 'Error');
        }
      },
      error: () => {
        this.modalSubmitting.set(false);
        this.toastService.error('Error saving portfolio.', 'Error');
      }
    });
  }

  async deletePortfolio(p: PortfolioDto): Promise<void> {
    const confirmed = await this.confirmService.danger(
      'Delete Portfolio Item',
      `Are you sure you want to delete project "${p.title}"?`
    );

    if (confirmed) {
      this.adminDataService.deletePortfolio(p.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.success(`Project deleted from database.`, 'Deleted');
            this.loadPortfolios();
          } else {
            this.toastService.error(res.message || 'Failed to delete project', 'Error');
          }
        },
        error: () => {
          this.toastService.error('Error deleting portfolio', 'Error');
        }
      });
    }
  }
}
