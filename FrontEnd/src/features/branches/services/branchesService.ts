import client from 'shared/services/apiClient';
import { API_ENDPOINTS } from 'shared/constants/apiEndpoints';
import { normalizeParams, QueryParams } from 'shared/utils/apiUtils';
import type { ApiResponse, Branch } from 'shared/types/api';

const branchesService = {
  getBranches: async (params: QueryParams = {}): Promise<ApiResponse<Branch[]>> => {
    const response = await client.get<ApiResponse<Branch[]>>(API_ENDPOINTS.BRANCHES, {
      params: normalizeParams(params),
    });
    return response.data;
  },
  saveBranch: async (payload: Partial<Branch>, id?: string): Promise<ApiResponse<Branch>> => {
    const response = id
      ? await client.put<ApiResponse<Branch>>(`${API_ENDPOINTS.BRANCHES}/${id}`, payload)
      : await client.post<ApiResponse<Branch>>(API_ENDPOINTS.BRANCHES, payload);
    return response.data;
  },
  deleteBranch: async (id: string): Promise<ApiResponse> => {
    const response = await client.delete<ApiResponse>(`${API_ENDPOINTS.BRANCHES}/${id}`);
    return response.data;
  },
};

export default branchesService;
