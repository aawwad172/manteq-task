import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DatePickerModule } from 'primeng/datepicker';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';

import { extractApiError } from '../../../core/services/api-error';
import { RequestsService } from '../../../core/services/requests.service';
import { CreateRequestPayload, RequestResult } from '../../../core/models/request.model';
import { parseDateOnly, toDateOnly } from '../../../core/utils/date-only';

@Component({
  selector: 'app-request-form',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    CardModule,
    InputTextModule,
    InputNumberModule,
    DatePickerModule,
    ButtonModule,
    MessageModule,
  ],
  templateUrl: './request-form.html',
  styleUrl: './request-form.css',
})
export class RequestForm {
  private readonly fb = inject(FormBuilder);
  private readonly requests = inject(RequestsService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly message = inject(MessageService);

  private readonly id = this.route.snapshot.paramMap.get('id');
  readonly isEdit = !!this.id;

  readonly saving = signal(false);
  readonly today = new Date();

  readonly form = this.fb.nonNullable.group({
    procedureName: ['', [Validators.required]],
    procedureDate: this.fb.control<Date | null>(null, [Validators.required, notInPast]),
    estimatedCost: this.fb.control<number | null>(null, [Validators.required, Validators.min(0.01)]),
  });

  constructor() {
    if (this.isEdit) {
      this.loadForEdit();
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const payload: CreateRequestPayload = {
      procedureName: raw.procedureName,
      procedureDate: toDateOnly(raw.procedureDate) as string,
      estimatedCost: raw.estimatedCost as number,
    };

    this.saving.set(true);
    const request$ =
      this.isEdit && this.id
        ? this.requests.update(this.id, payload)
        : this.requests.create(payload);

    request$.subscribe({
      next: (result) => {
        this.saving.set(false);
        this.message.add({
          severity: 'success',
          summary: this.isEdit ? 'Request updated' : 'Request created',
        });
        void this.router.navigate(['/requests', result.id], { state: { request: result } });
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.message.add({
          severity: 'error',
          summary: this.isEdit ? 'Update failed' : 'Create failed',
          detail: extractApiError(err),
        });
      },
    });
  }

  private loadForEdit(): void {
    const fromState = (history.state?.request as RequestResult | undefined) ?? undefined;
    if (fromState && fromState.id === this.id) {
      this.applyRequest(fromState);
      return;
    }

    // Direct load / refresh: fall back to fetching (no GET-by-id endpoint, so this scans the list).
    this.requests.getById(this.id as string).subscribe({
      next: (request) => {
        if (!request) {
          this.message.add({ severity: 'error', summary: 'Request not found' });
          void this.router.navigate(['/requests']);
          return;
        }
        this.applyRequest(request);
      },
      error: (err: HttpErrorResponse) => {
        this.message.add({
          severity: 'error',
          summary: 'Failed to load request',
          detail: extractApiError(err),
        });
        void this.router.navigate(['/requests']);
      },
    });
  }

  private applyRequest(request: RequestResult): void {
    // Only Draft requests are editable; anything else goes back to the read-only detail.
    if (request.status !== 'Draft') {
      this.message.add({
        severity: 'warn',
        summary: 'Not editable',
        detail: `A ${request.status} request cannot be edited.`,
      });
      void this.router.navigate(['/requests', request.id], { state: { request } });
      return;
    }

    this.form.patchValue({
      procedureName: request.procedureName,
      procedureDate: parseDateOnly(request.procedureDate),
      estimatedCost: request.estimatedCost,
    });
  }
}

/** Mirrors the backend rule: procedure date cannot be in the past. */
function notInPast(control: AbstractControl): ValidationErrors | null {
  const value = control.value as Date | null;
  if (!value) {
    return null;
  }
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const picked = new Date(value);
  picked.setHours(0, 0, 0, 0);
  return picked < today ? { pastDate: true } : null;
}
