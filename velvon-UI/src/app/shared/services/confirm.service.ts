import { Injectable, signal } from '@angular/core';
import { ConfirmOptions, ConfirmState } from '../models/confirm.model';

@Injectable({
  providedIn: 'root'
})
export class ConfirmService {
  private stateSignal = signal<ConfirmState>({
    isOpen: false,
    title: '',
    message: '',
    type: 'warning',
    confirmText: 'Confirm',
    cancelText: 'Cancel'
  });

  public state = this.stateSignal.asReadonly();

  public confirm(options: ConfirmOptions): Promise<boolean> {
    return new Promise<boolean>((resolve) => {
      this.stateSignal.set({
        isOpen: true,
        title: options.title,
        message: options.message,
        type: options.type || 'warning',
        confirmText: options.confirmText || (options.type === 'danger' ? 'Delete' : 'Confirm'),
        cancelText: options.cancelText || 'Cancel',
        icon: options.icon,
        resolve
      });
    });
  }

  public danger(title: string, message: string, confirmText: string = 'Delete'): Promise<boolean> {
    return this.confirm({ title, message, type: 'danger', confirmText });
  }

  public warning(title: string, message: string, confirmText: string = 'Proceed'): Promise<boolean> {
    return this.confirm({ title, message, type: 'warning', confirmText });
  }

  public info(title: string, message: string, confirmText: string = 'Got it'): Promise<boolean> {
    return this.confirm({ title, message, type: 'info', confirmText });
  }

  public success(title: string, message: string, confirmText: string = 'OK'): Promise<boolean> {
    return this.confirm({ title, message, type: 'success', confirmText });
  }

  public onConfirm(): void {
    const currentState = this.stateSignal();
    if (currentState.resolve) {
      currentState.resolve(true);
    }
    this.close();
  }

  public onCancel(): void {
    const currentState = this.stateSignal();
    if (currentState.resolve) {
      currentState.resolve(false);
    }
    this.close();
  }

  private close(): void {
    this.stateSignal.update(s => ({ ...s, isOpen: false, resolve: undefined }));
  }
}
