import { Component, Input, Output, EventEmitter, forwardRef, ElementRef, HostListener, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormsModule } from '@angular/forms';
import { IconComponent } from '../icon/icon.component';

interface CalendarDay {
  date: Date;
  dayNumber: number;
  isCurrentMonth: boolean;
  isToday: boolean;
  isSelected: boolean;
  isDisabled: boolean;
}

@Component({
  selector: 'app-date-picker',
  standalone: true,
  imports: [CommonModule, FormsModule, IconComponent],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DatePickerComponent),
      multi: true
    }
  ],
  templateUrl: './date-picker.component.html',
  styleUrls: ['./date-picker.component.css']
})
export class DatePickerComponent implements ControlValueAccessor {
  @Input() placeholder: string = 'Select date...';
  @Input() minDate?: Date | string;
  @Input() maxDate?: Date | string;
  @Input() disabled: boolean = false;
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Input() showPresets: boolean = true;

  @Output() dateChange = new EventEmitter<string>();

  isOpen = signal<boolean>(false);
  selectedDate = signal<Date | null>(null);
  currentViewDate = signal<Date>(new Date());

  weekDays = ['Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa', 'Su'];
  months = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];

  private onChange: (val: any) => void = () => {};
  private onTouched: () => void = () => {};

  constructor(private elementRef: ElementRef) {}

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.isOpen.set(false);
    }
  }

  formattedDate = computed(() => {
    const d = this.selectedDate();
    if (!d) return '';
    return this.formatDisplayDate(d);
  });

  currentMonthYear = computed(() => {
    const d = this.currentViewDate();
    return `${this.months[d.getMonth()]} ${d.getFullYear()}`;
  });

  calendarDays = computed(() => {
    const viewDate = this.currentViewDate();
    const year = viewDate.getFullYear();
    const month = viewDate.getMonth();
    const selected = this.selectedDate();
    const today = new Date();

    const firstDayOfMonth = new Date(year, month, 1);
    const lastDayOfMonth = new Date(year, month + 1, 0);

    // Monday as first day of week (0: Sun -> 6, 1: Mon -> 0, etc.)
    let firstDayIndex = firstDayOfMonth.getDay() - 1;
    if (firstDayIndex === -1) firstDayIndex = 6;

    const days: CalendarDay[] = [];

    // Previous month padding days
    const prevMonthLastDay = new Date(year, month, 0).getDate();
    for (let i = firstDayIndex - 1; i >= 0; i--) {
      const dayDate = new Date(year, month - 1, prevMonthLastDay - i);
      days.push({
        date: dayDate,
        dayNumber: prevMonthLastDay - i,
        isCurrentMonth: false,
        isToday: this.isSameDay(dayDate, today),
        isSelected: !!selected && this.isSameDay(dayDate, selected),
        isDisabled: this.isDateDisabled(dayDate)
      });
    }

    // Current month days
    for (let day = 1; day <= lastDayOfMonth.getDate(); day++) {
      const dayDate = new Date(year, month, day);
      days.push({
        date: dayDate,
        dayNumber: day,
        isCurrentMonth: true,
        isToday: this.isSameDay(dayDate, today),
        isSelected: !!selected && this.isSameDay(dayDate, selected),
        isDisabled: this.isDateDisabled(dayDate)
      });
    }

    // Next month padding days to make grid 35 or 42
    const totalCells = days.length > 35 ? 42 : 35;
    const remainingDays = totalCells - days.length;
    for (let day = 1; day <= remainingDays; day++) {
      const dayDate = new Date(year, month + 1, day);
      days.push({
        date: dayDate,
        dayNumber: day,
        isCurrentMonth: false,
        isToday: this.isSameDay(dayDate, today),
        isSelected: !!selected && this.isSameDay(dayDate, selected),
        isDisabled: this.isDateDisabled(dayDate)
      });
    }

    return days;
  });

  toggleOpen(): void {
    if (this.disabled) return;
    this.isOpen.update(o => !o);
    if (this.isOpen() && this.selectedDate()) {
      this.currentViewDate.set(new Date(this.selectedDate()!));
    }
    this.onTouched();
  }

  prevMonth(event: Event): void {
    event.stopPropagation();
    const curr = this.currentViewDate();
    this.currentViewDate.set(new Date(curr.getFullYear(), curr.getMonth() - 1, 1));
  }

  nextMonth(event: Event): void {
    event.stopPropagation();
    const curr = this.currentViewDate();
    this.currentViewDate.set(new Date(curr.getFullYear(), curr.getMonth() + 1, 1));
  }

  selectDay(day: CalendarDay, event: Event): void {
    event.stopPropagation();
    if (day.isDisabled) return;

    this.selectedDate.set(day.date);
    const isoString = this.formatIsoDate(day.date);
    this.onChange(isoString);
    this.dateChange.emit(isoString);
    this.isOpen.set(false);
  }

  selectPreset(preset: 'today' | 'yesterday' | 'weekAgo' | 'monthAgo', event: Event): void {
    event.stopPropagation();
    const d = new Date();
    if (preset === 'yesterday') {
      d.setDate(d.getDate() - 1);
    } else if (preset === 'weekAgo') {
      d.setDate(d.getDate() - 7);
    } else if (preset === 'monthAgo') {
      d.setMonth(d.getMonth() - 1);
    }
    this.selectedDate.set(d);
    this.currentViewDate.set(new Date(d));
    const iso = this.formatIsoDate(d);
    this.onChange(iso);
    this.dateChange.emit(iso);
    this.isOpen.set(false);
  }

  clear(event: Event): void {
    event.stopPropagation();
    if (this.disabled) return;
    this.selectedDate.set(null);
    this.onChange(null);
    this.dateChange.emit('');
  }

  private isSameDay(d1: Date, d2: Date): boolean {
    return d1.getFullYear() === d2.getFullYear() &&
           d1.getMonth() === d2.getMonth() &&
           d1.getDate() === d2.getDate();
  }

  private isDateDisabled(date: Date): boolean {
    if (this.minDate) {
      const min = new Date(this.minDate);
      min.setHours(0, 0, 0, 0);
      if (date < min) return true;
    }
    if (this.maxDate) {
      const max = new Date(this.maxDate);
      max.setHours(23, 59, 59, 999);
      if (date > max) return true;
    }
    return false;
  }

  private formatIsoDate(d: Date): string {
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private formatDisplayDate(d: Date): string {
    const day = String(d.getDate()).padStart(2, '0');
    const monthName = this.months[d.getMonth()].substring(0, 3);
    const year = d.getFullYear();
    return `${day} ${monthName} ${year}`;
  }

  // ControlValueAccessor
  writeValue(value: any): void {
    if (!value) {
      this.selectedDate.set(null);
    } else {
      const d = new Date(value);
      if (!isNaN(d.getTime())) {
        this.selectedDate.set(d);
        this.currentViewDate.set(new Date(d));
      } else {
        this.selectedDate.set(null);
      }
    }
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }
}
