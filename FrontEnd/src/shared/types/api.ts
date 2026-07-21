export interface ApiResponse<T = unknown> {
  success: boolean;
  message?: string;
  data?: T;
  statusCode?: number;
  page?: number;
  pageSize?: number;
  totalCount?: number;
  totalPages?: number;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
}

export interface UserProfile {
  id: string;
  companyId?: string | null;
  firstName: string;
  lastName: string;
  email?: string;
  emailConfirmed?: boolean;
  phoneNumber?: string;
  phoneConfirmed?: boolean;
  imagePath?: string | null;
  isActive?: boolean;
  isLockedOut?: boolean;
  lockoutEnd?: string | null;
  createdOn?: string;
  roles?: string[];
  permissions?: string[];
  isProtected?: boolean;
}

export interface UsersListResponse {
  users: UserProfile[];
}

export interface LoginResult {
  success: boolean;
  message?: string;
}

export interface SubscriptionPlan {
  id: string;
  name: string;
  description: string;
  price: number;
  billingPeriod: string;
  isActive: boolean;
  maxBranches?: number | null;
  maxEmployees?: number | null;
  maxHRUsers?: number | null;
  maxCompanyAdmins?: number | null;
}

export interface Company {
  id: string;
  name: string;
  code: string;
  email?: string | null;
  phoneNumber?: string | null;
  address?: string | null;
  isActive: boolean;
  currentPlanName?: string | null;
  currentPlanId?: string | null;
}

export interface CompanySubscription {
  id: string;
  companyId: string;
  companyName: string;
  planId: string;
  planName: string;
  startsOn: string;
  endsOn?: string | null;
  isActive: boolean;
}

export interface Branch {
  id: string;
  companyId: string;
  name: string;
  code: string;
  address: string;
  latitude: number;
  longitude: number;
  allowedRadius: number;
  isMainBranch: boolean;
  isActive: boolean;
  workScheduleId?: string | null;
  workScheduleName?: string | null;
  usesCompanyDefaultSchedule?: boolean;
}

export interface Employee {
  id: string;
  companyId?: string | null;
  branchId?: string | null;
  branchName?: string | null;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  isActive: boolean;
  roles: string[];
  hasBoundDevice: boolean;
}

export interface AttendanceRecord {
  id: string;
  employeeId: string;
  employeeName: string;
  branchId: string;
  branchName: string;
  attendanceDate: string;
  checkInAt: string;
  checkOutAt?: string | null;
  status?: string;
  workScheduleId?: string | null;
  workScheduleName?: string | null;
  workedMinutes?: number | null;
  lateMinutes?: number;
  earlyLeaveMinutes?: number;
  overtimeMinutes?: number;
}

export interface Permission {
  id: string;
  name: string;
  description: string;
  isSystemPermission: boolean;
  createdOn: string;
}

export interface Role {
  id: string;
  name: string;
  description: string;
  isActive: boolean;
  isSystemRole: boolean;
  isProtected: boolean;
  permissions: string[];
  createdOn: string;
}