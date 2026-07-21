import type { QueryClient } from '@tanstack/react-query';
import { queryKeys } from './queryKeys';

export type MutationResource =
  | 'companies'
  | 'branches'
  | 'employees'
  | 'attendance'
  | 'plans'
  | 'subscriptions'
  | 'users'
  | 'roles'
  | 'permissions'
  | 'reports'
  | 'dashboard';

const dependencyMap: Record<MutationResource, MutationResource[]> = {
  companies: ['companies', 'branches', 'employees', 'subscriptions', 'dashboard', 'reports'],
  branches: ['branches', 'employees', 'attendance', 'dashboard', 'reports'],
  employees: ['employees', 'attendance', 'users', 'dashboard', 'reports'],
  attendance: ['attendance', 'employees', 'dashboard', 'reports'],
  plans: ['plans', 'companies', 'subscriptions', 'dashboard', 'reports'],
  subscriptions: ['subscriptions', 'companies', 'dashboard', 'reports'],
  users: ['users', 'employees', 'dashboard', 'reports'],
  roles: ['roles', 'permissions', 'users', 'dashboard'],
  permissions: ['permissions', 'roles', 'users', 'dashboard'],
  reports: ['reports'],
  dashboard: ['dashboard'],
};

const keyByResource: Record<MutationResource, readonly unknown[]> = {
  companies: queryKeys.companies.all,
  branches: queryKeys.branches.all,
  employees: queryKeys.employees.all,
  attendance: queryKeys.attendance.all,
  plans: queryKeys.plans.all,
  subscriptions: queryKeys.subscriptions.all,
  users: queryKeys.users.all,
  roles: queryKeys.roles.all,
  permissions: queryKeys.permissions.all,
  reports: queryKeys.reports.all,
  dashboard: queryKeys.dashboard.all,
};

export async function invalidateMutationResources(queryClient: QueryClient, resource: MutationResource) {
  const resources = dependencyMap[resource] ?? [resource];
  await Promise.all(resources.map((item) => queryClient.invalidateQueries({ queryKey: keyByResource[item] })));
}
