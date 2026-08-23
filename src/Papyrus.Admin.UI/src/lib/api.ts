import createClient from 'openapi-fetch';
import type { paths } from '../api/schema';
import { authStore } from './auth-store';

export const api = createClient<paths>({
  baseUrl: '/api/v1',
  fetch: async (input: Request) => {
    const headers = new Headers(input.headers);
    headers.set('X-Admin-Api-Key', authStore.getApiKey());
    headers.set('X-Tenant-Id', authStore.getTenantId());
    const response = await fetch(input, { headers });
    if ([401, 403, 429].includes(response.status)) {
      window.dispatchEvent(new CustomEvent('papyrus:api-error', { detail: { status: response.status } }));
    }
    return response;
  },
});
