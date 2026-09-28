import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminDataService } from '../../../core/services/admin-data.service';
import { ToastService, ConfirmService, IconComponent, PaginationComponent } from '../../../shared';
import { ContactInquiryDto } from '../../../core/models/api.models';

@Component({
  selector: 'app-inquiries',
  standalone: true,
  imports: [CommonModule, IconComponent, PaginationComponent],
  templateUrl: './inquiries.component.html',
  styleUrls: ['./inquiries.component.css']
})
export class InquiriesComponent implements OnInit {
  private adminDataService = inject(AdminDataService);
  private toastService = inject(ToastService);
  private confirmService = inject(ConfirmService);

  loading = signal<boolean>(false);
  inquiries = signal<ContactInquiryDto[]>([]);
  selectedInquiry = signal<ContactInquiryDto | null>(null);

  // Sorting state
  sortColumn = signal<string>('createdAt');
  sortDirection = signal<'asc' | 'desc'>('desc');

  // Pagination
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);

  sortedInquiries = computed(() => {
    let list = this.inquiries();
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

  paginatedInquiries = computed(() => {
    const list = this.sortedInquiries();
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

  ngOnInit(): void {
    this.loadInquiries();
  }

  loadInquiries(): void {
    this.loading.set(true);
    this.adminDataService.getInquiries().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.inquiries.set(res.data);
        }
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  viewInquiry(inq: ContactInquiryDto): void {
    this.selectedInquiry.set(inq);
  }

  closeViewModal(): void {
    this.selectedInquiry.set(null);
  }

  async deleteInquiry(inq: ContactInquiryDto): Promise<void> {
    const confirmed = await this.confirmService.danger(
      'Delete Inquiry',
      `Are you sure you want to delete inquiry from "${inq.fullName}"?`
    );

    if (confirmed) {
      this.adminDataService.deleteInquiry(inq.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.success('Inquiry deleted from database.', 'Deleted');
            this.loadInquiries();
            if (this.selectedInquiry()?.id === inq.id) {
              this.closeViewModal();
            }
          }
        },
        error: () => {
          this.toastService.error('Error deleting inquiry', 'Error');
        }
      });
    }
  }
}
