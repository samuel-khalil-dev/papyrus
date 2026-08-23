import { useCallback, useSyncExternalStore } from 'react';
import { authStore } from '../lib/auth-store';
export function useAuth() { const apiKey = useSyncExternalStore(() => () => undefined, authStore.getApiKey, () => ''); const setApiKey = useCallback((value: string) => authStore.setApiKey(value), []); const clearApiKey = useCallback(() => authStore.clearApiKey(), []); return { isAuthenticated: Boolean(apiKey), apiKey, setApiKey, clearApiKey }; }
