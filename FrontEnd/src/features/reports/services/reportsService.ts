import client from 'shared/services/apiClient';
import { API_ENDPOINTS } from 'shared/constants/apiEndpoints';
import type { ApiResponse } from 'shared/types/api';
import type { ReportKey, ReportRequest, ReportRowBase } from '../types/reportTypes';
import { getReportDefinition } from '../constants/reportDefinitions';

const cleanParams = (request: ReportRequest) => Object.fromEntries(
  Object.entries(request).filter(([, value]) => value !== undefined && value !== null && value !== '')
);

const reportsService = {
  getReport: async (reportKey: ReportKey, request: ReportRequest): Promise<ApiResponse<ReportRowBase[]>> => {
    const definition = getReportDefinition(reportKey);
    const response = await client.get<ApiResponse<ReportRowBase[]>>(`${API_ENDPOINTS.REPORTS}/${definition.endpoint}`, {
      params: cleanParams(request),
    });
    return response.data;
  },

  exportReport: async (reportKey: ReportKey, request: ReportRequest, format = 'xlsx'): Promise<Blob> => {
    const response = await client.get(`${API_ENDPOINTS.REPORTS}/${reportKey}/export`, {
      params: { ...cleanParams(request), format },
      responseType: 'blob',
    });
    return response.data as Blob;
  },
};

export default reportsService;
