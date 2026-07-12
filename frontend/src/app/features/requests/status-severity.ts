import { Tag } from 'primeng/tag';

import { RequestStatus } from '../../core/models/request.model';

export type TagSeverity = Tag['severity'];

/** Maps a request status to a PrimeNG Tag severity (default Aura palette). */
export function statusSeverity(status: RequestStatus): TagSeverity {
  switch (status) {
    case 'Approved':
      return 'success';
    case 'Submitted':
      return 'info';
    case 'Rejected':
      return 'danger';
    case 'Draft':
    default:
      return 'secondary';
  }
}
