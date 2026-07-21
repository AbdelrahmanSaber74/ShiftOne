export interface QueryParams {
  page?: number;
  pageSize?: number;
  keyword?: string;
  isActive?: boolean | null;
  branchId?: string;
  companyId?: string;
  employeeId?: string;
  from?: string;
  to?: string;
  refresh?: boolean | string;
}

export function normalizeParams(params: QueryParams = {}) {
  return Object.fromEntries(
    Object.entries(params).filter(
      ([, value]) => value !== undefined && value !== null && value !== ''
    )
  );
}
