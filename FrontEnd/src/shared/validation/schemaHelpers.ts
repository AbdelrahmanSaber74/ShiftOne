import { z } from 'zod';

export type Translate = (key: string, options?: Record<string, unknown>) => string;

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const phonePattern = /^\+?[0-9\s()-]{7,30}$/;
const guidPattern = /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/;

export const emptyToUndefined = (value: unknown) => (typeof value === 'string' && value.trim() === '' ? undefined : value);
export const trimString = (value: unknown) => (typeof value === 'string' ? value.trim() : value);

export const requiredText = (t: Translate, max: number, requiredKey = 'validation:validation.required') =>
  z.preprocess(trimString, z.string({ error: t(requiredKey) }).min(1, t(requiredKey)).max(max, t('validation:validation.maxLength', { count: max })));

export const optionalText = (t: Translate, max: number) =>
  z.preprocess(emptyToUndefined, z.string().trim().max(max, t('validation:validation.maxLength', { count: max })).optional());

export const optionalEmail = (t: Translate) =>
  z.preprocess(emptyToUndefined, z.string().trim().email(t('validation:validation.email')).max(256, t('validation:validation.maxLength', { count: 256 })).optional());

export const requiredEmail = (t: Translate) =>
  z.preprocess(trimString, z.string({ error: t('validation:validation.required') }).min(1, t('validation:validation.required')).email(t('validation:validation.email')).max(256, t('validation:validation.maxLength', { count: 256 })));

export const optionalPhone = (t: Translate, max = 30) =>
  z.preprocess(emptyToUndefined, z.string().trim().max(max, t('validation:validation.maxLength', { count: max })).regex(phonePattern, t('validation:validation.phone')).optional());

export const requiredPassword = (t: Translate) =>
  z.string({ error: t('validation:validation.required') })
    .min(8, t('validation:validation.minLength', { count: 8 }))
    .max(50, t('validation:validation.maxLength', { count: 50 }));

export const optionalPassword = (t: Translate) =>
  z.preprocess(emptyToUndefined, z.string().min(8, t('validation:validation.minLength', { count: 8 })).max(50, t('validation:validation.maxLength', { count: 50 })).optional());

export const requiredSelect = (t: Translate) =>
  z.string({ error: t('validation:validation.selectRequired') }).min(1, t('validation:validation.selectRequired'));

export const optionalGuid = (t?: Translate) =>
  z.union([
    z.string().regex(guidPattern, { message: t ? t('validation:validation.invalid') : 'Invalid GUID' }),
    z.literal(''),
    z.null(),
    z.undefined()
  ]).transform((val) => (val === '' || val === null ? undefined : val));

export const requiredNumber = (t: Translate, min: number, max: number) =>
  z.coerce.number({ error: t('validation:validation.number') })
    .min(min, t('validation:validation.numberRange', { min, max }))
    .max(max, t('validation:validation.numberRange', { min, max }));

export const requiredInteger = (t: Translate, min: number, max: number) =>
  z.coerce.number({ error: t('validation:validation.number') })
    .int(t('validation:validation.integer'))
    .min(min, t('validation:validation.numberRange', { min, max }))
    .max(max, t('validation:validation.numberRange', { min, max }));

export const nullablePositiveInteger = (t: Translate) =>
  z.preprocess(emptyToUndefined, z.coerce.number({ error: t('validation:validation.number') }).int(t('validation:validation.integer')).min(1, t('validation:validation.positiveNumber')).optional().nullable());

export const booleanField = () => z.boolean().default(true);

export const emailOrPhone = (t: Translate) =>
  z.preprocess(trimString, z.string({ error: t('validation:validation.required') })
    .min(1, t('validation:validation.required'))
    .refine((value) => emailPattern.test(value) || phonePattern.test(value), t('validation:validation.emailOrPhone')));

export function fieldError(error?: { message?: string }) {
  return error?.message;
}
