import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LoaderService {
  private countSignal = signal<number>(0);
  private messageSignal = signal<string>('Loading, please wait...');

  public isLoading = signal<boolean>(false);
  public message = this.messageSignal.asReadonly();

  public show(message: string = 'Loading, please wait...'): void {
    this.messageSignal.set(message);
    this.countSignal.update(c => c + 1);
    this.isLoading.set(true);
  }

  public hide(): void {
    this.countSignal.update(c => {
      const next = Math.max(0, c - 1);
      if (next === 0) {
        this.isLoading.set(false);
      }
      return next;
    });
  }

  public reset(): void {
    this.countSignal.set(0);
    this.isLoading.set(false);
  }
}
