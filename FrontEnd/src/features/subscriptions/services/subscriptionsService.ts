import client from 'shared/services/apiClient';
import { API_ENDPOINTS } from 'shared/constants/apiEndpoints';
import { normalizeParams, QueryParams } from 'shared/utils/apiUtils';
import type { ApiResponse, CompanySubscription } from 'shared/types/api';

const subscriptionsService = {
  getSubscriptions: async (params: QueryParams = {}): Promise<ApiResponse<CompanySubscription[]>> => {
    const response = await client.get<ApiResponse<CompanySubscription[]>>(API_ENDPOINTS.SUBSCRIPTIONS, {
      params: normalizeParams(params),
    });
    return response.data;
  },
  assignSubscription: async (payload: { companyId: string; planId: string; startsOn?: string; endsOn?: string | null; isActive?: boolean }): Promise<ApiResponse> => {
    const response = await client.post<ApiResponse>(API_ENDPOINTS.SUBSCRIPTIONS, payload);
    return response.data;
  },
};

export default subscriptionsService;
