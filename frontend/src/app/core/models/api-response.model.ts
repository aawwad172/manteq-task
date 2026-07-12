/** Mirrors the backend ApiResponse<T> envelope (System.Text.Json camelCase). */
export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  statusCode: number;
  errorMessage: string | null;
  errorCode: string | null;
}

/** Mirrors the backend PaginationResult<T>. `page` holds the items for the current page. */
export interface PaginationResult<T> {
  totalDisplayRecords: number;
  totalRecords: number;
  pageNumber: number;
  pageSize: number;
  page: T[] | null;
}
