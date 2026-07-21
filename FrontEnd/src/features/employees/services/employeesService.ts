import client from 'shared/services/apiClient';
import { API_ENDPOINTS } from 'shared/constants/apiEndpoints';
import { normalizeParams, QueryParams } from 'shared/utils/apiUtils';
import type { ApiResponse, Employee } from 'shared/types/api';

const employeesService = {
  getEmployees: async (params: QueryParams = {}): Promise<ApiResponse<Employee[]>> => {
    const response = await client.get<ApiResponse<Employee[]>>(API_ENDPOINTS.EMPLOYEES, {
      params: normalizeParams(params),
    });
    return response.data;
  },
  saveEmployee: async (payload: Record<string, unknown>, id?: string): Promise<ApiResponse<Employee>> => {
    const response = id
      ? await client.put<ApiResponse<Employee>>(`${API_ENDPOINTS.EMPLOYEES}/${id}`, payload)
      : await client.post<ApiResponse<Employee>>(API_ENDPOINTS.EMPLOYEES, payload);
    return response.data;
  },
  deleteEmployee: async (id: string): Promise<ApiResponse> => {
    const response = await client.delete<ApiResponse>(`${API_ENDPOINTS.EMPLOYEES}/${id}`);
    return response.data;
  },
  resetDevice: async (employeeId: string): Promise<ApiResponse> => {
    const response = await client.post<ApiResponse>(`${API_ENDPOINTS.EMPLOYEES}/reset-device`, { employeeId });
    return response.data;
  },
};

export default employeesService;
