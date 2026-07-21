import { z } from 'zod';
import { booleanField, optionalPassword, optionalPhone, requiredPassword, requiredSelect, requiredText, Translate } from 'shared/validation/schemaHelpers';

export const employeeSchema = (t: Translate, isPlatformAdmin: boolean, isEdit: boolean) =>
  z.object({
    firstName: requiredText(t, 50),
    lastName: requiredText(t, 50),
    email: z.string().trim().min(1, t('validation:validation.required')).email(t('validation:validation.email')),
    phoneNumber: optionalPhone(t),
    companyId: isPlatformAdmin ? requiredSelect(t) : z.string().optional(),
    branchId: z.string().optional(),
    role: requiredSelect(t),
    password: isEdit ? optionalPassword(t) : requiredPassword(t),
    isActive: booleanField(),
  }).superRefine((data, ctx) => {
    if (data.role === 'Employee') {
      if (!data.branchId || data.branchId.trim() === '') {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          path: ['branchId'],
          message: t('validation:validation.selectRequired'),
        });
      }
    }
  });

export type EmployeeFormValues = z.infer<ReturnType<typeof employeeSchema>>;
