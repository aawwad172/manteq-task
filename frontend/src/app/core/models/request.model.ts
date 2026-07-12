/** Backend RequestStatus enum, serialized as a string. */
export type RequestStatus = 'Draft' | 'Submitted' | 'Approved' | 'Rejected';

/** Backend RequestResult read model. Dates arrive as ISO strings. */
export interface RequestResult {
  id: string;
  requestNumber: string;
  doctorId: string;
  procedureName: string;
  /** DateOnly -> "yyyy-MM-dd". */
  procedureDate: string;
  estimatedCost: number;
  status: RequestStatus;
  createdAt: string;
  updatedAt: string | null;
}

/** POST /api/requests body (CreateRequestCommand). */
export interface CreateRequestPayload {
  procedureName: string;
  /** "yyyy-MM-dd". */
  procedureDate: string;
  estimatedCost: number;
}

/** PUT /api/requests/{id} body (EditRequestCommand, minus the route id). */
export type EditRequestPayload = CreateRequestPayload;

/** POST /api/requests/{id}/approve body (reason optional). */
export interface ApproveRequestPayload {
  reason: string | null;
}

/** POST /api/requests/{id}/reject body (reason required, min 10 chars — enforced server-side too). */
export interface RejectRequestPayload {
  reason: string;
}

/** Query params for GET /api/requests. Row scope (own vs all) is decided server-side. */
export interface ListRequestsQuery {
  status?: RequestStatus | null;
  /** "yyyy-MM-dd". */
  createdFrom?: string | null;
  createdTo?: string | null;
  procedureFrom?: string | null;
  procedureTo?: string | null;
  pageNumber?: number | null;
  pageSize?: number | null;
}
