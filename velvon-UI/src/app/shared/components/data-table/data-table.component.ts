import { Component, Input, Output, EventEmitter, computed, signal, ContentChild, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableColumn, TableAction, TableBulkAction } from '../../models/table.model';
import { IconComponent } from '../icon/icon.component';
import { SkeletonComponent } from '../loader/skeleton.component';

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [CommonModule, FormsModule, IconComponent, SkeletonComponent],
  templateUrl: './data-table.component.html',
  styleUrls: ['./data-table.component.css']
})
export class DataTableComponent<T extends Record<string, any> = any> {
  @Input() title: string = '';
  @Input() subtitle: string = '';
  @Input() columns: TableColumn<T>[] = [];
  @Input() set data(value: T[]) {
    this.dataSource.set(value || []);
    this.selectedRows.set([]);
  }
  @Input() loading: boolean = false;
  @Input() selectable: boolean = true;
  @Input() showSearch: boolean = true;
  @Input() showExport: boolean = true;
  @Input() showRefresh: boolean = true;
  @Input() actions: TableAction<T>[] = [];
  @Input() bulkActions: TableBulkAction<T>[] = [];
  @Input() emptyTitle: string = 'No records found';
  @Input() emptySubtitle: string = 'Try adjusting your search criteria or add new data';
  @Input() rowKey: string = 'id';

  @Output() rowClick = new EventEmitter<T>();
  @Output() actionClick = new EventEmitter<{ actionId: string; row: T }>();
  @Output() bulkActionClick = new EventEmitter<{ actionId: string; selectedRows: T[] }>();
  @Output() refresh = new EventEmitter<void>();
  @Output() addNew = new EventEmitter<void>();

  dataSource = signal<T[]>([]);
  searchTerm = signal<string>('');
  sortColumn = signal<string | null>(null);
  sortDirection = signal<'asc' | 'desc' | null>(null);
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);
  selectedRows = signal<T[]>([]);

  pageSizes = [5, 10, 20, 50];

  // Filtered and Sorted Data
  filteredData = computed(() => {
    let items = [...this.dataSource()];
    const term = this.searchTerm().toLowerCase().trim();

    // 1. Search Filter
    if (term) {
      items = items.filter(item => {
        return this.columns.some(col => {
          const val = item[col.key];
          if (val === null || val === undefined) return false;
          return String(val).toLowerCase().includes(term);
        });
      });
    }

    // 2. Sorting
    const sortCol = this.sortColumn();
    const sortDir = this.sortDirection();

    if (sortCol && sortDir) {
      items.sort((a, b) => {
        const valA = a[sortCol];
        const valB = b[sortCol];

        if (valA === valB) return 0;
        if (valA === null || valA === undefined) return 1;
        if (valB === null || valB === undefined) return -1;

        if (typeof valA === 'number' && typeof valB === 'number') {
          return sortDir === 'asc' ? valA - valB : valB - valA;
        }

        const strA = String(valA).toLowerCase();
        const strB = String(valB).toLowerCase();
        const cmp = strA.localeCompare(strB);
        return sortDir === 'asc' ? cmp : -cmp;
      });
    }

    return items;
  });

  // Paginated Data
  paginatedData = computed(() => {
    const items = this.filteredData();
    const start = (this.currentPage() - 1) * this.pageSize();
    return items.slice(start, start + this.pageSize());
  });

  totalPages = computed(() => {
    return Math.ceil(this.filteredData().length / this.pageSize()) || 1;
  });

  startRecord = computed(() => {
    if (this.filteredData().length === 0) return 0;
    return (this.currentPage() - 1) * this.pageSize() + 1;
  });

  endRecord = computed(() => {
    return Math.min(this.currentPage() * this.pageSize(), this.filteredData().length);
  });

  isAllSelected = computed(() => {
    const pageItems = this.paginatedData();
    if (pageItems.length === 0) return false;
    const selected = this.selectedRows();
    return pageItems.every(item => selected.some(s => s[this.rowKey] === item[this.rowKey]));
  });

  isSomeSelected = computed(() => {
    const pageItems = this.paginatedData();
    const selected = this.selectedRows();
    const countOnPage = pageItems.filter(item => selected.some(s => s[this.rowKey] === item[this.rowKey])).length;
    return countOnPage > 0 && countOnPage < pageItems.length;
  });

  handleSort(column: TableColumn<T>): void {
    if (!column.sortable) return;

    if (this.sortColumn() === column.key) {
      if (this.sortDirection() === 'asc') {
        this.sortDirection.set('desc');
      } else if (this.sortDirection() === 'desc') {
        this.sortColumn.set(null);
        this.sortDirection.set(null);
      }
    } else {
      this.sortColumn.set(column.key);
      this.sortDirection.set('asc');
    }
  }

  toggleSelectAll(): void {
    const pageItems = this.paginatedData();
    const currentSelected = [...this.selectedRows()];

    if (this.isAllSelected()) {
      // Unselect page items
      const pageKeys = new Set(pageItems.map(p => p[this.rowKey]));
      this.selectedRows.set(currentSelected.filter(item => !pageKeys.has(item[this.rowKey])));
    } else {
      // Add missing page items
      const existingKeys = new Set(currentSelected.map(s => s[this.rowKey]));
      const toAdd = pageItems.filter(p => !existingKeys.has(p[this.rowKey]));
      this.selectedRows.set([...currentSelected, ...toAdd]);
    }
  }

  toggleSelectRow(row: T, event: Event): void {
    event.stopPropagation();
    const current = [...this.selectedRows()];
    const index = current.findIndex(s => s[this.rowKey] === row[this.rowKey]);

    if (index > -1) {
      current.splice(index, 1);
    } else {
      current.push(row);
    }
    this.selectedRows.set(current);
  }

  isRowSelected(row: T): boolean {
    return this.selectedRows().some(s => s[this.rowKey] === row[this.rowKey]);
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
    }
  }

  changePageSize(event: Event): void {
    const size = Number((event.target as HTMLSelectElement).value);
    this.pageSize.set(size);
    this.currentPage.set(1);
  }

  onRowClick(row: T): void {
    this.rowClick.emit(row);
  }

  onActionClick(actionId: string, row: T, event: Event): void {
    event.stopPropagation();
    this.actionClick.emit({ actionId, row });
  }

  onBulkActionClick(actionId: string): void {
    this.bulkActionClick.emit({
      actionId,
      selectedRows: this.selectedRows()
    });
  }

  getBadgeType(column: TableColumn<T>, value: any): 'primary' | 'success' | 'warning' | 'danger' | 'info' | 'neutral' {
    if (column.badgeTypeMap && column.badgeTypeMap[value]) {
      return column.badgeTypeMap[value];
    }
    const valLower = String(value).toLowerCase();
    if (valLower === 'active' || valLower === 'completed' || valLower === 'published' || valLower === 'true') return 'success';
    if (valLower === 'inactive' || valLower === 'failed' || valLower === 'cancelled' || valLower === 'false') return 'danger';
    if (valLower === 'pending' || valLower === 'in progress' || valLower === 'warning') return 'warning';
    return 'primary';
  }

  exportCsv(): void {
    if (typeof document === 'undefined') return;
    const data = this.filteredData();
    if (data.length === 0) return;

    const headers = this.columns.map(c => `"${c.label}"`).join(',');
    const rows = data.map(item => {
      return this.columns.map(col => {
        const val = item[col.key];
        return `"${val !== null && val !== undefined ? String(val).replace(/"/g, '""') : ''}"`;
      }).join(',');
    });

    const csvContent = 'data:text/csv;charset=utf-8,' + [headers, ...rows].join('\n');
    const encodedUri = encodeURI(csvContent);
    const link = document.createElement('a');
    link.setAttribute('href', encodedUri);
    link.setAttribute('download', `${this.title ? this.title.toLowerCase().replace(/\s+/g, '_') : 'export'}_${Date.now()}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }
}
