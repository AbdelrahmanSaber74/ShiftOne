import client from 'shared/services/apiClient';
import { API_ENDPOINTS } from 'shared/constants/apiEndpoints';
import { normalizeParams, QueryParams } from 'shared/utils/apiUtils';
import type { ApiResponse, Company } from 'shared/types/api';

const companiesService = {
  getCompanies: async (params: QueryParams = {}): Promise<ApiResponse<Company[]>> => {
    const response = await client.get<ApiResponse<Company[]>>(API_ENDPOINTS.COMPANIES, {
      params: normalizeParams(params),
    });
    return response.data;
  },
  saveCompany: async (payload: Partial<Company> & Record<string, unknown>, id?: string): Promise<ApiResponse<Company>> => {
    const response = id
      ? await client.put<ApiResponse<Company>>(`${API_ENDPOINTS.COMPANIES}/${id}`, payload)
      : await client.post<ApiResponse<Company>>(API_ENDPOINTS.COMPANIES, payload);
    return response.data;
  },
  deleteCompany: async (id: string): Promise<ApiResponse> => {
    const response = await client.delete<ApiResponse>(`${API_ENDPOINTS.COMPANIES}/${id}`);
    return response.data;
  },
};

export default companiesService;
