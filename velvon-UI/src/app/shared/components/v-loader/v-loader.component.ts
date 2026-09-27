import { Component, Input, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoaderService } from '../../services/loader.service';

@Component({
  selector: 'app-v-loader',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './v-loader.component.html',
  styleUrls: ['./v-loader.component.css']
})
export class VLoaderComponent implements OnInit, OnDestroy {
  public loaderService = inject(LoaderService);

  @Input() isSplashScreen: boolean = false;
  @Input() message: string = 'Initializing Enterprise Systems...';
  @Input() fullscreen: boolean = true;

  dots = signal<string>('.');
  private intervalId: any;

  ngOnInit(): void {
    let count = 0;
    this.intervalId = setInterval(() => {
      count = (count + 1) % 4;
      this.dots.set('.'.repeat(count || 1));
    }, 400);
  }

  ngOnDestroy(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }
  }
}
