import client from 'shared/services/apiClient';
import { API_ENDPOINTS } from 'shared/constants/apiEndpoints';
import { normalizeParams, QueryParams } from 'shared/utils/apiUtils';
import type { ApiResponse, Permission } from 'shared/types/api';

export interface SavePermissionPayload {
  name: string;
  description: string;
}

const permissionsService = {
  getPermissions: async (params: QueryParams = {}): Promise<ApiResponse<Permission[]>> => {
    const response = await client.get<ApiResponse<Permission[]>>(API_ENDPOINTS.PERMISSIONS, { params: normalizeParams(params) });
    return response.data;
  },
  savePermission: async (payload: SavePermissionPayload, id?: string): Promise<ApiResponse<Permission>> => {
    const response = id
      ? await client.put<ApiResponse<Permission>>(`${API_ENDPOINTS.PERMISSIONS}/${id}`, payload)
      : await client.post<ApiResponse<Permission>>(API_ENDPOINTS.PERMISSIONS, payload);
    return response.data;
  },
  deletePermission: async (id: string): Promise<ApiResponse> => {
    const response = await client.delete<ApiResponse>(`${API_ENDPOINTS.PERMISSIONS}/${id}`);
    return response.data;
  },
};

export default permissionsService;