import { Injectable, signal } from '@angular/core';
import { Toast, ToastType } from '../models/toast.model';

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toastsSignal = signal<Toast[]>([]);
  public toasts = this.toastsSignal.asReadonly();

  public show(message: string, type: ToastType = 'info', title?: string, duration: number = 4500): string {
    const id = 'toast_' + Date.now() + '_' + Math.random().toString(36).substring(2, 7);
    const newToast: Toast = {
      id,
      type,
      title: title || this.getDefaultTitle(type),
      message,
      duration,
      createdAt: Date.now()
    };

    this.toastsSignal.update(toasts => [...toasts, newToast]);

    if (duration > 0) {
      setTimeout(() => {
        this.remove(id);
      }, duration);
    }

    return id;
  }

  public success(message: string, title?: string, duration?: number): string {
    return this.show(message, 'success', title, duration);
  }

  public error(message: string, title?: string, duration?: number): string {
    return this.show(message, 'error', title, duration);
  }

  public warning(message: string, title?: string, duration?: number): string {
    return this.show(message, 'warning', title, duration);
  }

  public info(message: string, title?: string, duration?: number): string {
    return this.show(message, 'info', title, duration);
  }

  public remove(id: string): void {
    this.toastsSignal.update(toasts => toasts.filter(t => t.id !== id));
  }

  public clear(): void {
    this.toastsSignal.set([]);
  }

  private getDefaultTitle(type: ToastType): string {
    switch (type) {
      case 'success': return 'Success';
      case 'error': return 'Error';
      case 'warning': return 'Warning';
      case 'info': return 'Notification';
    }
  }
}
