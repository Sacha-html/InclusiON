import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, catchError, Observable, tap, throwError } from 'rxjs';
import {
  ApiResponse,
  AuthResponse,
  LoginRequest,
  RegisterUserRequest,
  User,
  IdentifyUserRequest,
  PinLoginRequest,
  VisualStandardLoginRequest,
  FamilyLoginRequest,
  AssistedLoginRequest,
  UpdateLoginMethodRequest,
  ChangePasswordRequest,
  IdentifyUserResponse,
  VisualLoginResponse,
  LoginMethodsResponse,
  UserProfileResponse,
} from '@models';
import { LocalStorageService, STORAGE_KEYS } from './local-storage.service';
import { environment } from '@env';

interface JwtPayload {
  sub?: string;
  userId?: string;
  email?: string;
  name?: string;
  surname?: string;
  role?: string;
  permission?: string | string[];
  isGlobalAdmin?: string | boolean;
  institutionId?: string | number | (string | number)[];
  exp?: number;
  [key: string]: unknown;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private storage = inject(LocalStorageService);

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
    return environment.apiUrl;
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

  /**
   * Obtiene el perfil del usuario autenticado desde el servidor.
   * Requiere autenticación.
   */
  getProfile(): Observable<ApiResponse<UserProfileResponse>> {
    return this.http
      .get<ApiResponse<UserProfileResponse>>(`${this.apiUrl}/Auth/profile`)
      .pipe(catchError(this.handleError));
  }

  isAuthenticated(): boolean {
    return this.hasValidToken();
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  getToken(): string | null {
    return this.storage.getAccessToken();
  }

  getRefreshToken(): string | null {
    return this.storage.getRefreshToken();
  }

  private hasValidToken(): boolean {
    const token = this.getToken();
    if (!token) return false;

    try {
      const payload = this.decodeToken(token);
      const exp = payload.exp;
      if (!exp) return false;
      const expirationDate = new Date(exp * 1000);
      return expirationDate > new Date();
    } catch (error) {
      return false;
    }
  }

  private decodeToken(token: string): JwtPayload {
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
    this.storage.clearSession();
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
  }

  private setToken(token: string): void {
    this.storage.setAccessToken(token);
  }

  private setRefreshToken(token: string): void {
    this.storage.setRefreshToken(token);
  }

  private setUser(user: User): void {
    this.storage.setCurrentUser(user);
    this.currentUserSubject.next(user);
  }

  private getUserFromStorage(): User | null {
    return this.storage.getCurrentUser<User>();
  }

  private checkTokenValidity(): void {
    if (!this.hasValidToken()) {
      this.clearSession();
    }
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    // El interceptor ya maneja el error y lo enriquece con errorCode
    // Solo re-lanzamos para que los componentes puedan manejarlo
    return throwError(() => error);
  }

  getUserFromToken(): User | null {
    const token = this.getToken();

    if (!token) {
      return null;
    }

    try {
      const payload = this.decodeToken(token);

      return {
        id: payload.sub || payload.userId || '',
        email: payload.email || '',
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
 
  hasPermission(permission: string): boolean {
    const token = this.getToken();
    if (!token) return false;

    try {
      const payload = this.decodeToken(token);
      const raw = payload.permission;
      const permissions: string[] = Array.isArray(raw) ? raw : raw ? [raw] : [];
      return Array.isArray(permissions)
        ? permissions.includes(permission)
        : permissions === permission;
    } catch {
      return false;
    }
  }

  /**
   * Verifica si el usuario autenticado es un administrador global.
   * Retorna true solo si el rol es Admin y el claim isGlobalAdmin es "true".
   */
  isGlobalAdmin(): boolean {
    const token = this.getToken();
    if (!token) return false;

    try {
      const payload = this.decodeToken(token);
      const isAdmin = payload.role === 'Admin';
      const isGlobal = payload.isGlobalAdmin === 'true' || payload.isGlobalAdmin === true;
      return isAdmin && isGlobal;
    } catch {
      return false;
    }
  }

  /**
   * Obtiene los IDs de instituciones asignadas al usuario desde el JWT.
   * El claim institutionId puede ser un solo valor o un array.
   */
  getInstitutionIds(): number[] {
    const token = this.getToken();
    if (!token) return [];

    try {
      const payload = this.decodeToken(token);
      const raw = payload.institutionId;
      if (!raw) return [];

      if (Array.isArray(raw)) {
        return raw.map((id: string | number) => Number(id)).filter((id: number) => !isNaN(id));
      }

      const parsed = Number(raw);
      return isNaN(parsed) ? [] : [parsed];
    } catch {
      return [];
    }
  }

  /**
   * Verifica si el usuario tiene alguno de los roles especificados
   */
  hasAnyRole(roles: string[]): boolean {
    const user = this.getCurrentUser();
    if (!user) return false;
    return roles.includes(user.role);
  }

  /**
   * Obtiene el rol del usuario actual
   */
  getUserRole(): string | null {
    const user = this.getCurrentUser();
    return user?.role || null;
  }

  // Visual Login Methods

  identifyUser(request: IdentifyUserRequest): Observable<IdentifyUserResponse> {
    return this.http
      .post<IdentifyUserResponse>(`${this.apiUrl}/Auth/identify`, request)
      .pipe(catchError(this.handleError));
  }

  loginWithPin(request: PinLoginRequest): Observable<VisualLoginResponse> {
    return this.http
      .post<VisualLoginResponse>(`${this.apiUrl}/Auth/login/pin`, request)
      .pipe(
        tap((response) => {
          if (response.success && response.data?.success) {
            this.setVisualLoginSession(response);
          }
        }),
        catchError(this.handleError)
      );
  }

  loginVisualStandard(request: VisualStandardLoginRequest): Observable<VisualLoginResponse> {
    return this.http
      .post<VisualLoginResponse>(`${this.apiUrl}/Auth/login/visual-standard`, request)
      .pipe(
        tap((response) => {
          if (response.success && response.data?.success) {
            this.setVisualLoginSession(response);
          }
        }),
        catchError(this.handleError)
      );
  }

  loginFamily(request: FamilyLoginRequest): Observable<VisualLoginResponse> {
    return this.http
      .post<VisualLoginResponse>(`${this.apiUrl}/Auth/login/family`, request)
      .pipe(
        tap((response) => {
          if (response.success && response.data?.success) {
            this.setVisualLoginSession(response);
          }
        }),
        catchError(this.handleError)
      );
  }

  loginAssisted(request: AssistedLoginRequest): Observable<VisualLoginResponse> {
    return this.http
      .post<VisualLoginResponse>(`${this.apiUrl}/Auth/login/assisted`, request)
      .pipe(
        tap((response) => {
          if (response.success && response.data?.success) {
            this.setVisualLoginSession(response);
          }
        }),
        catchError(this.handleError)
      );
  }

  private setVisualLoginSession(response: VisualLoginResponse): void {
    if (response.data?.accessToken) {
      this.setToken(response.data.accessToken);
    }
    if (response.data?.refreshToken) {
      this.setRefreshToken(response.data.refreshToken);
    }
    if (response.data?.user) {
      const userInfo = response.data.user;
      const user: User = {
        id: userInfo.id,
        email: '',
        name: userInfo.displayName,
        surname: '',
        role: userInfo.roles[0] || 'Person',
        isActive: true,
        createdAt: new Date(),
      };
      this.setUser(user);

      // Store accessibility preferences
      if (userInfo.accessibility) {
        this.storage.setObject(STORAGE_KEYS.ACCESSIBILITY_PREFERENCES, userInfo.accessibility);
      }
    }
    this.isAuthenticatedSubject.next(true);
  }

  getDeviceId(): string {
    return this.storage.getOrCreateDeviceId();
  }

  isTokenExpiringSoon(): boolean {
    const token = this.getToken();
    if (!token) return false;

    try {
      const payload = this.decodeToken(token);
      const exp = payload.exp;
      if (!exp) return false;
      const expirationDate = new Date(exp * 1000);
      const now = new Date();
      const fiveMinutes = 5 * 60 * 1000;

      return expirationDate.getTime() - now.getTime() < fiveMinutes;
    } catch (error) {
      return false;
    }
  }

  // Password Management

  changePassword(request: ChangePasswordRequest): Observable<ApiResponse<{ success: boolean }>> {
    return this.http
      .put<ApiResponse<{ success: boolean }>>(`${this.apiUrl}/Auth/change-password`, request)
      .pipe(catchError(this.handleError));
  }

  // Login Method Management

  getLoginMethods(): Observable<LoginMethodsResponse> {
    return this.http
      .get<LoginMethodsResponse>(`${this.apiUrl}/Auth/login-methods`)
      .pipe(catchError(this.handleError));
  }

  updateMyLoginMethod(request: UpdateLoginMethodRequest): Observable<UpdateLoginMethodApiResponse> {
    return this.http
      .put<UpdateLoginMethodApiResponse>(`${this.apiUrl}/Persons/me/login-method`, request)
      .pipe(catchError(this.handleError));
  }

  updateUserLoginMethod(userId: string, request: UpdateLoginMethodRequest): Observable<UpdateLoginMethodApiResponse> {
    return this.http
      .put<UpdateLoginMethodApiResponse>(`${this.apiUrl}/Persons/${userId}/login-method`, request)
      .pipe(catchError(this.handleError));
  }
}

export interface UpdateLoginMethodApiResponse {
  success: boolean;
  message: string;
  data: {
    updated: boolean;
    loginMethodId: number;
    loginMethodName: string;
    temporaryPassword?: string;
  } | null;
  errors: string[];
}
