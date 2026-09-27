import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent } from '../icon/icon.component';

@Component({
  selector: 'app-form-card',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    <div class="form-card card">
      @if (title) {
        <div class="form-card-header">
          <div class="title-wrap">
            @if (icon) {
              <div class="header-icon-badge">
                <app-icon [name]="icon" [size]="20"></app-icon>
              </div>
            }
            <div>
              <h3 class="form-title">{{ title }}</h3>
              @if (subtitle) {
                <p class="form-subtitle">{{ subtitle }}</p>
              }
            </div>
          </div>

          <div class="header-actions">
            <ng-content select="[slot=header-actions]"></ng-content>
          </div>
        </div>
      }

      <div class="form-card-body">
        <ng-content></ng-content>
      </div>

      @if (showFooter) {
        <div class="form-card-footer">
          <div class="footer-left">
            <ng-content select="[slot=footer-left]"></ng-content>
          </div>
          <div class="footer-buttons">
            @if (showCancel) {
              <button
                type="button"
                class="btn btn-secondary"
                [disabled]="submitting"
                (click)="cancel.emit()"
              >
                {{ cancelText }}
              </button>
            }
            <button
              type="button"
              class="btn btn-primary"
              [disabled]="submitting || submitDisabled"
              (click)="submit.emit()"
            >
              @if (submitting) {
                <app-icon name="loader" [size]="16" customClass="animate-spin"></app-icon>
                <span>Saving...</span>
              } @else {
                @if (submitIcon) {
                  <app-icon [name]="submitIcon" [size]="16"></app-icon>
                }
                <span>{{ submitText }}</span>
              }
            </button>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .form-card {
      padding: 0;
      overflow: hidden;
    }
    .form-card-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 1rem;
      padding: 1.25rem 1.5rem;
      border-bottom: 1px solid var(--border-subtle);
    }
    .title-wrap {
      display: flex;
      align-items: center;
      gap: 0.875rem;
    }
    .header-icon-badge {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 40px;
      height: 40px;
      border-radius: var(--radius-md);
      background: var(--primary-subtle);
      color: var(--primary-400);
      border: 1px solid rgba(99, 102, 241, 0.2);
    }
    .form-title {
      font-size: 1.15rem;
      font-weight: 700;
      color: var(--text-main);
      margin: 0;
    }
    .form-subtitle {
      font-size: 0.8125rem;
      color: var(--text-muted);
      margin-top: 0.15rem;
    }
    .form-card-body {
      padding: 1.5rem;
    }
    .form-card-footer {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 1rem 1.5rem;
      background: var(--bg-surface-elevated);
      border-top: 1px solid var(--border-subtle);
      flex-wrap: wrap;
      gap: 0.75rem;
    }
    .footer-buttons {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      margin-left: auto;
    }
  `]
})
export class FormCardComponent {
  @Input() title: string = '';
  @Input() subtitle: string = '';
  @Input() icon: string = '';
  @Input() showFooter: boolean = true;
  @Input() showCancel: boolean = true;
  @Input() submitText: string = 'Save Changes';
  @Input() cancelText: string = 'Cancel';
  @Input() submitIcon: string = 'check';
  @Input() submitting: boolean = false;
  @Input() submitDisabled: boolean = false;

  @Output() submit = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();
}
