import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { ConfigService } from './config.service';
import { BehaviorSubject, catchError, Observable, tap, throwError } from 'rxjs';
import {
  AuthResponse,
  LoginRequest,
  RegisterUserRequest,
  User,
} from '../models';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private configService = inject(ConfigService);

  private currentUserSubject = new BehaviorSubject<User | null>(
    this.getUserFromStorage()
  );
  public currentUser$ = this.currentUserSubject.asObservable();

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(
    this.hasValidToken()
  );
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor() {
    this.checkTokenValidity();
  }

  private get apiUrl(): string {
    return this.configService.getApiUrl();
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/Auth/login`, credentials)
      .pipe(
        tap((response) => {
          if (response.success && response.data) {
            this.setSession(response);
          }
        }),
        catchError(this.handleError)
      );
  }

  register(userData: RegisterUserRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/auth/register`, userData)
      .pipe(
        tap((response) => {
          if (
            response.success &&
            response.data.accessToken &&
            response.data.user
          ) {
            this.setSession(response);
          }
        }),
        catchError(this.handleError)
      );
  }

  logout(): void {
    this.clearSession();

    this.router.navigate(['/login']);
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();

    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }

    return this.http
      .post<AuthResponse>(`${this.apiUrl}/auth/refresh`, {
        refreshToken,
      })
      .pipe(
        tap((response) => {
          if (response.success && response.data && response.data.accessToken) {
            this.setToken(response.data.accessToken);
            if (response.data.refreshToken) {
              this.setRefreshToken(response.data.refreshToken);
            }
          }
        }),
        catchError((err) => {
          this.logout();
          return throwError(() => err);
        })
      );
  }

  isAuthenticated(): boolean {
    return this.hasValidToken();
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  getToken(): string | null {
    return localStorage.getItem('access_token');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refresh_token');
  }

  private hasValidToken(): boolean {
    const token = this.getToken();
    if (!token) return false;

    try {
      const payload = this.decodeToken(token);
      const expirationDate = new Date(payload.exp * 1000);
      return expirationDate > new Date();
    } catch (error) {
      return false;
    }
  }

  private decodeToken(token: string): any {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map((c) => {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
          })
          .join('')
      );
      return JSON.parse(jsonPayload);
    } catch (error) {
      throw new Error('Invalid token');
    }
  }

  private setSession(authResponse: AuthResponse): void {
    if (authResponse.data.accessToken) {
      this.setToken(authResponse.data.accessToken);
    }

    if (authResponse.data.refreshToken) {
      this.setRefreshToken(authResponse.data.refreshToken);
    }

    if (authResponse.data.user) {
      this.setUser(authResponse.data.user);
    }

    this.isAuthenticatedSubject.next(true);
  }

  private clearSession(): void {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('current_user');
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
  }

  private setToken(token: string): void {
    localStorage.setItem('access_token', token);
  }

  private setRefreshToken(token: string): void {
    localStorage.setItem('refresh_token', token);
  }

  private setUser(user: User): void {
    localStorage.setItem('current_user', JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  private getUserFromStorage(): User | null {
    const userJson = localStorage.getItem('current_user');
    if (userJson) {
      try {
        return JSON.parse(userJson);
      } catch (error) {
        return null;
      }
    }
    return null;
  }

  private checkTokenValidity(): void {
    if (!this.hasValidToken()) {
      this.clearSession();
    }
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'Ocurrió un error desconocido';

    if (error.error instanceof ErrorEvent) {
      // Error del cliente
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Error del servidor
      if (error.status === 401) {
        errorMessage = 'Credenciales inválidas';
      } else if (error.status === 403) {
        errorMessage = 'No tienes permisos para realizar esta acción';
      } else if (error.status === 404) {
        errorMessage = 'Recurso no encontrado';
      } else if (error.status === 500) {
        errorMessage = 'Error en el servidor. Por favor, intenta más tarde';
      } else if (error.error?.message) {
        errorMessage = error.error.message;
      } else {
        errorMessage = `Error ${error.status}: ${error.statusText}`;
      }
    }

    console.error('❌ Error HTTP:', errorMessage, error);
    return throwError(() => new Error(errorMessage));
  }

  getUserFromToken(): User | null {
    const token = this.getToken();

    if (!token) {
      return null;
    }

    try {
      const payload = this.decodeToken(token);

      return {
        id: payload.sub || payload.userId,
        email: payload.email,
        name: payload.name || '',
        surname: payload.surname || '',
        role: payload.role || 'user',
        isActive: true,
        createdAt: new Date(),
      };
    } catch (error) {
      return null;
    }
  }

  hasRole(role: string): boolean {
    const user = this.getCurrentUser();
    return user?.role === role;
  }

  isTokenExpiringSoon(): boolean {
    const token = this.getToken();
    if (!token) return false;

    try {
      const payload = this.decodeToken(token);
      const expirationDate = new Date(payload.exp * 1000);
      const now = new Date();
      const fiveMinutes = 5 * 60 * 1000;

      return expirationDate.getTime() - now.getTime() < fiveMinutes;
    } catch (error) {
      return false;
    }
  }
}
