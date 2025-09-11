import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap, catchError, throwError } from 'rxjs';
import {
  AdminLoginRequest,
  AdminLoginResponse,
  TeamJoinRequest,
  TeamJoinResponse,
} from '../models/auth.model';
import { environment } from '../../environments/environment';

export interface AuthState {
  isAuthenticated: boolean;
  userType: 'admin' | 'team' | null;
  token: string | null;
  teamInfo?: {
    teamId: number;
    eventId: number;
    teamName: string;
    eventName: string;
  };
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = environment.apiUrl || 'http://localhost:5000';

  private authStateSubject = new BehaviorSubject<AuthState>({
    isAuthenticated: false,
    userType: null,
    token: null,
  });

  public authState$ = this.authStateSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {
    this.initializeAuthState();
  }

  private initializeAuthState(): void {
    const adminToken = localStorage.getItem('adminToken');
    const teamSession = localStorage.getItem('teamSession');
    const teamInfo = localStorage.getItem('teamInfo');

    if (adminToken && this.isTokenValid(adminToken)) {
      this.authStateSubject.next({
        isAuthenticated: true,
        userType: 'admin',
        token: adminToken,
      });
    } else if (teamSession && teamInfo) {
      try {
        const parsedTeamInfo = JSON.parse(teamInfo);
        this.authStateSubject.next({
          isAuthenticated: true,
          userType: 'team',
          token: teamSession,
          teamInfo: parsedTeamInfo,
        });
      } catch (error) {
        console.error('Error parsing team info from localStorage', error);
        this.clearTeamAuth();
      }
    }
  }

  adminLogin(credentials: AdminLoginRequest): Observable<AdminLoginResponse> {
    return this.http
      .post<AdminLoginResponse>(`${this.apiUrl}/api/auth/admin/login`, credentials)
      .pipe(
        tap((response) => {
          localStorage.setItem('adminToken', response.token);
          localStorage.setItem('adminTokenExpiry', response.expiresAt);

          this.authStateSubject.next({
            isAuthenticated: true,
            userType: 'admin',
            token: response.token,
          });
        }),
        catchError(this.handleError)
      );
  }

  teamJoin(joinRequest: TeamJoinRequest): Observable<TeamJoinResponse> {
    return this.http.post<TeamJoinResponse>(`${this.apiUrl}/api/auth/team/join`, joinRequest).pipe(
      tap((response) => {
        localStorage.setItem('teamSession', response.sessionToken);
        localStorage.setItem(
          'teamInfo',
          JSON.stringify({
            teamId: response.teamId,
            eventId: response.eventId,
            teamName: response.teamName,
            eventName: response.eventName,
          })
        );

        this.authStateSubject.next({
          isAuthenticated: true,
          userType: 'team',
          token: response.sessionToken,
          teamInfo: {
            teamId: response.teamId,
            eventId: response.eventId,
            teamName: response.teamName,
            eventName: response.eventName,
          },
        });
      }),
      catchError(this.handleError)
    );
  }

  getTeamQRCode(teamCode: string): Observable<Blob> {
    const headers = this.getAdminHeaders();
    return this.http
      .get(`${this.apiUrl}/api/auth/team/qr/${teamCode}`, {
        headers,
        responseType: 'blob',
      })
      .pipe(catchError(this.handleError));
  }

  adminLogout(): void {
    localStorage.removeItem('adminToken');
    localStorage.removeItem('adminTokenExpiry');
    this.clearAuthState();
    this.router.navigate(['/admin/login']);
  }

  teamLogout(): void {
    this.clearTeamAuth();
    this.clearAuthState();
    this.router.navigate(['/team/join']);
  }

  private clearTeamAuth(): void {
    localStorage.removeItem('teamSession');
    localStorage.removeItem('teamInfo');
  }

  private clearAuthState(): void {
    this.authStateSubject.next({
      isAuthenticated: false,
      userType: null,
      token: null,
    });
  }

  get isAuthenticated(): boolean {
    return this.authStateSubject.value.isAuthenticated;
  }

  get isAdmin(): boolean {
    return this.authStateSubject.value.userType === 'admin';
  }

  get isTeam(): boolean {
    return this.authStateSubject.value.userType === 'team';
  }

  get currentToken(): string | null {
    return this.authStateSubject.value.token;
  }

  get teamInfo() {
    return this.authStateSubject.value.teamInfo;
  }

  getAdminHeaders(): HttpHeaders {
    const token = this.currentToken;
    if (!token || !this.isAdmin) {
      throw new Error('Admin authentication required');
    }
    return new HttpHeaders().set('Authorization', `Bearer ${token}`);
  }

  getTeamHeaders(): HttpHeaders {
    const token = this.currentToken;
    if (!token || !this.isTeam) {
      throw new Error('Team authentication required');
    }
    return new HttpHeaders().set('X-Team-Session', token);
  }

  private isTokenValid(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expiry = payload.exp * 1000; // Convert to milliseconds
      return Date.now() < expiry;
    } catch (error) {
      return false;
    }
  }

  checkTokenExpiry(): void {
    const authState = this.authStateSubject.value;
    if (!authState.isAuthenticated || !authState.token) return;

    if (authState.userType === 'admin') {
      const expiry = localStorage.getItem('adminTokenExpiry');
      if (expiry && new Date(expiry) <= new Date()) {
        console.log('Admin token expired, logging out...');
        this.adminLogout();
      }
    }
  }

  // Error handling
  private handleError = (error: any) => {
    console.error('Auth Service Error:', error);

    if (error.status === 401) {
      // Unauthorized - clear auth state
      this.clearAuthState();
      if (error.url?.includes('admin')) {
        this.router.navigate(['/admin/login']);
      } else {
        this.router.navigate(['/team/join']);
      }
    }

    return throwError(() => error);
  };

  getTeamJoinUrl(teamCode: string): string {
    return `${window.location.origin}/join/${teamCode}`;
  }
}
