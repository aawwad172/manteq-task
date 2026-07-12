import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse, PaginationResult } from '../models/api-response.model';
import {
  ApproveRequestPayload,
  CreateRequestPayload,
  EditRequestPayload,
  ListRequestsQuery,
  RejectRequestPayload,
  RequestResult,
} from '../models/request.model';

/**
 * Typed client for the pre-authorization request endpoints. Every call unwraps the backend
 * ApiResponse<T> envelope. Row scoping (own vs all) is decided server-side from the JWT, so
 * the list call sends only filter params.
 *
 * NOTE: the backend exposes no GET /api/requests/{id}. `getById` therefore scans the (role-scoped)
 * list as a fallback; the common path is to pass the row via router state from the list screen.
 */
@Injectable({ providedIn: 'root' })
export class RequestsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/requests`;

  list(query: ListRequestsQuery): Observable<PaginationResult<RequestResult>> {
    let params = new HttpParams();
    params = appendIfSet(params, 'Status', query.status);
    params = appendIfSet(params, 'CreatedFrom', query.createdFrom);
    params = appendIfSet(params, 'CreatedTo', query.createdTo);
    params = appendIfSet(params, 'ProcedureFrom', query.procedureFrom);
    params = appendIfSet(params, 'ProcedureTo', query.procedureTo);
    params = appendIfSet(params, 'PageNumber', query.pageNumber);
    params = appendIfSet(params, 'PageSize', query.pageSize);

    return this.http
      .get<ApiResponse<PaginationResult<RequestResult>>>(this.baseUrl, { params })
      .pipe(map((res) => unwrap(res)));
  }

  getById(id: string): Observable<RequestResult | undefined> {
    return this.list({ pageNumber: 1, pageSize: 500 }).pipe(
      map((page) => (page.page ?? []).find((r) => r.id === id)),
    );
  }

  create(payload: CreateRequestPayload): Observable<RequestResult> {
    return this.http
      .post<ApiResponse<RequestResult>>(this.baseUrl, payload)
      .pipe(map((res) => unwrap(res)));
  }

  update(id: string, payload: EditRequestPayload): Observable<RequestResult> {
    return this.http
      .put<ApiResponse<RequestResult>>(`${this.baseUrl}/${id}`, payload)
      .pipe(map((res) => unwrap(res)));
  }

  submit(id: string): Observable<RequestResult> {
    return this.http
      .post<ApiResponse<RequestResult>>(`${this.baseUrl}/${id}/submit`, {})
      .pipe(map((res) => unwrap(res)));
  }

  approve(id: string, payload: ApproveRequestPayload): Observable<RequestResult> {
    return this.http
      .post<ApiResponse<RequestResult>>(`${this.baseUrl}/${id}/approve`, payload)
      .pipe(map((res) => unwrap(res)));
  }

  reject(id: string, payload: RejectRequestPayload): Observable<RequestResult> {
    return this.http
      .post<ApiResponse<RequestResult>>(`${this.baseUrl}/${id}/reject`, payload)
      .pipe(map((res) => unwrap(res)));
  }
}

function unwrap<T>(response: ApiResponse<T>): T {
  if (response.data === null || response.data === undefined) {
    throw new Error(response.errorMessage ?? 'Request failed.');
  }
  return response.data;
}

function appendIfSet(
  params: HttpParams,
  key: string,
  value: string | number | null | undefined,
): HttpParams {
  return value === null || value === undefined || value === ''
    ? params
    : params.set(key, String(value));
}
