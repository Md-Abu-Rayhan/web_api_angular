import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, tap, catchError, throwError } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private authUrl = '/api/v1/Auth/login';
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {
    this.checkAuthStatus();
  }

  login(credentials: { email: string; password: string }) {
    return this.http.post<{ token: string }>(this.authUrl, credentials).pipe(
      tap(response => {
        localStorage.setItem('jwt_token', response.token);
        this.isAuthenticatedSubject.next(true);
      }),
      catchError(error => {
        this.isAuthenticatedSubject.next(false);
        return throwError(() => new Error('Invalid credentials'));
      })
    );
  }

  logout() {
    localStorage.removeItem('jwt_token');
    this.isAuthenticatedSubject.next(false);
    this.router.navigate(['/auth/sign-in']);
  }

  getToken(): string | null {
    return localStorage.getItem('jwt_token');
  }

  private checkAuthStatus() {
    const token = this.getToken();
    this.isAuthenticatedSubject.next(!!token && !this.isTokenExpired(token));
  }

  private isTokenExpired(token: string): boolean {
    const expiry = (JSON.parse(atob(token.split('.')[1]))).exp;
    return (Math.floor(Date.now() / 1000)) >= expiry;
  }
} 