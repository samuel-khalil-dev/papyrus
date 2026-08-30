import { useSyncExternalStore } from 'react';
import { authStore } from '../lib/auth-store';
export function useAuth() { const apiKey = useSyncExternalStore(() => () => undefined, authStore.getApiKey, () => ''); const setApiKey = (value: string) => authStore.setApiKey(value); const clearApiKey = () => authStore.clearApiKey(); return { isAuthenticated: Boolean(apiKey), apiKey, setApiKey, clearApiKey }; }
