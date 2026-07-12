import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DatePickerModule } from 'primeng/datepicker';
import { SelectModule } from 'primeng/select';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';

import { AuthService } from '../../../core/auth/auth.service';
import { Permissions } from '../../../core/auth/permissions';
import { extractApiError } from '../../../core/services/api-error';
import { RequestsService } from '../../../core/services/requests.service';
import { RequestResult, RequestStatus } from '../../../core/models/request.model';
import { toDateOnly } from '../../../core/utils/date-only';
import { statusSeverity } from '../status-severity';

@Component({
  selector: 'app-requests-list',
  imports: [
    FormsModule,
    DecimalPipe,
    TableModule,
    TagModule,
    ButtonModule,
    SelectModule,
    DatePickerModule,
    TooltipModule,
  ],
  templateUrl: './requests-list.html',
  styleUrl: './requests-list.css',
})
export class RequestsList {
  private readonly requests = inject(RequestsService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly message = inject(MessageService);

  // Row scope is enforced by the backend from the JWT. The frontend only decides whether to
  // *show* the admin filters and the create button, based on the caller's permissions.
  readonly canViewAll = this.auth.hasPermission(Permissions.Requests.ViewAll);
  readonly canCreate = this.auth.hasPermission(Permissions.Requests.Create);
  readonly canEdit = this.auth.hasPermission(Permissions.Requests.Edit);

  readonly rows = signal<RequestResult[]>([]);
  readonly total = signal(0);
  readonly loading = signal(false);
  readonly first = signal(0);
  readonly pageSize = signal(10);

  readonly statusSeverity = statusSeverity;

  readonly statusOptions: { label: string; value: RequestStatus }[] = [
    { label: 'Draft', value: 'Draft' },
    { label: 'Submitted', value: 'Submitted' },
    { label: 'Approved', value: 'Approved' },
    { label: 'Rejected', value: 'Rejected' },
  ];

  // Admin filter models (plain fields for [(ngModel)]).
  statusFilter: RequestStatus | null = null;
  createdRange: Date[] | null = null;
  procedureRange: Date[] | null = null;

  private readonly title = computed(() => (this.canViewAll ? 'All requests' : 'My requests'));
  readonly heading = this.title;

  onLazyLoad(event: TableLazyLoadEvent): void {
    this.first.set(event.first ?? 0);
    this.pageSize.set(event.rows ?? 10);
    this.fetch();
  }

  applyFilters(): void {
    this.first.set(0);
    this.fetch();
  }

  clearFilters(): void {
    this.statusFilter = null;
    this.createdRange = null;
    this.procedureRange = null;
    this.applyFilters();
  }

  openDetail(row: RequestResult): void {
    // Pass the row via router state so the detail screen needs no extra fetch (no GET-by-id exists).
    void this.router.navigate(['/requests', row.id], { state: { request: row } });
  }

  editDraft(row: RequestResult, event: Event): void {
    event.stopPropagation();
    void this.router.navigate(['/requests', row.id, 'edit'], { state: { request: row } });
  }

  createNew(): void {
    void this.router.navigate(['/requests', 'new']);
  }

  private fetch(): void {
    const pageNumber = Math.floor(this.first() / this.pageSize()) + 1;
    this.loading.set(true);

    this.requests
      .list({
        pageNumber,
        pageSize: this.pageSize(),
        status: this.canViewAll ? this.statusFilter : null,
        createdFrom: this.canViewAll ? toDateOnly(this.createdRange?.[0]) : null,
        createdTo: this.canViewAll ? toDateOnly(this.createdRange?.[1]) : null,
        procedureFrom: this.canViewAll ? toDateOnly(this.procedureRange?.[0]) : null,
        procedureTo: this.canViewAll ? toDateOnly(this.procedureRange?.[1]) : null,
      })
      .subscribe({
        next: (page) => {
          this.rows.set(page.page ?? []);
          this.total.set(page.totalRecords);
          this.loading.set(false);
        },
        error: (err: HttpErrorResponse) => {
          this.loading.set(false);
          this.message.add({
            severity: 'error',
            summary: 'Failed to load requests',
            detail: extractApiError(err),
          });
        },
      });
  }
}
