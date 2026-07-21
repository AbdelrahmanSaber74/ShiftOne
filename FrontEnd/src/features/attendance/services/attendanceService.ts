import client from 'shared/services/apiClient';
import { API_ENDPOINTS } from 'shared/constants/apiEndpoints';
import { normalizeParams, QueryParams } from 'shared/utils/apiUtils';
import type { ApiResponse, AttendanceRecord } from 'shared/types/api';

const attendanceService = {
  getAttendance: async (params: QueryParams = {}): Promise<ApiResponse<AttendanceRecord[]>> => {
    const response = await client.get<ApiResponse<AttendanceRecord[]>>(API_ENDPOINTS.ATTENDANCE, {
      params: normalizeParams(params),
    });
    return response.data;
  },
};

export default attendanceService;
