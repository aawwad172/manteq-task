/**
 * Permission claim values, mirrored from the backend PermissionConstants.
 * These are the strings carried in the JWT under the "Permission" claim.
 */
export const Permissions = {
  Requests: {
    Create: 'requests.create',
    Edit: 'requests.edit',
    Submit: 'requests.submit',
    ViewOwn: 'requests.view.own',
    ViewAll: 'requests.view.all',
    Approve: 'requests.approve',
    Reject: 'requests.reject',
  },
  Audit: {
    View: 'audit.view',
  },
} as const;
