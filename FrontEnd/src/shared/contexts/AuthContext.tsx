import { createContext, ReactNode, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { AxiosError } from 'axios';
import authService from 'features/auth/services/authService';
import { tokenStorage } from 'shared/services/tokenStorage';
import type { ApiResponse, LoginResult, UserProfile } from 'shared/types/api';
import { isPlatformAdmin } from 'shared/utils/authUtils';

interface AuthContextValue {
  user: UserProfile | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (emailOrPhone: string, password: string) => Promise<LoginResult>;
  logout: () => void;
  refreshProfile: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

interface AuthProviderProps {
  children: ReactNode;
}

function getErrorMessage(error: unknown, fallback: string): string {
  const axiosError = error as AxiosError<ApiResponse>;
  return axiosError.response?.data?.message || fallback;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<UserProfile | null>(null);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  const fetchProfile = useCallback(async () => {
    try {
      const accessToken = tokenStorage.getAccessToken();
      if (!accessToken) {
        setUser(null);
        setIsAuthenticated(false);
        return;
      }

      const response = await authService.getCurrentUser(tokenStorage.isAdminSession());
      if (response.success && response.data) {
        if (!isPlatformAdmin(response.data)) {
          localStorage.removeItem('active_company_id');
        }
        setUser(response.data);
        setIsAuthenticated(true);
      } else {
        setUser(null);
        setIsAuthenticated(false);
      }
    } catch (error) {
      console.error('Failed to load user profile:', error);
      setUser(null);
      setIsAuthenticated(false);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void fetchProfile();
  }, [fetchProfile]);

  const login = useCallback(async (emailOrPhone: string, password: string): Promise<LoginResult> => {
    // First try Admin login
    try {
      const result = await authService.login(emailOrPhone, password, true);
      if (result.success) {
        await fetchProfile();
        return { success: true };
      }
    } catch (adminError) {
      // Admin login failed or unauthorized, we proceed to try employee login
      console.log('Admin login attempt failed, trying employee login...');
    }

    // Fallback to Employee login
    try {
      const result = await authService.login(emailOrPhone, password, false);
      if (result.success) {
        await fetchProfile();
        return { success: true };
      }
      return { success: false, message: result.message || 'Login failed' };
    } catch (error) {
      return {
        success: false,
        message: getErrorMessage(error, 'Login failed. Please check credentials.'),
      };
    }
  }, [fetchProfile]);

  const logout = useCallback(() => {
    authService.logout();
    setUser(null);
    setIsAuthenticated(false);
  }, []);

  const value = useMemo<AuthContextValue>(() => ({
    user,
    isAuthenticated,
    isLoading,
    login,
    logout,
    refreshProfile: fetchProfile,
  }), [fetchProfile, isAuthenticated, isLoading, login, logout, user]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}

