import { STORAGE_KEYS } from 'shared/constants/storageKeys';
import type { AuthTokens } from 'shared/types/api';

interface SessionTokens extends AuthTokens {
  isAdmin: boolean;
}

export const tokenStorage = {
  getAccessToken: (): string | null => localStorage.getItem(STORAGE_KEYS.accessToken),
  getRefreshToken: (): string | null => localStorage.getItem(STORAGE_KEYS.refreshToken),
  isAdminSession: (): boolean => localStorage.getItem(STORAGE_KEYS.isAdmin) === 'true',
  setSession: ({ accessToken, refreshToken, isAdmin }: SessionTokens): void => {
    localStorage.setItem(STORAGE_KEYS.accessToken, accessToken);
    localStorage.setItem(STORAGE_KEYS.refreshToken, refreshToken);
    localStorage.setItem(STORAGE_KEYS.isAdmin, String(isAdmin));
  },
  setTokens: ({ accessToken, refreshToken }: AuthTokens): void => {
    localStorage.setItem(STORAGE_KEYS.accessToken, accessToken);
    localStorage.setItem(STORAGE_KEYS.refreshToken, refreshToken);
  },
  clear: (): void => {
    localStorage.removeItem(STORAGE_KEYS.accessToken);
    localStorage.removeItem(STORAGE_KEYS.refreshToken);
    localStorage.removeItem(STORAGE_KEYS.isAdmin);
  },
};
