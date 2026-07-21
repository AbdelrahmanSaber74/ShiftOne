import client from 'shared/services/apiClient';
import { API_ENDPOINTS } from 'shared/constants/apiEndpoints';
import { normalizeParams, QueryParams } from 'shared/utils/apiUtils';
import type { ApiResponse, SubscriptionPlan } from 'shared/types/api';

const plansService = {
  getPlans: async (params: QueryParams = {}): Promise<ApiResponse<SubscriptionPlan[]>> => {
    const response = await client.get<ApiResponse<SubscriptionPlan[]>>(API_ENDPOINTS.PLANS, {
      params: normalizeParams(params),
    });
    return response.data;
  },
  savePlan: async (payload: Partial<SubscriptionPlan>, id?: string): Promise<ApiResponse<SubscriptionPlan>> => {
    const response = id
      ? await client.put<ApiResponse<SubscriptionPlan>>(`${API_ENDPOINTS.PLANS}/${id}`, payload)
      : await client.post<ApiResponse<SubscriptionPlan>>(API_ENDPOINTS.PLANS, payload);
    return response.data;
  },
  deletePlan: async (id: string): Promise<ApiResponse> => {
    const response = await client.delete<ApiResponse>(`${API_ENDPOINTS.PLANS}/${id}`);
    return response.data;
  },
};

export default plansService;
