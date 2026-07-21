import type { ReportKey, ReportRequest } from 'features/reports/types/reportTypes';

type QueryFilters = object;

const cleanFilters = <T extends QueryFilters>(filters?: T) => filters ?? {};

export const queryKeys = {
  dashboard: {
    all: ['dashboard'] as const,
    stats: () => [...queryKeys.dashboard.all, 'stats'] as const,
  },
  companies: {
    all: ['companies'] as const,
    list: (filters?: QueryFilters) => [...queryKeys.companies.all, 'list', cleanFilters(filters)] as const,
    details: (id: string) => [...queryKeys.companies.all, 'details', id] as const,
  },
  branches: {
    all: ['branches'] as const,
    list: (filters?: QueryFilters) => [...queryKeys.branches.all, 'list', cleanFilters(filters)] as const,
    details: (id: string) => [...queryKeys.branches.all, 'details', id] as const,
  },
  employees: {
    all: ['employees'] as const,
    list: (filters?: QueryFilters) => [...queryKeys.employees.all, 'list', cleanFilters(filters)] as const,
    details: (id: string) => [...queryKeys.employees.all, 'details', id] as const,
  },
  attendance: {
    all: ['attendance'] as const,
    list: (filters?: QueryFilters) => [...queryKeys.attendance.all, 'list', cleanFilters(filters)] as const,
    today: () => [...queryKeys.attendance.all, 'today'] as const,
  },
  plans: {
    all: ['plans'] as const,
    list: (filters?: QueryFilters) => [...queryKeys.plans.all, 'list', cleanFilters(filters)] as const,
    details: (id: string) => [...queryKeys.plans.all, 'details', id] as const,
  },
  subscriptions: {
    all: ['subscriptions'] as const,
    list: (filters?: QueryFilters) => [...queryKeys.subscriptions.all, 'list', cleanFilters(filters)] as const,
  },
  users: {
    all: ['users'] as const,
    list: (filters?: QueryFilters) => [...queryKeys.users.all, 'list', cleanFilters(filters)] as const,
    details: (id: string) => [...queryKeys.users.all, 'details', id] as const,
    current: () => [...queryKeys.users.all, 'current'] as const,
  },
  roles: {
    all: ['roles'] as const,
    list: (filters?: QueryFilters) => [...queryKeys.roles.all, 'list', cleanFilters(filters)] as const,
    details: (id: string) => [...queryKeys.roles.all, 'details', id] as const,
  },
  permissions: {
    all: ['permissions'] as const,
    list: (filters?: QueryFilters) => [...queryKeys.permissions.all, 'list', cleanFilters(filters)] as const,
    details: (id: string) => [...queryKeys.permissions.all, 'details', id] as const,
  },
  reports: {
    all: ['reports'] as const,
    report: (reportKey: ReportKey, filters?: ReportRequest) => [...queryKeys.reports.all, reportKey, cleanFilters(filters)] as const,
  },
} as const;

