import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminDataService } from '../../../core/services/admin-data.service';
import { IconComponent, PaginationComponent } from '../../../shared';
import { AuditLogDto } from '../../../core/models/api.models';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [CommonModule, IconComponent, PaginationComponent],
  templateUrl: './audit-logs.component.html',
  styleUrls: ['./audit-logs.component.css']
})
export class AuditLogsComponent implements OnInit {
  private adminDataService = inject(AdminDataService);

  loading = signal<boolean>(false);
  logs = signal<AuditLogDto[]>([]);

  // Sorting state
  sortColumn = signal<string>('createdAt');
  sortDirection = signal<'asc' | 'desc'>('desc');

  // Pagination
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);

  sortedLogs = computed(() => {
    let list = this.logs();
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

  paginatedLogs = computed(() => {
    const list = this.sortedLogs();
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
