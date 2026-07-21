import { z } from 'zod';
import { emailOrPhone, requiredPassword, Translate } from 'shared/validation/schemaHelpers';

export const signInSchema = (t: Translate) => z.object({
  emailOrPhone: emailOrPhone(t),
  password: requiredPassword(t),
});

export type SignInFormValues = z.infer<ReturnType<typeof signInSchema>>;
