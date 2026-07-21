export type ReportKey = 'attendance' | 'employees' | 'companies' | 'branches' | 'subscriptions' | 'plan-usage';

export interface ReportRequest {
  page: number;
  pageSize: number;
  keyword?: string;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  companyId?: string;
  branchId?: string;
  employeeId?: string;
  scheduleId?: string;
  from?: string;
  to?: string;
  status?: string;
  role?: string;
}

export interface ReportRowBase {
  id: string;
  [key: string]: unknown;
}

export interface ReportDefinition {
  key: ReportKey;
  endpoint: string;
  audience: 'platform' | 'tenant';
  titleKey: string;
  descriptionKey: string;
  emptyKey: string;
  defaultSortBy: string;
  columns: Array<{ key: string; headerKey: string; type?: 'text' | 'date' | 'datetime' | 'boolean' | 'number' | 'currency' | 'status' }>;
  statusOptions?: Array<{ labelKey: string; value: string }>;
  supportsEmployeeFilter?: boolean;
}



