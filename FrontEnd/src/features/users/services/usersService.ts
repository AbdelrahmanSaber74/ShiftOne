import client from 'shared/services/apiClient';
import { API_ENDPOINTS } from 'shared/constants/apiEndpoints';
import type { ApiResponse, UsersListResponse } from 'shared/types/api';

export interface UsersQuery {
  page?: number;
  pageSize?: number;
  keyword?: string;
  isActive?: boolean | null;
}

const usersService = {
  getUsers: async ({ page = 1, pageSize = 10, keyword = '', isActive = null }: UsersQuery = {}): Promise<ApiResponse<UsersListResponse>> => {
    const response = await client.get<ApiResponse<UsersListResponse>>(API_ENDPOINTS.ADMIN.GET_ALL_USERS, {
      params: {
        page,
        pageSize,
        keyword: keyword || undefined,
        isActive: isActive === null ? undefined : isActive,
      },
    });
    return response.data;
  },

  approveUser: async (userId: string): Promise<ApiResponse> => {
    const response = await client.put<ApiResponse>(API_ENDPOINTS.ADMIN.APPROVE_USER, null, {
      params: { userId },
    });
    return response.data;
  },

  unapproveUser: async (userId: string): Promise<ApiResponse> => {
    const response = await client.put<ApiResponse>(API_ENDPOINTS.ADMIN.UNAPPROVE_USER, null, {
      params: { userId },
    });
    return response.data;
  },
};

export default usersService;
