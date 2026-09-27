import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminDataService } from '../../../core/services/admin-data.service';
import { IconComponent } from '../../../shared';
import { AuditLogDto } from '../../../core/models/api.models';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [CommonModule, IconComponent],
  templateUrl: './audit-logs.component.html',
  styleUrls: ['./audit-logs.component.css']
})
export class AuditLogsComponent implements OnInit {
  private adminDataService = inject(AdminDataService);

  loading = signal<boolean>(false);
  logs = signal<AuditLogDto[]>([]);

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(): void {
    this.loading.set(true);
    this.adminDataService.getAuditLogs(100).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.logs.set(res.data);
        }
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }
}
