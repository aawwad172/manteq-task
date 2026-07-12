import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { MessageModule } from 'primeng/message';
import { TagModule } from 'primeng/tag';
import { TextareaModule } from 'primeng/textarea';

import { AuthService } from '../../../core/auth/auth.service';
import { Permissions } from '../../../core/auth/permissions';
import { extractApiError } from '../../../core/services/api-error';
import { RequestsService } from '../../../core/services/requests.service';
import { RequestResult } from '../../../core/models/request.model';
import { statusSeverity } from '../status-severity';

@Component({
  selector: 'app-request-detail',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    DatePipe,
    DecimalPipe,
    CardModule,
    TagModule,
    ButtonModule,
    DialogModule,
    TextareaModule,
    MessageModule,
  ],
  templateUrl: './request-detail.html',
  styleUrl: './request-detail.css',
})
export class RequestDetail {
  private readonly fb = inject(FormBuilder);
  private readonly requests = inject(RequestsService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly message = inject(MessageService);

  private readonly id = this.route.snapshot.paramMap.get('id') as string;

  readonly request = signal<RequestResult | null>(null);
  readonly loading = signal(true);
  readonly acting = signal(false);
  readonly statusSeverity = statusSeverity;

  rejectDialogVisible = false;
  readonly rejectForm = this.fb.nonNullable.group({
    reason: ['', [Validators.required, Validators.minLength(10)]],
  });

  // Action visibility mirrors the backend guards: Doctor submits a Draft; Admin approves/rejects a
  // Submitted request. The server re-checks permission, ownership, and status regardless.
  readonly canSubmit = computed(
    () => this.request()?.status === 'Draft' && this.auth.hasPermission(Permissions.Requests.Submit),
  );
  readonly canEdit = computed(
    () => this.request()?.status === 'Draft' && this.auth.hasPermission(Permissions.Requests.Edit),
  );
  readonly canApprove = computed(
    () =>
      this.request()?.status === 'Submitted' && this.auth.hasPermission(Permissions.Requests.Approve),
  );
  readonly canReject = computed(
    () =>
      this.request()?.status === 'Submitted' && this.auth.hasPermission(Permissions.Requests.Reject),
  );

  constructor() {
    const fromState = (history.state?.request as RequestResult | undefined) ?? undefined;
    if (fromState && fromState.id === this.id) {
      this.request.set(fromState);
      this.loading.set(false);
    } else {
      this.load();
    }
  }

  editDraft(): void {
    const current = this.request();
    if (current) {
      void this.router.navigate(['/requests', current.id, 'edit'], { state: { request: current } });
    }
  }

  submit(): void {
    this.run(this.requests.submit(this.id), 'Request submitted');
  }

  approve(): void {
    this.run(this.requests.approve(this.id, { reason: null }), 'Request approved');
  }

  openReject(): void {
    this.rejectForm.reset({ reason: '' });
    this.rejectDialogVisible = true;
  }

  confirmReject(): void {
    if (this.rejectForm.invalid) {
      this.rejectForm.markAllAsTouched();
      return;
    }
    this.run(
      this.requests.reject(this.id, { reason: this.rejectForm.getRawValue().reason }),
      'Request rejected',
      () => (this.rejectDialogVisible = false),
    );
  }

  private load(): void {
    this.loading.set(true);
    this.requests.getById(this.id).subscribe({
      next: (request) => {
        this.loading.set(false);
        if (!request) {
          this.message.add({ severity: 'error', summary: 'Request not found' });
          void this.router.navigate(['/requests']);
          return;
        }
        this.request.set(request);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.message.add({
          severity: 'error',
          summary: 'Failed to load request',
          detail: extractApiError(err),
        });
        void this.router.navigate(['/requests']);
      },
    });
  }

  private run(
    action: ReturnType<RequestsService['submit']>,
    successSummary: string,
    onSuccess?: () => void,
  ): void {
    this.acting.set(true);
    action.subscribe({
      next: (updated) => {
        this.acting.set(false);
        this.request.set(updated);
        onSuccess?.();
        this.message.add({ severity: 'success', summary: successSummary });
      },
      error: (err: HttpErrorResponse) => {
        this.acting.set(false);
        // 409 (illegal transition) and 400 (validation) surface here as toasts.
        this.message.add({
          severity: 'error',
          summary: 'Action failed',
          detail: extractApiError(err),
        });
      },
    });
  }
}
