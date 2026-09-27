import { Component, Input, Output, EventEmitter, forwardRef, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-toggle',
  standalone: true,
  imports: [CommonModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ToggleComponent),
      multi: true
    }
  ],
  template: `
    <label class="toggle-container" [class.is-disabled]="disabled">
      <input
        type="checkbox"
        class="toggle-input"
        [checked]="checked()"
        [disabled]="disabled"
        (change)="onToggle($event)"
      />
      <div class="toggle-track" [class.checked]="checked()">
        <div class="toggle-thumb"></div>
      </div>
      @if (label) {
        <span class="toggle-label">{{ label }}</span>
      }
    </label>
  `,
  styles: [`
    .toggle-container {
      display: inline-flex;
      align-items: center;
      gap: 0.625rem;
      cursor: pointer;
      user-select: none;
    }
    .toggle-container.is-disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
    .toggle-input {
      position: absolute;
      opacity: 0;
      width: 0;
      height: 0;
      margin: 0;
    }
    .toggle-track {
      position: relative;
      width: 44px;
      height: 24px;
      background: var(--bg-surface-elevated);
      border: 1px solid var(--border-default);
      border-radius: var(--radius-full);
      transition: all var(--transition-fast);
      flex-shrink: 0;
    }
    .toggle-track.checked {
      background: var(--primary-base);
      border-color: var(--primary-base);
      box-shadow: 0 0 10px rgba(99, 102, 241, 0.4);
    }
    .toggle-thumb {
      position: absolute;
      top: 2px;
      left: 2px;
      width: 18px;
      height: 18px;
      background: #ffffff;
      border-radius: 50%;
      transition: transform var(--transition-fast);
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.3);
    }
    .toggle-track.checked .toggle-thumb {
      transform: translateX(20px);
    }
    .toggle-label {
      font-size: 0.875rem;
      color: var(--text-main);
      font-weight: 500;
    }
  `]
})
export class ToggleComponent implements ControlValueAccessor {
  @Input() label: string = '';
  @Input() disabled: boolean = false;
  @Output() change = new EventEmitter<boolean>();

  checked = signal<boolean>(false);

  private onChange: (val: boolean) => void = () => {};
  private onTouched: () => void = () => {};

  onToggle(event: Event): void {
    if (this.disabled) return;
    const isChecked = (event.target as HTMLInputElement).checked;
    this.checked.set(isChecked);
    this.onChange(isChecked);
    this.change.emit(isChecked);
    this.onTouched();
  }

  writeValue(val: boolean): void {
    this.checked.set(!!val);
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
