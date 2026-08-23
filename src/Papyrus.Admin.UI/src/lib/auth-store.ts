const API_KEY = 'papyrus.admin.api-key';
const TENANT_ID = 'papyrus.admin.tenant-id';
export const authStore = { getApiKey: () => localStorage.getItem(API_KEY) ?? 'dev-admin-key', setApiKey: (value: string) => localStorage.setItem(API_KEY, value), clearApiKey: () => localStorage.removeItem(API_KEY), getTenantId: () => localStorage.getItem(TENANT_ID) ?? 'main', setTenantId: (value: string) => localStorage.setItem(TENANT_ID, value) };
