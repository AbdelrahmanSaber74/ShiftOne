import type { ReportDefinition } from '../types/reportTypes';

export const reportDefinitions: ReportDefinition[] = [
  {
    key: 'attendance', endpoint: 'attendance', audience: 'tenant', titleKey: 'reports.attendance.title', descriptionKey: 'reports.attendance.description', emptyKey: 'reports.attendance.empty', defaultSortBy: 'attendanceDate', supportsEmployeeFilter: true,
    statusOptions: [
      { labelKey: 'reports.status.all', value: '' },
      { labelKey: 'reports.status.present', value: 'Present' },
      { labelKey: 'reports.status.missingCheckOut', value: 'MissingCheckOut' },
      { labelKey: 'reports.status.absent', value: 'Absent' },
      { labelKey: 'reports.status.late', value: 'Late' },
      { labelKey: 'reports.status.earlyArrival', value: 'EarlyArrival' },
      { labelKey: 'reports.status.earlyLeave', value: 'EarlyLeave' },
      { labelKey: 'reports.status.overtime', value: 'Overtime' },
      { labelKey: 'reports.status.outsideSchedule', value: 'OutsideSchedule' },
      { labelKey: 'reports.status.dayOff', value: 'DayOff' },
      { labelKey: 'reports.status.holiday', value: 'Holiday' },
    ],
    columns: [
      { key: 'companyName', headerKey: 'reports.columns.company' },
      { key: 'branchName', headerKey: 'reports.columns.branch' },
      { key: 'employeeName', headerKey: 'reports.columns.employee' },
      { key: 'attendanceDate', headerKey: 'reports.columns.date', type: 'date' },
      { key: 'status', headerKey: 'reports.columns.status', type: 'status' },
      { key: 'holidayName', headerKey: 'reports.columns.holiday' },
      { key: 'workScheduleName', headerKey: 'reports.columns.schedule' },
      { key: 'scheduledStartTime', headerKey: 'reports.columns.scheduledStart' },
      { key: 'scheduledEndTime', headerKey: 'reports.columns.scheduledEnd' },
      { key: 'checkInAt', headerKey: 'reports.columns.checkIn', type: 'datetime' },
      { key: 'checkOutAt', headerKey: 'reports.columns.checkOut', type: 'datetime' },
      { key: 'workedMinutes', headerKey: 'reports.columns.workedMinutes', type: 'number' },
      { key: 'lateMinutes', headerKey: 'reports.columns.lateMinutes', type: 'number' },
      { key: 'earlyLeaveMinutes', headerKey: 'reports.columns.earlyLeaveMinutes', type: 'number' },
      { key: 'overtimeMinutes', headerKey: 'reports.columns.overtimeMinutes', type: 'number' },
      { key: 'deviceId', headerKey: 'reports.columns.device' },
    ],
  },
  {
    key: 'employees', endpoint: 'employees', audience: 'tenant', titleKey: 'reports.employees.title', descriptionKey: 'reports.employees.description', emptyKey: 'reports.employees.empty', defaultSortBy: 'employeeName',
    statusOptions: [
      { labelKey: 'reports.status.all', value: '' },
      { labelKey: 'reports.status.active', value: 'true' },
      { labelKey: 'reports.status.inactive', value: 'false' },
    ],
    columns: [
      { key: 'companyName', headerKey: 'reports.columns.company' },
      { key: 'branchName', headerKey: 'reports.columns.branch' },
      { key: 'employeeName', headerKey: 'reports.columns.employee' },
      { key: 'email', headerKey: 'reports.columns.email' },
      { key: 'phoneNumber', headerKey: 'reports.columns.phone' },
      { key: 'roles', headerKey: 'reports.columns.roles' },
      { key: 'isActive', headerKey: 'reports.columns.status', type: 'boolean' },
      { key: 'joinedOn', headerKey: 'reports.columns.joinedOn', type: 'date' },
      { key: 'hasBoundDevice', headerKey: 'reports.columns.deviceBound', type: 'boolean' },
    ],
  },
  {
    key: 'companies', endpoint: 'companies', audience: 'platform', titleKey: 'reports.companies.title', descriptionKey: 'reports.companies.description', emptyKey: 'reports.companies.empty', defaultSortBy: 'companyName',
    statusOptions: [
      { labelKey: 'reports.status.all', value: '' },
      { labelKey: 'reports.status.active', value: 'Active' },
      { labelKey: 'reports.status.inactive', value: 'Inactive' },
    ],
    columns: [
      { key: 'companyName', headerKey: 'reports.columns.company' },
      { key: 'planName', headerKey: 'reports.columns.plan' },
      { key: 'branchesCount', headerKey: 'reports.columns.branches', type: 'number' },
      { key: 'employeesCount', headerKey: 'reports.columns.employees', type: 'number' },
      { key: 'subscriptionStatus', headerKey: 'reports.columns.subscriptionStatus', type: 'status' },
      { key: 'expirationDate', headerKey: 'reports.columns.expirationDate', type: 'date' },
    ],
  },
  {
    key: 'branches', endpoint: 'branches', audience: 'tenant', titleKey: 'reports.branches.title', descriptionKey: 'reports.branches.description', emptyKey: 'reports.branches.empty', defaultSortBy: 'branchName',
    statusOptions: [
      { labelKey: 'reports.status.all', value: '' },
      { labelKey: 'reports.status.active', value: 'true' },
      { labelKey: 'reports.status.inactive', value: 'false' },
    ],
    columns: [
      { key: 'companyName', headerKey: 'reports.columns.company' },
      { key: 'branchName', headerKey: 'reports.columns.branch' },
      { key: 'employeesCount', headerKey: 'reports.columns.employees', type: 'number' },
      { key: 'attendanceToday', headerKey: 'reports.columns.attendanceToday', type: 'number' },
      { key: 'geoFenceStatus', headerKey: 'reports.columns.geoFence', type: 'status' },
      { key: 'isActive', headerKey: 'reports.columns.status', type: 'boolean' },
    ],
  },
  {
    key: 'subscriptions', endpoint: 'subscriptions', audience: 'platform', titleKey: 'reports.subscriptions.title', descriptionKey: 'reports.subscriptions.description', emptyKey: 'reports.subscriptions.empty', defaultSortBy: 'startDate',
    statusOptions: [
      { labelKey: 'reports.status.all', value: '' },
      { labelKey: 'reports.status.active', value: 'Active' },
      { labelKey: 'reports.status.inactive', value: 'Inactive' },
    ],
    columns: [
      { key: 'companyName', headerKey: 'reports.columns.company' },
      { key: 'planName', headerKey: 'reports.columns.plan' },
      { key: 'price', headerKey: 'reports.columns.price', type: 'currency' },
      { key: 'status', headerKey: 'reports.columns.status', type: 'status' },
      { key: 'holidayName', headerKey: 'reports.columns.holiday' },
      { key: 'startDate', headerKey: 'reports.columns.startDate', type: 'date' },
      { key: 'endDate', headerKey: 'reports.columns.endDate', type: 'date' },
      { key: 'remainingDays', headerKey: 'reports.columns.remainingDays', type: 'number' },
    ],
  },
  {
    key: 'plan-usage', endpoint: 'plan-usage', audience: 'platform', titleKey: 'reports.planUsage.title', descriptionKey: 'reports.planUsage.description', emptyKey: 'reports.planUsage.empty', defaultSortBy: 'planName',
    columns: [
      { key: 'planName', headerKey: 'reports.columns.plan' },
      { key: 'companiesCount', headerKey: 'reports.columns.companies', type: 'number' },
      { key: 'employeesCount', headerKey: 'reports.columns.employees', type: 'number' },
      { key: 'branchesCount', headerKey: 'reports.columns.branches', type: 'number' },
      { key: 'averageUsage', headerKey: 'reports.columns.averageUsage', type: 'number' },
    ],
  },
];

export const getReportDefinition = (key: string) => reportDefinitions.find((report) => report.key === key) ?? reportDefinitions[0];


