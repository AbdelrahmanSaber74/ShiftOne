import { z } from 'zod';
import { booleanField, optionalText, requiredText, Translate } from 'shared/validation/schemaHelpers';

export const roleSchema = (t: Translate) => z.object({
  name: requiredText(t, 256),
  description: optionalText(t, 500).default(''),
  isActive: booleanField(),
  permissionIds: z.array(z.string()).default([]),
});

export type RoleFormValues = z.infer<ReturnType<typeof roleSchema>>;
