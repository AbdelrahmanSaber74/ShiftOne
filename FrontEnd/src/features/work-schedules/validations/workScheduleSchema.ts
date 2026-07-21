import { z } from 'zod';
import { booleanField, optionalGuid, optionalText, requiredSelect, requiredText, Translate } from 'shared/validation/schemaHelpers';

const timeRegex = /^([0-1]?\d|2[0-3]):[0-5]\d(:[0-5]\d)?$/;

export const workScheduleSchema = (t: Translate, requireCompany: boolean) =>
  z.object({
    companyId: requireCompany
      ? requiredSelect(t)
      : z.string().optional().nullable().transform((v) => v || undefined),
    branchId: z.string().optional().nullable().transform((v) => v || undefined),
    name: requiredText(t, 120),
    description: z.preprocess((v) => (v === null || v === undefined ? '' : String(v)), z.string().max(500).default('')),
    timeZoneId: z.preprocess((v) => (v ? String(v) : 'Arab Standard Time'), z.string().default('Arab Standard Time')),
    isDefault: z.preprocess((v) => Boolean(v), z.boolean().default(false)),
    isActive: z.preprocess((v) => (v === undefined || v === null ? true : Boolean(v)), z.boolean().default(true)),
    days: z.array(
      z.object({
        dayOfWeek: z.preprocess((v) => Math.max(0, Math.min(6, Number(v) || 0)), z.number().int().min(0).max(6)),
        isWorkingDay: z.preprocess((v) => Boolean(v), z.boolean().default(false)),
        startTime: z.preprocess((v) => (v ? String(v) : ''), z.string().optional().nullable()),
        endTime: z.preprocess((v) => (v ? String(v) : ''), z.string().optional().nullable()),
        lateGraceMinutes: z.preprocess((v) => Math.max(0, Number(v) || 0), z.number().int().min(0).default(0)),
        earlyLeaveGraceMinutes: z.preprocess((v) => Math.max(0, Number(v) || 0), z.number().int().min(0).default(0)),
        minimumWorkingMinutes: z.preprocess((v) => Math.max(0, Number(v) || 0), z.number().int().min(0).default(0)),
        overtimeEnabled: z.preprocess((v) => Boolean(v), z.boolean().default(false)),
      })
    ).superRefine((days, ctx) => {
      if (!days.some((day) => day.isWorkingDay)) {
        ctx.addIssue({ code: 'custom', message: t('workSchedules.validation.workingDayRequired') });
      }
      days.forEach((day, index) => {
        if (!day.isWorkingDay) return;
        if (!day.startTime || !timeRegex.test(day.startTime)) {
          ctx.addIssue({ code: 'custom', path: [index, 'startTime'], message: t('workSchedules.validation.startRequired') });
        }
        if (!day.endTime || !timeRegex.test(day.endTime)) {
          ctx.addIssue({ code: 'custom', path: [index, 'endTime'], message: t('workSchedules.validation.endRequired') });
        }
        if (day.startTime && day.endTime && timeRegex.test(day.startTime) && timeRegex.test(day.endTime) && day.startTime >= day.endTime) {
          ctx.addIssue({ code: 'custom', path: [index, 'endTime'], message: t('workSchedules.validation.timeRange') });
        }
      });
    }),
  });

export type WorkScheduleFormValues = z.infer<ReturnType<typeof workScheduleSchema>>;
