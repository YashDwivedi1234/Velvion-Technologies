import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoaderService } from '../../services/loader.service';

@Component({
  selector: 'app-loader',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './loader.component.html',
  styleUrls: ['./loader.component.css']
})
export class LoaderComponent {
  public loaderService = inject(LoaderService);

  @Input() inline: boolean = false;
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Input() text: string = '';
}
