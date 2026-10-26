/**
 * Every API call goes to /api. In development, the Angular dev server's proxy forwards it to the API
 * and strips the prefix (D18), so the browser sees one origin and the API needs no CORS policy.
 */
export const API_BASE = '/api';
