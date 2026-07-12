/**
 * Default (production) environment. No separate prod backend is configured for this task,
 * so it points at the same local API. Override per-config via fileReplacements.
 */
export const environment = {
  production: true,
  apiBaseUrl: 'http://localhost:5184',
};
