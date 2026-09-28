import { Component, Input, Output, EventEmitter, computed, signal, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IconComponent } from '../icon/icon.component';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule, FormsModule, IconComponent],
  templateUrl: './pagination.component.html',
  styleUrls: ['./pagination.component.css']
})
export class PaginationComponent implements OnChanges {
  @Input() totalItems: number = 0;
  @Input() pageSize: number = 10;
  @Input() currentPage: number = 1;
  @Input() pageSizeOptions: number[] = [5, 10, 20, 50];
  @Input() showPageSize: boolean = true;
  @Input() showTotalInfo: boolean = true;

  @Output() pageChange = new EventEmitter<number>();
  @Output() pageSizeChange = new EventEmitter<number>();

  totalItemsSig = signal<number>(0);
  pageSizeSig = signal<number>(10);
  currentPageSig = signal<number>(1);

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['totalItems']) {
      this.totalItemsSig.set(this.totalItems || 0);
    }
    if (changes['pageSize']) {
      this.pageSizeSig.set(this.pageSize || 10);
    }
    if (changes['currentPage']) {
      this.currentPageSig.set(this.currentPage || 1);
    }
  }

  totalPages = computed(() => {
    const total = this.totalItemsSig();
    const size = this.pageSizeSig();
    return Math.max(1, Math.ceil(total / size));
  });

  startItem = computed(() => {
    const total = this.totalItemsSig();
    if (total === 0) return 0;
    const page = this.currentPageSig();
    const size = this.pageSizeSig();
    return (page - 1) * size + 1;
  });

  endItem = computed(() => {
    const total = this.totalItemsSig();
    const page = this.currentPageSig();
    const size = this.pageSizeSig();
    return Math.min(page * size, total);
  });

  pageNumbers = computed(() => {
    const total = this.totalPages();
    const current = this.currentPageSig();
    const delta = 2;
    const range: (number | string)[] = [];

    for (let i = Math.max(2, current - delta); i <= Math.min(total - 1, current + delta); i++) {
      range.push(i);
    }

    if (current - delta > 2) {
      range.unshift('...');
    }
    if (current + delta < total - 1) {
      range.push('...');
    }

    range.unshift(1);
    if (total > 1) {
      range.push(total);
    }

    return range;
  });

  setPage(page: number | string): void {
    if (typeof page !== 'number') return;
    if (page < 1 || page > this.totalPages() || page === this.currentPageSig()) return;
    this.currentPageSig.set(page);
    this.pageChange.emit(page);
  }

  prevPage(): void {
    if (this.currentPageSig() > 1) {
      this.setPage(this.currentPageSig() - 1);
    }
  }

  nextPage(): void {
    if (this.currentPageSig() < this.totalPages()) {
      this.setPage(this.currentPageSig() + 1);
    }
  }

  firstPage(): void {
    this.setPage(1);
  }

  lastPage(): void {
    this.setPage(this.totalPages());
  }

  onPageSizeChange(event: Event): void {
    const newSize = Number((event.target as HTMLSelectElement).value);
    this.pageSizeSig.set(newSize);
    this.currentPageSig.set(1);
    this.pageSizeChange.emit(newSize);
    this.pageChange.emit(1);
  }
}
