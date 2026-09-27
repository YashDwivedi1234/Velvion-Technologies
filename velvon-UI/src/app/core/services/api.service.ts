import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, map, of, throwError } from 'rxjs';
import { ApiResponse } from '../models/api.models';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private http = inject(HttpClient);
  private platformId = inject(PLATFORM_ID);
  // Default ASP.NET Core API Base URL
  private readonly baseUrl = 'http://localhost:5164/api';

  get<T>(endpoint: string, queryParams?: Record<string, any>): Observable<ApiResponse<T>> {
    if (!isPlatformBrowser(this.platformId)) {
      return of({
        success: true,
        message: 'SSR Pre-render pass',
        data: [] as any,
        errors: [],
        statusCode: 200
      });
    }

    let params = new HttpParams();
    if (queryParams) {
      Object.keys(queryParams).forEach(key => {
        if (queryParams[key] !== undefined && queryParams[key] !== null) {
          params = params.set(key, queryParams[key]);
        }
      });
    }

    return this.http.get<ApiResponse<T>>(`${this.baseUrl}/${endpoint}`, { params }).pipe(
      catchError(error => {
        console.error(`API GET [${endpoint}] Error:`, error);
        return of({
          success: false,
          message: error.error?.message || error.message || 'Server request failed',
          data: null as any,
          errors: error.error?.errors || [error.statusText],
          statusCode: error.status || 500
        });
      })
    );
  }

  post<T>(endpoint: string, body: any, queryParams?: Record<string, any>): Observable<ApiResponse<T>> {
    let params = new HttpParams();
    if (queryParams) {
      Object.keys(queryParams).forEach(key => {
        if (queryParams[key] !== undefined && queryParams[key] !== null) {
          params = params.set(key, queryParams[key]);
        }
      });
    }

    return this.http.post<ApiResponse<T>>(`${this.baseUrl}/${endpoint}`, body, { params }).pipe(
      catchError(error => {
        console.error(`API POST [${endpoint}] Error:`, error);
        return of({
          success: false,
          message: error.error?.message || error.message || 'Server operation failed',
          data: null as any,
          errors: error.error?.errors || [error.statusText],
          statusCode: error.status || 500
        });
      })
    );
  }
}
