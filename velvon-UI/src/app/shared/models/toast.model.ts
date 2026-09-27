export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface Toast {
  id: string;
  type: ToastType;
  title?: string;
  message: string;
  duration?: number; // duration in ms, 0 means sticky
  createdAt: number;
  remainingTime?: number;
}
