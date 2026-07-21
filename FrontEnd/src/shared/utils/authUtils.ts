import type { UserProfile } from 'shared/types/api';

const platformRoles = new Set(['SuperAdmin', 'Admin']);

export function isPlatformAdmin(user?: Pick<UserProfile, 'roles'> | null): boolean {
  return !!user?.roles?.some((role) => platformRoles.has(role));
}

export function hasPermission(user: Pick<UserProfile, 'roles' | 'permissions'> | null | undefined, permission: string): boolean {
  if (isPlatformAdmin(user)) return true;
  return !!user?.permissions?.includes(permission);
}

export function hasAnyPermission(user: Pick<UserProfile, 'roles' | 'permissions'> | null | undefined, permissions: string[]): boolean {
  if (isPlatformAdmin(user)) return true;
  return permissions.some((permission) => hasPermission(user, permission));
}

export function tokenHasPlatformRole(token: string | null): boolean {
  if (!token) return false;

  try {
    const payload = JSON.parse(window.atob(token.split('.')[1] || '')) as Record<string, unknown>;
    const roleValues = [
      payload.role,
      payload.roles,
      payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'],
    ].flatMap((value) => Array.isArray(value) ? value : value ? [value] : []);

    return roleValues.some((role) => typeof role === 'string' && platformRoles.has(role));
  } catch {
    return false;
  }
}
