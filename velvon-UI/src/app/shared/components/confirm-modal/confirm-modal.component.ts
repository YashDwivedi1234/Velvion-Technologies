import { Component, HostListener, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConfirmService } from '../../services/confirm.service';
import { IconComponent } from '../icon/icon.component';

@Component({
  selector: 'app-confirm-modal',
  standalone: true,
  imports: [CommonModule, IconComponent],
  templateUrl: './confirm-modal.component.html',
  styleUrls: ['./confirm-modal.component.css']
})
export class ConfirmModalComponent {
  public confirmService = inject(ConfirmService);

  @HostListener('window:keydown.escape')
  onEscape(): void {
    if (this.confirmService.state().isOpen) {
      this.confirmService.onCancel();
    }
  }

  getIconName(type?: string): string {
    switch (type) {
      case 'danger': return 'trash';
      case 'warning': return 'alert-triangle';
      case 'success': return 'check-circle';
      default: return 'info';
    }
  }

  getConfirmBtnClass(type?: string): string {
    switch (type) {
      case 'danger': return 'btn-danger';
      case 'warning': return 'btn-warning-action';
      case 'success': return 'btn-success-action';
      default: return 'btn-primary';
    }
  }
}
