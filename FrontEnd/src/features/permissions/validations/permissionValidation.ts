import { z } from 'zod';
import { optionalText, requiredText, Translate } from 'shared/validation/schemaHelpers';

export const permissionSchema = (t: Translate) => z.object({
  name: requiredText(t, 256).refine((value) => /^[A-Za-z]+\.[A-Za-z]+$/.test(value), t('validation:validation.moduleAction')),
  description: optionalText(t, 500).default(''),
});

export type PermissionFormValues = z.infer<ReturnType<typeof permissionSchema>>;
