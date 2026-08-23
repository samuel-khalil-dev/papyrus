import { useCallback, useSyncExternalStore } from 'react';
import { authStore } from '../lib/auth-store';
export function useTenant() { const tenantId = useSyncExternalStore(() => () => undefined, authStore.getTenantId, () => 'main'); const setTenantId = useCallback((value: string) => authStore.setTenantId(value), []); return { tenantId, setTenantId }; }
