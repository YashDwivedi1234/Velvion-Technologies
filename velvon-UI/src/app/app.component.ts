import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';

// Shared Library Components
import {
  ToastContainerComponent,
  ConfirmModalComponent,
  VLoaderComponent,
  LoaderService,
  ThemeService
} from './shared';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    ToastContainerComponent,
    ConfirmModalComponent,
    VLoaderComponent
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  public loaderService = inject(LoaderService);
  public themeService = inject(ThemeService);

  // Starting V Loader initial splash
  isInitialLoading = signal<boolean>(true);

  ngOnInit(): void {
    // Show high-tech circuit V loader on first launch
    if (typeof window !== 'undefined') {
      setTimeout(() => {
        this.isInitialLoading.set(false);
      }, 1200);
    } else {
      this.isInitialLoading.set(false);
    }
  }
}
