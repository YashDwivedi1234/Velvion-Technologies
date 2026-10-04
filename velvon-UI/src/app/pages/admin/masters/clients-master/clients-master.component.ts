import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { ToastService, ConfirmService, IconComponent, ToggleComponent, PaginationComponent } from '../../../../shared';
import { ClientDto, SaveClientDto } from '../../../../core/models/api.models';

@Component({
  selector: 'app-clients-master',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IconComponent, ToggleComponent, PaginationComponent],
  templateUrl: './clients-master.component.html',
  styleUrls: ['./clients-master.component.css']
})
export class ClientsMasterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private adminDataService = inject(AdminDataService);
  private toastService = inject(ToastService);
  private confirmService = inject(ConfirmService);

  loading = signal<boolean>(false);
  clients = signal<ClientDto[]>([]);
  searchQuery = signal<string>('');

  // Sorting state
  sortColumn = signal<string>('displayOrder');
  sortDirection = signal<'asc' | 'desc'>('asc');

  // Pagination
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);

  filteredClients = computed(() => {
    let list = this.clients();
    const query = this.searchQuery().toLowerCase().trim();
    if (query) {
      list = list.filter(c =>
        c.clientName.toLowerCase().includes(query) ||
        (c.industry && c.industry.toLowerCase().includes(query)) ||
        (c.partnerTier && c.partnerTier.toLowerCase().includes(query))
      );
    }
    return list;
  });

  sortedClients = computed(() => {
    let list = this.filteredClients();
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

  paginatedClients = computed(() => {
    const list = this.sortedClients();
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
  currentClientId = signal<number>(0);
  modalSubmitting = signal<boolean>(false);
  clientForm!: FormGroup;

  // View Modal state
  isViewModalOpen = signal<boolean>(false);
  viewingClient = signal<ClientDto | null>(null);

  openViewModal(c: ClientDto): void {
    this.viewingClient.set(c);
    this.isViewModalOpen.set(true);
  }

  closeViewModal(): void {
    this.isViewModalOpen.set(false);
    this.viewingClient.set(null);
  }

  ngOnInit(): void {
    this.initForm();
    this.loadClients();
  }

  private initForm(): void {
    this.clientForm = this.fb.group({
      clientName: ['', [Validators.required, Validators.maxLength(150)]],
      industry: [''],
      partnerTier: ['Enterprise Client'],
      websiteUrl: [''],
      logoUrl: [''],
      displayOrder: [0, [Validators.min(0)]],
      isActive: [true]
    });
  }

  loadClients(): void {
    this.loading.set(true);
    this.adminDataService.getClients().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.clients.set(res.data);
        } else {
          this.toastService.error(res.message || 'Failed to load clients', 'Database Error');
        }
      },
      error: () => {
        this.loading.set(false);
        this.toastService.error('Error connecting to Clients API', 'Network Error');
      }
    });
  }

  openAddModal(): void {
    this.isEditMode.set(false);
    this.currentClientId.set(0);
    this.clientForm.reset({
      clientName: '',
      industry: '',
      partnerTier: 'Enterprise Client',
      websiteUrl: '',
      logoUrl: '',
      displayOrder: this.clients().length + 1,
      isActive: true
    });
    this.isModalOpen.set(true);
  }

  openEditModal(client: ClientDto): void {
    this.isEditMode.set(true);
    this.currentClientId.set(client.id);
    this.clientForm.reset({
      clientName: client.clientName,
      industry: client.industry || '',
      partnerTier: client.partnerTier || 'Enterprise Client',
      websiteUrl: client.websiteUrl || '',
      logoUrl: client.logoUrl || '',
      displayOrder: client.displayOrder,
      isActive: client.isActive
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
  }

  saveClient(): void {
    if (this.clientForm.invalid) {
      this.clientForm.markAllAsTouched();
      this.toastService.error('Client Name is required.', 'Form Incomplete');
      return;
    }

    this.modalSubmitting.set(true);
    const val = this.clientForm.value;

    const dto: SaveClientDto = {
      id: this.currentClientId(),
      clientName: val.clientName.trim(),
      industry: val.industry?.trim(),
      partnerTier: val.partnerTier?.trim() || 'Enterprise Client',
      websiteUrl: val.websiteUrl?.trim(),
      logoUrl: val.logoUrl?.trim(),
      displayOrder: Number(val.displayOrder) || 0,
      isActive: val.isActive
    };

    this.adminDataService.saveClient(dto).subscribe({
      next: (res) => {
        this.modalSubmitting.set(false);
        if (res.success) {
          this.toastService.success(
            this.isEditMode() ? `Client "${val.clientName}" updated.` : `Client "${val.clientName}" created.`,
            'Saved to Database'
          );
          this.closeModal();
          this.loadClients();
        } else {
          this.toastService.error(res.message || 'Failed to save client.', 'Error');
        }
      },
      error: () => {
        this.modalSubmitting.set(false);
        this.toastService.error('Error saving client to database.', 'Error');
      }
    });
  }

  async deleteClient(client: ClientDto): Promise<void> {
    const confirmed = await this.confirmService.danger(
      'Delete Client / Partner',
      `Are you sure you want to delete client "${client.clientName}"?`
    );

    if (confirmed) {
      this.adminDataService.deleteClient(client.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.success(`Client "${client.clientName}" deleted.`, 'Deleted');
            this.loadClients();
          } else {
            this.toastService.error(res.message || 'Failed to delete client', 'Error');
          }
        },
        error: () => {
          this.toastService.error('Network error deleting client', 'Error');
        }
      });
    }
  }
}
