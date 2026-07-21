import { useCallback, useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useToast } from '@chakra-ui/react';
import { useTranslation } from 'react-i18next';
import reportsService from '../services/reportsService';
import { downloadReportFile } from '../services/downloadReportFile';
import { getReportDefinition } from '../constants/reportDefinitions';
import type { ReportKey, ReportRequest } from '../types/reportTypes';
import { queryKeys } from 'shared/query/queryKeys';

const defaultPageSize = 20;

type AppliedReportRequest = ReportRequest & { page: number; pageSize: number };

export function useReportData(reportKey: ReportKey) {
  const toast = useToast();
  const { t } = useTranslation();
  const definition = useMemo(() => getReportDefinition(reportKey), [reportKey]);
  const [page, setPageState] = useState(1);
  const [pageSize, setPageSizeState] = useState(defaultPageSize);
  const [keyword, setKeyword] = useState('');
  const [companyId, setCompanyId] = useState('');
  const [branchId, setBranchId] = useState('');
  const [employeeId, setEmployeeId] = useState('');
  const [scheduleId, setScheduleId] = useState('');
  const [from, setFrom] = useState('');
  const [to, setTo] = useState('');
  const [status, setStatus] = useState('');
  const [role, setRole] = useState('');
  const [sortBy, setSortBy] = useState(definition.defaultSortBy);
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');
  const [generated, setGenerated] = useState(false);
  const [exporting, setExporting] = useState(false);
  const [appliedRequest, setAppliedRequest] = useState<AppliedReportRequest | null>(null);

  const request = useMemo<AppliedReportRequest>(() => ({
    page,
    pageSize,
    keyword,
    companyId,
    branchId,
    employeeId,
    scheduleId,
    from,
    to,
    status,
    role,
    sortBy,
    sortDirection,
  }), [branchId, companyId, employeeId, from, keyword, page, pageSize, role, scheduleId, sortBy, sortDirection, status, to]);

  const reportQuery = useQuery({
    queryKey: queryKeys.reports.report(reportKey, (appliedRequest || { reportKey, idle: true }) as any),
    queryFn: () => reportsService.getReport(reportKey, appliedRequest || request),
    enabled: !!appliedRequest,
  });

  const applyRequest = useCallback((nextRequest: AppliedReportRequest) => {
    setGenerated(true);
    setAppliedRequest(nextRequest);
  }, []);

  const generate = useCallback(async () => {
    const nextRequest = { ...request, page: 1 };
    setPageState(1);
    applyRequest(nextRequest);
    if (appliedRequest && JSON.stringify(appliedRequest) === JSON.stringify(nextRequest)) {
      const result = await reportQuery.refetch();
      if (result.isError) toast({ title: t('reports.errors.load'), status: 'error', duration: 3000, isClosable: true });
    }
  }, [appliedRequest, applyRequest, reportQuery, request, t, toast]);

  const setPage = useCallback((nextPage: number) => {
    setPageState(nextPage);
    if (appliedRequest) setAppliedRequest({ ...appliedRequest, page: nextPage });
  }, [appliedRequest]);

  const setPageSize = useCallback((nextPageSize: number) => {
    setPageSizeState(nextPageSize);
    setPageState(1);
    if (appliedRequest) setAppliedRequest({ ...appliedRequest, page: 1, pageSize: nextPageSize });
  }, [appliedRequest]);

  const exportXlsx = useCallback(async () => {
    setExporting(true);
    try {
      const exportRequest = appliedRequest || request;
      const blob = await reportsService.exportReport(reportKey, exportRequest, 'xlsx');
      downloadReportFile(blob, `${reportKey}-${new Date().toISOString().slice(0, 10)}.xlsx`);
      toast({ title: t('reports.export.success'), status: 'success', duration: 2500, isClosable: true });
    } catch {
      toast({ title: t('reports.export.error'), status: 'error', duration: 3000, isClosable: true });
    } finally {
      setExporting(false);
    }
  }, [appliedRequest, reportKey, request, t, toast]);

  const resetFilters = useCallback(() => {
    setKeyword('');
    setCompanyId('');
    setBranchId('');
    setEmployeeId('');
    setScheduleId('');
    setFrom('');
    setTo('');
    setStatus('');
    setRole('');
    setPageState(1);
  }, []);

  const updateReport = useCallback((nextReport: ReportKey) => {
    const nextDefinition = getReportDefinition(nextReport);
    setSortBy(nextDefinition.defaultSortBy);
    setSortDirection('asc');
    setKeyword('');
    setCompanyId('');
    setBranchId('');
    setEmployeeId('');
    setScheduleId('');
    setFrom('');
    setTo('');
    setStatus('');
    setRole('');
    setPageState(1);
    setPageSizeState(defaultPageSize);
    setGenerated(false);
    setAppliedRequest(null);
  }, []);

  const response = reportQuery.data;
  const hasPendingChanges = !!appliedRequest && JSON.stringify(appliedRequest) !== JSON.stringify(request);

  return {
    definition,
    rows: response?.data || [],
    page,
    pageSize,
    totalCount: response?.totalCount ?? response?.data?.length ?? 0,
    keyword,
    companyId,
    branchId,
    employeeId,
    scheduleId,
    from,
    to,
    status,
    role,
    sortBy,
    sortDirection,
    loading: reportQuery.isLoading || reportQuery.isFetching,
    exporting,
    generated,
    hasPendingChanges,
    setKeyword,
    setCompanyId,
    setBranchId,
    setEmployeeId,
    setScheduleId,
    setFrom,
    setTo,
    setStatus,
    setRole,
    setSortBy,
    setSortDirection,
    setPage,
    setPageSize,
    generate,
    exportXlsx,
    resetFilters,
    updateReport,
  };
}
