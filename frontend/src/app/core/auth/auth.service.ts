import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { LoginRequest, LoginResult } from '../models/auth.model';
import { TokenService } from './token.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokens = inject(TokenService);

  readonly isAuthenticated = this.tokens.isAuthenticated;
  readonly permissions = this.tokens.permissions;

  /** Authenticates against POST /users/login and stores the returned access token. */
  login(credentials: LoginRequest): Observable<LoginResult> {
    return this.http
      .post<ApiResponse<LoginResult>>(`${environment.apiBaseUrl}/users/login`, credentials)
      .pipe(
        map((response) => {
          const result = response.data;
          if (!result) {
            throw new Error(response.errorMessage ?? 'Login failed.');
          }
          this.tokens.setToken(result.accessToken);
          return result;
        }),
      );
  }

  logout(): void {
    this.tokens.clear();
  }

  hasPermission(permission: string): boolean {
    return this.tokens.hasPermission(permission);
  }
}
