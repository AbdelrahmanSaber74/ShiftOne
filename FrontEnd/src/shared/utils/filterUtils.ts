export type StatusFilterValue = 'all' | 'active' | 'inactive';

export function statusFilterToBoolean(value: StatusFilterValue): boolean | null {
  if (value === 'active') return true;
  if (value === 'inactive') return false;
  return null;
}

export function booleanToStatusFilter(value: boolean | null | undefined): StatusFilterValue {
  if (value === true) return 'active';
  if (value === false) return 'inactive';
  return 'all';
}

export const defaultStatusOptions = [
  { label: 'common.allStatus', value: 'all' },
  { label: 'common.active', value: 'active' },
  { label: 'common.inactive', value: 'inactive' },
];

