import { Injectable, computed, signal } from '@angular/core';

const TOKEN_KEY = 'access_token';

/** Claim key the backend uses for granular permissions (CustomClaims.Permission). */
const PERMISSION_CLAIM = 'Permission';

interface JwtPayload {
  exp?: number;
  [claim: string]: unknown;
}

/**
 * Stores the JWT and exposes what the UI needs to gate itself: the raw token (for the
 * interceptor), authentication state, and the permission claims decoded from the token.
 * The token is the single source of truth for "what can this user do" — matching the
 * backend, which authorizes on the same permission claims.
 */
@Injectable({ providedIn: 'root' })
export class TokenService {
  private readonly _token = signal<string | null>(localStorage.getItem(TOKEN_KEY));

  /** Current raw access token (or null). */
  readonly token = this._token.asReadonly();

  private readonly payload = computed<JwtPayload | null>(() => decodeJwt(this._token()));

  /** Permission claim values held by the current user. */
  readonly permissions = computed<string[]>(() => {
    const claim = this.payload()?.[PERMISSION_CLAIM];
    if (Array.isArray(claim)) {
      return claim.filter((c): c is string => typeof c === 'string');
    }
    return typeof claim === 'string' ? [claim] : [];
  });

  /** True when a non-expired token is present. */
  readonly isAuthenticated = computed<boolean>(() => {
    const p = this.payload();
    if (!p) {
      return false;
    }
    if (typeof p.exp === 'number') {
      return p.exp * 1000 > Date.now();
    }
    return true;
  });

  setToken(token: string): void {
    localStorage.setItem(TOKEN_KEY, token);
    this._token.set(token);
  }

  clear(): void {
    localStorage.removeItem(TOKEN_KEY);
    this._token.set(null);
  }

  hasPermission(permission: string): boolean {
    return this.permissions().includes(permission);
  }
}

/** Decodes a JWT payload without verifying the signature (verification is the server's job). */
function decodeJwt(token: string | null): JwtPayload | null {
  if (!token) {
    return null;
  }
  const parts = token.split('.');
  if (parts.length !== 3) {
    return null;
  }
  try {
    const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
    const json = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join(''),
    );
    return JSON.parse(json) as JwtPayload;
  } catch {
    return null;
  }
}
