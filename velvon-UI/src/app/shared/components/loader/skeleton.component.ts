import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-skeleton',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="skeleton shimmer-box"
      [style.width]="width"
      [style.height]="height"
      [style.border-radius]="circle ? '50%' : (borderRadius || 'var(--radius-md)')"
      [style.margin]="margin"
    ></div>
  `,
  styles: [`
    :host {
      display: block;
    }
    .skeleton {
      display: block;
      background: var(--shimmer-gradient);
      background-size: 200% 100%;
      animation: shimmer 1.6s infinite ease-in-out;
    }
  `]
})
export class SkeletonComponent {
  @Input() width: string = '100%';
  @Input() height: string = '20px';
  @Input() borderRadius: string = '';
  @Input() circle: boolean = false;
  @Input() margin: string = '0';
}
