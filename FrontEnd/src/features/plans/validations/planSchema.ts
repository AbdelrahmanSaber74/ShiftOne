import { z } from 'zod';
import { booleanField, nullablePositiveInteger, optionalText, requiredSelect, requiredText, Translate } from 'shared/validation/schemaHelpers';

export const planSchema = (t: Translate) => z.object({
  name: requiredText(t, 100),
  description: optionalText(t, 500).default(''),
  price: z.coerce.number({ error: t('validation:validation.number') }).min(0, t('validation:validation.nonNegativeNumber')),
  billingPeriod: requiredSelect(t),
  maxBranches: nullablePositiveInteger(t),
  maxEmployees: nullablePositiveInteger(t),
  maxHRUsers: nullablePositiveInteger(t),
  maxCompanyAdmins: nullablePositiveInteger(t),
  isActive: booleanField(),
});

export type PlanFormValues = z.infer<ReturnType<typeof planSchema>>;
