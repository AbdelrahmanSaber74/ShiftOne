import i18n from 'i18next';

const messageMap: Record<string, { en: string; ar: string }> = {
  'Messages.PlanLimitExceeded': {
    en: 'Plan limit exceeded. You cannot add more records under this subscription plan.',
    ar: 'تم تجاوز الحد الأقصى للخطة. لا يمكنك إضافة المزيد من السجلات في باقة الاشتراك الحالية.',
  },
  'Messages.EmailAlreadyExists': {
    en: 'This email is already registered to another account.',
    ar: 'هذا البريد الإلكتروني مسجل بالفعل لحساب آخر.',
  },
  'Messages.PhoneNumberAlreadyExists': {
    en: 'This phone number is already registered to another account.',
    ar: 'رقم الهاتف هذا مسجل بالفعل لحساب آخر.',
  },
  'Messages.CompanyLimitExceeded': {
    en: 'Company limit exceeded under this subscription plan.',
    ar: 'تم تجاوز الحد الأقصى للشركات في باقة الاشتراك الحالية.',
  },
  'Messages.BranchLimitExceeded': {
    en: 'Branch limit exceeded under this subscription plan.',
    ar: 'تم تجاوز الحد الأقصى للفروع في باقة الاشتراك الحالية.',
  },
  'Messages.UserAlreadyExists': {
    en: 'This user account already exists.',
    ar: 'حساب هذا المستخدم موجود بالفعل.',
  },
  'Messages.InvalidCredentials': {
    en: 'Invalid email/phone or password. Please try again.',
    ar: 'البريد الإلكتروني/الهاتف أو كلمة المرور غير صالحة.',
  },
  'Messages.UserNotFound': {
    en: 'User account not found.',
    ar: 'حساب المستخدم غير موجود.',
  },
  'Messages.AccountLocked': {
    en: 'Your account is locked. Please contact support.',
    ar: 'تم قفل حسابك. يرجى الاتصال بالدعم.',
  },
  'Messages.AccountInactive': {
    en: 'Your account is inactive. Please contact your administrator.',
    ar: 'حسابك غير نشط. يرجى مراجعة مدير النظام.',
  },
  'Messages.WorkScheduleInvalidTimeRange': {
    en: 'Work schedule start time must be before end time for working days.',
    ar: 'يجب أن يكون وقت بداية العمل قبل وقت النهاية للأيام المفعلة.',
  },
  'Messages.WorkScheduleInvalidRules': {
    en: 'Work schedule rules (grace minutes / minimum working minutes) are invalid.',
    ar: 'قواعد جدول مواعيد العمل غير صالحة.',
  },
  'Messages.WorkScheduleWorkingDayRequired': {
    en: 'At least one working day must be selected.',
    ar: 'يجب تحديد يوم عمل واحد على الأقل.',
  }
};

export function getErrorMessage(err: any): string {
  if (err?.response?.data?.message) {
    return err.response.data.message;
  }
  return err?.message || '';
}

export function translateServerMessage(err: any, fallbackMessage: string): string {
  const rawMessage = getErrorMessage(err);
  if (!rawMessage) return fallbackMessage;

  const isRtl = i18n.language?.startsWith('ar');
  const mapped = messageMap[rawMessage];
  if (mapped) {
    return isRtl ? mapped.ar : mapped.en;
  }

  if (rawMessage.startsWith('Messages.')) {
    const cleanKey = rawMessage.replace('Messages.', '');
    const readable = cleanKey.replace(/([A-Z])/g, ' $1').trim();
    return readable;
  }

  return rawMessage;
}
