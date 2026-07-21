import client from 'shared/services/apiClient';
import { API_ENDPOINTS } from 'shared/constants/apiEndpoints';
import { normalizeParams, QueryParams } from 'shared/utils/apiUtils';
import type { ApiResponse, Role } from 'shared/types/api';

export interface SaveRolePayload {
  name: string;
  description: string;
  isActive: boolean;
}

const rolesService = {
  getRoles: async (params: QueryParams = {}): Promise<ApiResponse<Role[]>> => {
    const response = await client.get<ApiResponse<Role[]>>(API_ENDPOINTS.ROLES, { params: normalizeParams(params) });
    return response.data;
  },
  saveRole: async (payload: SaveRolePayload, id?: string): Promise<ApiResponse<Role>> => {
    const response = id
      ? await client.put<ApiResponse<Role>>(`${API_ENDPOINTS.ROLES}/${id}`, payload)
      : await client.post<ApiResponse<Role>>(API_ENDPOINTS.ROLES, payload);
    return response.data;
  },
  deleteRole: async (id: string): Promise<ApiResponse> => {
    const response = await client.delete<ApiResponse>(`${API_ENDPOINTS.ROLES}/${id}`);
    return response.data;
  },
  assignPermissions: async (id: string, permissionIds: string[]): Promise<ApiResponse<Role>> => {
    const response = await client.put<ApiResponse<Role>>(`${API_ENDPOINTS.ROLES}/${id}/permissions`, { permissionIds });
    return response.data;
  },
};

export default rolesService;