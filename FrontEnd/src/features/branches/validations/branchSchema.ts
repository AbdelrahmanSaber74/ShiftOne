import { z } from 'zod';
import { booleanField, requiredInteger, requiredNumber, requiredSelect, requiredText, Translate } from 'shared/validation/schemaHelpers';

export const branchSchema = (t: Translate, isPlatformAdmin: boolean) => z.object({
  companyId: isPlatformAdmin ? z.string().min(1, t('validation:validation.selectRequired')) : z.string().optional(),
  workScheduleId: z.string().optional(),
  name: requiredText(t, 150),
  code: requiredText(t, 50),
  address: requiredText(t, 300),
  latitude: requiredNumber(t, -90, 90),
  longitude: requiredNumber(t, -180, 180),
  allowedRadius: requiredInteger(t, 1, 100000),
  isMainBranch: z.boolean().default(false),
  isActive: booleanField(),
});

export type BranchFormValues = z.infer<ReturnType<typeof branchSchema>>;
