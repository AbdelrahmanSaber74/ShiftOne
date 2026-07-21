import type { UseFormSetError } from 'react-hook-form';

export function normalizeEmptyToNull<T extends Record<string, unknown>>(values: T, keys: Array<keyof T>) {
  return keys.reduce((next, key) => {
    if (next[key] === '') next[key] = null as T[keyof T];
    return next;
  }, { ...values });
}

export function mapBackendFieldErrors<T extends Record<string, unknown>>(
  errors: Record<string, string[]> | undefined,
  setError: UseFormSetError<T>,
  fieldMap: Record<string, keyof T>,
) {
  if (!errors) return false;
  let mapped = false;
  Object.entries(errors).forEach(([apiField, messages]) => {
    const key = fieldMap[apiField] || fieldMap[apiField.toLowerCase()];
    if (key && messages[0]) {
      setError(key as any, { type: 'server', message: messages[0] });
      mapped = true;
    }
  });
  return mapped;
}
