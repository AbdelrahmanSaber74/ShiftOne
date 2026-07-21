import { z } from 'zod';
import { booleanField, requiredSelect, Translate } from 'shared/validation/schemaHelpers';

export const subscriptionSchema = (t: Translate) => z.object({
  companyId: requiredSelect(t),
  planId: requiredSelect(t),
  isActive: booleanField(),
});

export type SubscriptionFormValues = z.infer<ReturnType<typeof subscriptionSchema>>;
