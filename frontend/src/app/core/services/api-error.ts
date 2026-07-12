import { HttpErrorResponse } from '@angular/common/http';

/**
 * Pulls a human-readable message out of a failed backend call. The backend returns errors as the
 * ApiResponse envelope ({ errorMessage, errorCode, ... }); this reads that, then falls back.
 */
export function extractApiError(err: unknown, fallback = 'Something went wrong.'): string {
  if (err instanceof HttpErrorResponse) {
    const body = err.error as { errorMessage?: string; message?: string } | string | null;
    if (typeof body === 'string' && body.trim().length > 0) {
      return body;
    }
    if (body && typeof body === 'object') {
      if (body.errorMessage) {
        return body.errorMessage;
      }
      if (body.message) {
        return body.message;
      }
    }
  }
  return fallback;
}
