import { z } from 'zod';
import { booleanField, optionalEmail, optionalPhone, optionalText, requiredEmail, requiredPassword, requiredText, Translate } from 'shared/validation/schemaHelpers';

export const createCompanySchema = (t: Translate) => z.object({
  name: requiredText(t, 150),
  code: requiredText(t, 50),
  email: optionalEmail(t),
  phoneNumber: optionalPhone(t),
  address: optionalText(t, 300),
  planId: z.string().optional(),
  adminEmail: requiredEmail(t),
  adminPassword: requiredPassword(t),
  isActive: booleanField(),
});

export const updateCompanySchema = (t: Translate) => z.object({
  name: requiredText(t, 150),
  code: requiredText(t, 50),
  email: optionalEmail(t),
  phoneNumber: optionalPhone(t),
  address: optionalText(t, 300),
  planId: z.string().optional(),
  adminEmail: z.string().optional(),
  adminPassword: z.string().optional(),
  isActive: booleanField(),
});

export type CompanyFormValues = z.infer<ReturnType<typeof createCompanySchema>>;
