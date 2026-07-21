import client from 'shared/services/apiClient';
import { tokenStorage } from 'shared/services/tokenStorage';
import { API_ENDPOINTS } from 'shared/constants/apiEndpoints';
import type { ApiResponse, AuthTokens, UserProfile } from 'shared/types/api';

const authService = {
  login: async (emailOrPhone: string, password: string, isAdmin = false): Promise<ApiResponse<AuthTokens>> => {
    const url = isAdmin ? API_ENDPOINTS.ADMIN.LOGIN : API_ENDPOINTS.AUTH.LOGIN;
    const response = await client.post<ApiResponse<AuthTokens>>(url, { emailOrPhone, password });

    if (response.data.success && response.data.data) {
      const { accessToken, refreshToken } = response.data.data;
      tokenStorage.setSession({ accessToken, refreshToken, isAdmin });
    }

    return response.data;
  },

  logout: (): void => {
    tokenStorage.clear();
    window.location.href = '/auth/sign-in';
  },

  getCurrentUser: async (isAdmin = tokenStorage.isAdminSession()): Promise<ApiResponse<UserProfile>> => {
    const url = isAdmin ? API_ENDPOINTS.ADMIN.GET_INFO : API_ENDPOINTS.AUTH.GET_PROFILE;
    const response = await client.get<ApiResponse<UserProfile>>(url);
    return response.data;
  },
};

export default authService;
