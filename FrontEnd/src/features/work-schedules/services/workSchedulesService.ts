import client from 'shared/services/apiClient';
import { API_ENDPOINTS } from 'shared/constants/apiEndpoints';
import { normalizeParams, QueryParams } from 'shared/utils/apiUtils';
import type { ApiResponse } from 'shared/types/api';
import type { WorkSchedule } from '../types';

const workSchedulesService = {
  getWorkSchedules: async (params: QueryParams = {}): Promise<ApiResponse<WorkSchedule[]>> => {
    const response = await client.get<ApiResponse<WorkSchedule[]>>(API_ENDPOINTS.WORK_SCHEDULES, { params: normalizeParams(params) });
    return response.data;
  },
  saveWorkSchedule: async (payload: Partial<WorkSchedule>, id?: string): Promise<ApiResponse<WorkSchedule>> => {
    const response = id
      ? await client.put<ApiResponse<WorkSchedule>>(`${API_ENDPOINTS.WORK_SCHEDULES}/${id}`, payload)
      : await client.post<ApiResponse<WorkSchedule>>(API_ENDPOINTS.WORK_SCHEDULES, payload);
    return response.data;
  },
  deleteWorkSchedule: async (id: string): Promise<ApiResponse> => {
    const response = await client.delete<ApiResponse>(`${API_ENDPOINTS.WORK_SCHEDULES}/${id}`);
    return response.data;
  },
  setDefault: async (id: string): Promise<ApiResponse<WorkSchedule>> => {
    const response = await client.post<ApiResponse<WorkSchedule>>(`${API_ENDPOINTS.WORK_SCHEDULES}/${id}/set-default`);
    return response.data;
  },
};

export default workSchedulesService;
