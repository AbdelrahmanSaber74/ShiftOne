import React, { useEffect, useMemo, useState } from 'react';
import { Badge, Box, Button, Flex, Icon, Select, SimpleGrid, Text, Tooltip, useColorModeValue, useToast } from '@chakra-ui/react';
import { MdBusiness, MdFileDownload, MdGroups, MdPlayArrow, MdPrint, MdStore, MdSubscriptions, MdTableChart, MdToday } from 'react-icons/md';
import { useTranslation } from 'react-i18next';
import { useAuth } from 'shared/contexts/AuthContext';
import { hasPermission, isPlatformAdmin } from 'shared/utils/authUtils';

import SharedDataTable, { SharedDataTableColumn } from 'shared/components/ui/SharedDataTable';
import SharedPageHeader from 'shared/components/ui/SharedPageHeader';
import SharedToolbar from 'shared/components/ui/SharedToolbar';
import { SharedInput } from 'shared/components/ui/SharedFormElements';
import SharedStatusBadge from 'shared/components/ui/SharedStatusBadge';
import { formatCurrency, formatDate, formatNumber } from 'shared/utils/localeUtils';
import companiesService from 'features/companies/services/companiesService';
import branchesService from 'features/branches/services/branchesService';
import employeesService from 'features/employees/services/employeesService';
import workSchedulesService from 'features/work-schedules/services/workSchedulesService';
import type { WorkSchedule } from 'features/work-schedules/types';
import type { Branch, Company, Employee } from 'shared/types/api';

import { reportDefinitions } from '../constants/reportDefinitions';
import { useReportData } from '../hooks/useReportData';
import type { ReportKey, ReportRowBase } from '../types/reportTypes';

const reportIcons: Record<ReportKey, React.ElementType> = {
  attendance: MdToday,
  employees: MdGroups,
  companies: MdBusiness,
  branches: MdStore,
  subscriptions: MdSubscriptions,
  'plan-usage': MdTableChart,
};

export default function ReportsPage() {
  const { t, i18n } = useTranslation();
  const toast = useToast();
  const { user } = useAuth();
  const isPlatformUser = isPlatformAdmin(user);
  const canFilterCompanies = isPlatformUser;
  const canExportReports = hasPermission(user, 'Reports.Export');
  const visibleReportDefinitions = useMemo(() => reportDefinitions.filter((definition) => isPlatformUser || definition.audience === 'tenant'), [isPlatformUser]);
  const [activeReport, setActiveReport] = useState<ReportKey>('attendance');
  const report = useReportData(activeReport);
  const updateReport = report.updateReport;
  const [companies, setCompanies] = useState<Company[]>([]);
  const [branches, setBranches] = useState<Branch[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [schedules, setSchedules] = useState<WorkSchedule[]>([]);
  const [referencesLoading, setReferencesLoading] = useState(false);
  const borderColor = useColorModeValue('gray.200', 'whiteAlpha.100');
  const cardBg = useColorModeValue('white', 'navy.800');
  const mutedColor = useColorModeValue('secondaryGray.600', 'secondaryGray.400');
  const activeBg = useColorModeValue('brand.50', 'rgba(23,105,224,0.16)');
  const activeBorder = useColorModeValue('brand.500', 'brand.300');

  useEffect(() => {
    const loadReferences = async () => {
      setReferencesLoading(true);
      const referenceRequests = [
        canFilterCompanies ? companiesService.getCompanies({ page: 1, pageSize: 200, isActive: true }) : Promise.resolve(null),
        branchesService.getBranches({ page: 1, pageSize: 200, isActive: true }),
        employeesService.getEmployees({ page: 1, pageSize: 500, isActive: true }),
        workSchedulesService.getWorkSchedules({ page: 1, pageSize: 300, isActive: true }),
      ] as const;
      const [companiesResult, branchesResult, employeesResult, schedulesResult] = await Promise.allSettled(referenceRequests);
      if (companiesResult.status === 'fulfilled' && companiesResult.value) setCompanies(companiesResult.value.data || []);
      if (branchesResult.status === 'fulfilled') setBranches(branchesResult.value.data || []);
      if (employeesResult.status === 'fulfilled') setEmployees(employeesResult.value.data || []);
      if (schedulesResult.status === 'fulfilled') setSchedules(schedulesResult.value.data || []);
      const failed = [companiesResult, branchesResult, employeesResult, schedulesResult].some((result) => result.status === 'rejected');
      if (failed) toast({ title: t('reports.errors.references'), status: 'warning', duration: 2500, isClosable: true });
      setReferencesLoading(false);
    };
    void loadReferences();
  }, [canFilterCompanies, t, toast]);

  const locale = i18n.language?.startsWith('ar') ? 'ar-EG' : 'en-US';

  const columns = useMemo<SharedDataTableColumn<ReportRowBase>[]>(() => report.definition.columns.map((column) => ({
    key: column.key,
    header: t(column.headerKey),
    isSortable: true,
    getSortValue: (row) => String(row[column.key] ?? ''),
    render: (row) => renderCell(row[column.key], column.type, locale, t),
  })), [locale, report.definition.columns, t]);

  useEffect(() => {
    if (visibleReportDefinitions.length === 0) return;
    if (!visibleReportDefinitions.some((definition) => definition.key === activeReport)) {
      const fallbackReport = visibleReportDefinitions[0].key;
      setActiveReport(fallbackReport);
      updateReport(fallbackReport);
    }
  }, [activeReport, updateReport, visibleReportDefinitions]);

  const handleReportChange = (next: ReportKey) => {
    if (!visibleReportDefinitions.some((definition) => definition.key === next)) return;
    setActiveReport(next);
    updateReport(next);
  };

  const showCompanyFilter = canFilterCompanies && activeReport !== 'plan-usage';
  const showBranchFilter = activeReport === 'attendance' || activeReport === 'employees' || activeReport === 'branches';
  const showEmployeeFilter = activeReport === 'attendance';
  const showScheduleFilter = activeReport === 'attendance';
  const showDateFilter = activeReport === 'attendance' || activeReport === 'subscriptions';
  const showRoleFilter = activeReport === 'employees';
  const showStatusFilter = !!report.definition.statusOptions;

  return (
    <Flex direction="column" w="100%">
      <SharedPageHeader title={t('reports.pageTitle')} description={t('reports.pageDesc')} />

      <SimpleGrid columns={{ base: 1, md: 2, xl: 3 }} gap="12px" mb="16px">
        {visibleReportDefinitions.map((definition) => {
          const isActive = definition.key === activeReport;
          const ReportIcon = reportIcons[definition.key];
          return (
            <Box
              as="button"
              type="button"
              key={definition.key}
              onClick={() => handleReportChange(definition.key)}
              textAlign="start"
              bg={isActive ? activeBg : cardBg}
              border="1px solid"
              borderColor={isActive ? activeBorder : borderColor}
              borderRadius="8px"
              p="14px"
              transition="150ms ease"
              _hover={{ borderColor: activeBorder }}
            >
              <Flex gap="12px" align="flex-start">
                <Flex h="38px" w="38px" borderRadius="8px" align="center" justify="center" bg={isActive ? 'brand.500' : 'gray.100'} color={isActive ? 'white' : 'secondaryGray.700'} flex="0 0 auto">
                  <Icon as={ReportIcon} boxSize="20px" />
                </Flex>
                <Box minW="0">
                  <Text fontSize="sm" fontWeight="800" noOfLines={1}>{t(definition.titleKey)}</Text>
                  <Text mt="4px" fontSize="xs" color={mutedColor} noOfLines={2}>{t(definition.descriptionKey)}</Text>
                </Box>
              </Flex>
            </Box>
          );
        })}
      </SimpleGrid>

      <SharedToolbar>
        <Flex direction="column" gap="14px" w="100%">
          <Flex justify="space-between" gap="12px" align={{ base: 'stretch', md: 'center' }} direction={{ base: 'column', md: 'row' }}>
            <Box>
              <Text fontSize="lg" fontWeight="800">{t(report.definition.titleKey)}</Text>
              <Text fontSize="sm" color={mutedColor}>{t('reports.filters.helper')}</Text>
            </Box>
            {report.hasPendingChanges && <Badge alignSelf={{ base: 'flex-start', md: 'center' }} colorScheme="orange" borderRadius="full" px="10px" py="4px">{t('reports.filters.pending')}</Badge>}
          </Flex>

          <SimpleGrid columns={{ base: 1, md: 2, xl: 4 }} gap="12px" w="100%">
            <SharedInput mb="0" value={report.keyword} onChange={(event) => { report.setKeyword(event.target.value); }} placeholder={t('reports.filters.search')} />
            {showCompanyFilter && (
              <Select value={report.companyId} onChange={(event) => { report.setCompanyId(event.target.value); }} borderRadius="8px" isDisabled={referencesLoading}>
                <option value="">{t('reports.filters.allCompanies')}</option>
                {companies.map((company) => <option key={company.id} value={company.id}>{company.name}</option>)}
              </Select>
            )}
            {showBranchFilter && (
              <Select value={report.branchId} onChange={(event) => { report.setBranchId(event.target.value); }} borderRadius="8px" isDisabled={referencesLoading}>
                <option value="">{t('reports.filters.allBranches')}</option>
                {branches.map((branch) => <option key={branch.id} value={branch.id}>{branch.name}</option>)}
              </Select>
            )}
            {showEmployeeFilter && (
              <Select value={report.employeeId} onChange={(event) => { report.setEmployeeId(event.target.value); }} borderRadius="8px" isDisabled={referencesLoading}>
                <option value="">{t('reports.filters.allEmployees')}</option>
                {employees.map((employee) => <option key={employee.id} value={employee.id}>{`${employee.firstName} ${employee.lastName}`}</option>)}
              </Select>
            )}
            {showScheduleFilter && (
              <Select value={report.scheduleId} onChange={(event) => { report.setScheduleId(event.target.value); }} borderRadius="8px" isDisabled={referencesLoading}>
                <option value="">{t('reports.filters.allSchedules')}</option>
                {schedules.map((schedule) => <option key={schedule.id} value={schedule.id}>{schedule.name}</option>)}
              </Select>
            )}
            {showRoleFilter && (
              <Select value={report.role} onChange={(event) => { report.setRole(event.target.value); }} borderRadius="8px">
                <option value="">{t('reports.filters.allRoles')}</option>
                <option value="Employee">{t('reports.roles.employee')}</option>
                <option value="HR">{t('reports.roles.hr')}</option>
                <option value="CompanyAdmin">{t('reports.roles.companyAdmin')}</option>
              </Select>
            )}
            {showStatusFilter && (
              <Select value={report.status} onChange={(event) => { report.setStatus(event.target.value); }} borderRadius="8px">
                {(report.definition.statusOptions || []).map((option) => <option key={option.value || 'all'} value={option.value}>{t(option.labelKey)}</option>)}
              </Select>
            )}
            {showDateFilter && <SharedInput mb="0" type="date" aria-label={t('reports.filters.from')} value={report.from} onChange={(event) => { report.setFrom(event.target.value); }} />}
            {showDateFilter && <SharedInput mb="0" type="date" aria-label={t('reports.filters.to')} value={report.to} onChange={(event) => { report.setTo(event.target.value); }} />}
            <Select value={report.sortBy} onChange={(event) => report.setSortBy(event.target.value)} borderRadius="8px">
              {report.definition.columns.map((column) => <option key={column.key} value={column.key}>{t(column.headerKey)}</option>)}
            </Select>
            <Select value={report.sortDirection} onChange={(event) => report.setSortDirection(event.target.value as 'asc' | 'desc')} borderRadius="8px">
              <option value="asc">{t('reports.sort.asc')}</option>
              <option value="desc">{t('reports.sort.desc')}</option>
            </Select>
          </SimpleGrid>

          <Flex gap="10px" wrap="wrap" justify="flex-end">
            <Button variant="ghost" onClick={report.resetFilters}>{t('common.clearFilters')}</Button>
            <Button leftIcon={<Icon as={MdPlayArrow as React.ElementType} />} variant="brand" onClick={report.generate} isLoading={report.loading}>{report.generated ? t('reports.actions.refresh') : t('reports.actions.generate')}</Button>
            <Button leftIcon={<Icon as={MdFileDownload as React.ElementType} />} variant="outline" onClick={report.exportXlsx} isLoading={report.exporting} isDisabled={!report.generated || !canExportReports}>{t('reports.actions.exportXlsx')}</Button>
            <Tooltip label={t('reports.actions.printFuture')}>
              <Button leftIcon={<Icon as={MdPrint as React.ElementType} />} variant="outline" isDisabled>{t('reports.actions.print')}</Button>
            </Tooltip>
          </Flex>
        </Flex>
      </SharedToolbar>

      <SharedDataTable
        columns={columns}
        data={report.rows}
        isLoading={report.loading}
        emptyTitle={report.generated ? t(report.definition.emptyKey) : t('reports.empty.generateTitle')}
        emptyDescription={report.generated ? t('reports.empty.adjustFilters') : t('reports.empty.generateDesc')}
        emptyActionLabel={t('reports.actions.generate')}
        onEmptyAction={report.generate}
        isFiltered={!!report.keyword || !!report.companyId || !!report.branchId || !!report.employeeId || !!report.scheduleId || !!report.from || !!report.to || !!report.status || !!report.role}
        onClearFilters={report.resetFilters}
        page={report.page}
        pageSize={report.pageSize}
        totalCount={report.totalCount}
        onPageChange={report.setPage}
        onPageSizeChange={report.setPageSize}
      />
    </Flex>
  );
}

function renderCell(value: unknown, type: string | undefined, locale: string, t: (key: string) => string) {
  if (value === null || value === undefined || value === '') return <Text color="secondaryGray.500">--</Text>;
  if (type === 'date') return formatDate(String(value), locale, { dateStyle: 'medium' });
  if (type === 'datetime') return formatDate(String(value), locale, { dateStyle: 'medium', timeStyle: 'short' });
  if (type === 'currency') return formatCurrency(Number(value), 'SAR', locale);
  if (type === 'number') return formatNumber(Number(value), locale);
  if (type === 'boolean') return <SharedStatusBadge isActive={Boolean(value)} activeLabel={t('reports.status.active')} inactiveLabel={t('reports.status.inactive')} />;
  if (type === 'status') return <Badge borderRadius="full" px="10px" py="4px" colorScheme={statusColor(String(value))}>{translateStatus(String(value), t)}</Badge>;
  return String(value);
}

function statusColor(value: string) {
  const normalized = value.toLowerCase();
  if (normalized.includes('active') || normalized.includes('present') || normalized.includes('configured')) return 'green';
  if (normalized.includes('missing') || normalized.includes('absent')) return 'yellow';
  return 'red';
}

function translateStatus(value: string, t: (key: string) => string) {
  const normalized = value.toLowerCase();
  if (normalized === 'present') return t('reports.status.present');
  if (normalized === 'missingcheckout') return t('reports.status.missingCheckOut');
  if (normalized === 'absent') return t('reports.status.absent');
  if (normalized === 'active') return t('reports.status.active');
  if (normalized === 'inactive') return t('reports.status.inactive');
  if (normalized === 'configured') return t('reports.status.configured');
  if (normalized === 'notconfigured') return t('reports.status.notConfigured');
  return value;
}



