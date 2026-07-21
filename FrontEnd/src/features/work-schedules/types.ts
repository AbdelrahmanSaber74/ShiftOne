export interface WorkScheduleDay {
  id?: string;
  dayOfWeek: number;
  isWorkingDay: boolean;
  startTime?: string | null;
  endTime?: string | null;
  lateGraceMinutes: number;
  earlyLeaveGraceMinutes: number;
  minimumWorkingMinutes: number;
  overtimeEnabled: boolean;
}

export interface WorkSchedule {
  id: string;
  companyId: string;
  companyName: string;
  branchId?: string | null;
  branchName?: string | null;
  name: string;
  description?: string | null;
  timeZoneId: string;
  isDefault: boolean;
  isActive: boolean;
  workingDaysCount: number;
  createdOn: string;
  days: WorkScheduleDay[];
}
