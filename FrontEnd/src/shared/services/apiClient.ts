import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
import { API_BASE_URL, API_ENDPOINTS } from 'shared/constants/apiEndpoints';
import { tokenStorage } from 'shared/services/tokenStorage';
import { tokenHasPlatformRole } from 'shared/utils/authUtils';
import type { ApiResponse, AuthTokens } from 'shared/types/api';

interface RetryRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean;
}

const client = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

client.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = tokenStorage.getAccessToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    const activeCompanyId = localStorage.getItem('active_company_id');
    if (activeCompanyId && activeCompanyId !== 'all') {
      config.headers['X-Company-Id'] = activeCompanyId;
    }
    if (config.params && (config.params.refresh === true || config.params.refresh === 'true')) {
      config.headers['Cache-Control'] = 'no-cache';
      delete config.params.refresh;
    }
    return config;
  },
  (error: AxiosError) => Promise.reject(error),
);

client.interceptors.response.use(
  (response) => response,
  async (error: AxiosError<ApiResponse>) => {
    const originalRequest = error.config as RetryRequestConfig | undefined;
    const requestUrl = originalRequest?.url?.toLowerCase() || '';
    const isLoginEndpoint = requestUrl.includes('/login') || requestUrl.includes('/sign-in');

    if (error.response?.status !== 401 || !originalRequest || originalRequest._retry || isLoginEndpoint) {
      return Promise.reject(error);
    }

    originalRequest._retry = true;

    try {
      const refreshToken = tokenStorage.getRefreshToken();
      if (!refreshToken) {
        throw new Error('No refresh token available');
      }

      const refreshUrl = tokenStorage.isAdminSession()
        ? API_ENDPOINTS.ADMIN.REFRESH
        : API_ENDPOINTS.AUTH.REFRESH;
      const response = await axios.post<ApiResponse<AuthTokens>>(`${API_BASE_URL}${refreshUrl}`, { refreshToken });
      const tokens = response.data.data;

      if (!tokens) {
        throw new Error('Refresh response did not include tokens');
      }

      tokenStorage.setTokens(tokens);
      originalRequest.headers.Authorization = `Bearer ${tokens.accessToken}`;
      return client(originalRequest);
    } catch (refreshError) {
      tokenStorage.clear();
      if (!window.location.pathname.toLowerCase().includes('/auth/sign-in')) {
        window.location.href = '/auth/sign-in';
      }
      return Promise.reject(refreshError);
    }
  },
);

export default client;


