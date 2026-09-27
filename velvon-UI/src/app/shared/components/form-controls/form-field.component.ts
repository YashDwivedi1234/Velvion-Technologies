import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../icon/icon.component';

@Component({
  selector: 'app-form-field',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    <div class="form-field-group">
      @if (label) {
        <label [attr.for]="forId" class="field-label">
          <span>{{ label }}</span>
          @if (required) {
            <span class="required-star" title="Required field">*</span>
          }
        </label>
      }

      <div class="field-content">
        <ng-content></ng-content>
      </div>

      @if (error) {
        <div class="field-error" role="alert">
          <app-icon name="alert-triangle" [size]="14"></app-icon>
          <span>{{ error }}</span>
        </div>
      } @else if (hint) {
        <div class="field-hint">{{ hint }}</div>
      }
    </div>
  `,
  styles: [`
    :host {
      display: block;
      width: 100%;
    }
    .form-field-group {
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
      margin-bottom: 1rem;
      width: 100%;
    }
    .field-label {
      font-size: 0.8125rem;
      font-weight: 600;
      color: var(--text-main);
      display: flex;
      align-items: center;
      gap: 0.25rem;
    }
    .required-star {
      color: var(--danger-500);
      font-weight: 700;
    }
    .field-content {
      position: relative;
      width: 100%;
    }
    .field-error {
      font-size: 0.775rem;
      color: var(--danger-text);
      display: flex;
      align-items: center;
      gap: 0.35rem;
      animation: slideDown 0.15s ease-out;
      font-weight: 500;
    }
    .field-hint {
      font-size: 0.775rem;
      color: var(--text-muted);
    }
  `]
})
export class FormFieldComponent {
  @Input() label: string = '';
  @Input() forId: string = '';
  @Input() required: boolean = false;
  @Input() error: string | null = null;
  @Input() hint: string = '';
}
