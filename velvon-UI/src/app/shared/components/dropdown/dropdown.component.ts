import { Component, Input, Output, EventEmitter, forwardRef, ElementRef, HostListener, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormsModule } from '@angular/forms';
import { DropdownOption } from '../../models/dropdown.model';
import { IconComponent } from '../icon/icon.component';

@Component({
  selector: 'app-dropdown',
  standalone: true,
  imports: [CommonModule, FormsModule, IconComponent],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DropdownComponent),
      multi: true
    }
  ],
  templateUrl: './dropdown.component.html',
  styleUrls: ['./dropdown.component.css']
})
export class DropdownComponent implements ControlValueAccessor {
  @Input() options: DropdownOption[] = [];
  @Input() placeholder: string = 'Select an option...';
  @Input() multiple: boolean = false;
  @Input() searchable: boolean = true;
  @Input() clearable: boolean = true;
  @Input() disabled: boolean = false;
  @Input() maxChips: number = 3;
  @Input() size: 'sm' | 'md' | 'lg' = 'md';

  @Output() selectionChange = new EventEmitter<any>();

  isOpen = signal<boolean>(false);
  searchTerm = signal<string>('');
  selectedValues = signal<any[]>([]);

  private onChange: (value: any) => void = () => {};
  private onTouched: () => void = () => {};

  constructor(private elementRef: ElementRef) {}

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.isOpen.set(false);
      this.searchTerm.set('');
    }
  }

  filteredOptions = computed(() => {
    const term = this.searchTerm().toLowerCase().trim();
    if (!term) return this.options;
    return this.options.filter(opt =>
      opt.label.toLowerCase().includes(term) ||
      (opt.description && opt.description.toLowerCase().includes(term))
    );
  });

  selectedOptions = computed(() => {
    const vals = this.selectedValues();
    return this.options.filter(opt => vals.includes(opt.value));
  });

  firstSelectedOption = computed(() => {
    const opts = this.selectedOptions();
    return opts.length > 0 ? opts[0] : null;
  });

  selectedSingleLabel = computed(() => {
    const vals = this.selectedValues();
    if (vals.length === 0) return '';
    const match = this.options.find(opt => opt.value === vals[0]);
    return match ? match.label : '';
  });

  toggleOpen(): void {
    if (this.disabled) return;
    this.isOpen.update(open => !open);
    if (!this.isOpen()) {
      this.searchTerm.set('');
    }
    this.onTouched();
  }

  selectOption(option: DropdownOption, event?: Event): void {
    if (event) event.stopPropagation();
    if (option.disabled) return;

    if (this.multiple) {
      const current = this.selectedValues();
      const index = current.indexOf(option.value);
      let updated: any[];
      if (index > -1) {
        updated = current.filter(v => v !== option.value);
      } else {
        updated = [...current, option.value];
      }
      this.selectedValues.set(updated);
      this.onChange(updated);
      this.selectionChange.emit(updated);
    } else {
      this.selectedValues.set([option.value]);
      this.onChange(option.value);
      this.selectionChange.emit(option.value);
      this.isOpen.set(false);
      this.searchTerm.set('');
    }
  }

  removeChip(value: any, event: Event): void {
    event.stopPropagation();
    if (this.disabled) return;
    const updated = this.selectedValues().filter(v => v !== value);
    this.selectedValues.set(updated);
    this.onChange(this.multiple ? updated : null);
    this.selectionChange.emit(this.multiple ? updated : null);
  }

  clearSelection(event: Event): void {
    event.stopPropagation();
    if (this.disabled) return;
    this.selectedValues.set([]);
    this.onChange(this.multiple ? [] : null);
    this.selectionChange.emit(this.multiple ? [] : null);
    this.searchTerm.set('');
  }

  isSelected(value: any): boolean {
    return this.selectedValues().includes(value);
  }

  // ControlValueAccessor methods
  writeValue(value: any): void {
    if (value === null || value === undefined) {
      this.selectedValues.set([]);
    } else if (Array.isArray(value)) {
      this.selectedValues.set(value);
    } else {
      this.selectedValues.set([value]);
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
