import { Injectable, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { ApiService } from './api.service';
import { LoginDto, LoginResponseDto, SaveUserDto, UserDto, ApiResponse } from '../models/api.models';

const STORAGE_KEY = 'velvion_auth_user';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private api = inject(ApiService);
  private router = inject(Router);

  currentUser = signal<LoginResponseDto | null>(this.getStoredUser());
  isLoggedIn = computed(() => !!this.currentUser());

  constructor() {
    // Initial check
  }

  private getStoredUser(): LoginResponseDto | null {
    if (typeof window !== 'undefined' && window.localStorage) {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (stored) {
        try {
          return JSON.parse(stored);
        } catch {
          localStorage.removeItem(STORAGE_KEY);
        }
      }
    }
    return null;
  }

  login(dto: LoginDto): Observable<ApiResponse<LoginResponseDto>> {
    return this.api.post<LoginResponseDto>('Users/login', dto).pipe(
      tap(res => {
        if (res.success && res.data) {
          this.currentUser.set(res.data);
          if (typeof window !== 'undefined' && window.localStorage) {
            localStorage.setItem(STORAGE_KEY, JSON.stringify(res.data));
          }
        }
      })
    );
  }

  register(dto: SaveUserDto): Observable<ApiResponse<UserDto>> {
    return this.api.post<UserDto>('Users/save', dto);
  }

  logout(): void {
    this.currentUser.set(null);
    if (typeof window !== 'undefined' && window.localStorage) {
      localStorage.removeItem(STORAGE_KEY);
    }
    this.router.navigate(['/']);
  }
}
