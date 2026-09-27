import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminDataService } from '../../../core/services/admin-data.service';
import { ToastService, ConfirmService, IconComponent } from '../../../shared';
import { ContactInquiryDto } from '../../../core/models/api.models';

@Component({
  selector: 'app-inquiries',
  standalone: true,
  imports: [CommonModule, IconComponent],
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
