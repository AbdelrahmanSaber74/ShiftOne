import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { Flex, Select, useToast } from '@chakra-ui/react';
import { useTranslation } from 'react-i18next';

import SharedDataTable from 'shared/components/ui/SharedDataTable';
import SharedPageHeader from 'shared/components/ui/SharedPageHeader';
import SharedToolbar from 'shared/components/ui/SharedToolbar';
import { SharedInput } from 'shared/components/ui/SharedFormElements';
import { useAuth } from 'shared/contexts/AuthContext';

import attendanceService from '../services/attendanceService';
import branchesService from 'features/branches/services/branchesService';
import companiesService from 'features/companies/services/companiesService';
import type { AttendanceRecord, Branch, Company } from 'shared/types/api';

export default function AttendancePage() {
  const toast = useToast();
  const { t } = useTranslation();
  const { user } = useAuth();
  const isSuperAdmin = user?.roles?.includes('SuperAdmin') || user?.roles?.includes('Admin');

  const [rows, setRows] = useState<AttendanceRecord[]>([]);
  const [branches, setBranches] = useState<Branch[]>([]);
  const [companies, setCompanies] = useState<Company[]>([]);
  const [selectedCompanyId, setSelectedCompanyId] = useState<string>(localStorage.getItem('active_company_id') || 'all');
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [tableLoading, setTableLoading] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [branchFilter, setBranchFilter] = useState('all');
  const [from, setFrom] = useState('');
  const [to, setTo] = useState('');

  useEffect(() => {
    if (!isSuperAdmin) return;
    companiesService.getCompanies({ page: 1, pageSize: 100 }).then((response) => {
      const nextCompanies = response.data || [];
      setCompanies(nextCompanies);
    });
  }, [isSuperAdmin]);

  const load = useCallback(async (refresh = false) => {
    setTableLoading(true);
    try {
      const attendanceParams = {
        page,
        pageSize,
        branchId: branchFilter === 'all' ? undefined : branchFilter,
        from: from || undefined,
        to: to || undefined,
        refresh,
      };

      let nextBranches: Branch[] = [];
      if (!isSuperAdmin || (selectedCompanyId && selectedCompanyId !== 'all')) {
        const branchesResponse = await branchesService.getBranches({ page: 1, pageSize: 100, isActive: true, refresh });
        nextBranches = branchesResponse.data || [];
      }

      const attendanceResponse = await attendanceService.getAttendance(attendanceParams);
      setRows(attendanceResponse.data || []);
      setTotalCount(attendanceResponse.totalCount ?? attendanceResponse.data?.length ?? 0);
      setBranches(nextBranches);
    } catch {
      toast({ title: t('common.error'), description: t('attendance.loadError'), status: 'error', duration: 3000, isClosable: true });
    } finally {
      setTableLoading(false);
    }
  }, [branchFilter, from, page, pageSize, selectedCompanyId, t, to, toast]);

  useEffect(() => {
    void load();
  }, [load]);

  const filteredRows = useMemo(() => {
    const query = searchQuery.trim().toLowerCase();
    if (!query) return rows;
    return rows.filter((row) => row.employeeName.toLowerCase().includes(query) || row.branchName.toLowerCase().includes(query) || row.attendanceDate.includes(query));
  }, [rows, searchQuery]);


  return (
    <Flex direction="column" w="100%">
      <SharedPageHeader title={t('attendance.pageTitle')} description={t('attendance.pageDesc')} />

      <SharedToolbar
        searchValue={searchQuery}
        onSearchChange={setSearchQuery}
        searchPlaceholder={t('attendance.searchPlaceholder')}
        onRefresh={() => void load(true)}
        isRefreshing={tableLoading}
        extraFilters={
          <>
            {isSuperAdmin && (
              <Select
                value={selectedCompanyId}
                onChange={(event) => {
                  const id = event.target.value;
                  setSelectedCompanyId(id);
                  localStorage.setItem('active_company_id', id);
                  setBranchFilter('all');
                  setPage(1);
                }}
                maxW={{ base: '100%', md: '220px' }}
                borderRadius="8px"
              >
                <option value="all">{t('subscriptions.allCompanies')}</option>
                {companies.map((company) => (
                  <option key={company.id} value={company.id}>
                    {company.name}
                  </option>
                ))}
              </Select>
            )}
            <Select
              value={branchFilter}
              onChange={(event) => {
                setBranchFilter(event.target.value);
                setPage(1);
              }}
              maxW={{ base: '100%', md: '180px' }}
              borderRadius="8px"
              isDisabled={isSuperAdmin && (selectedCompanyId === 'all' || !selectedCompanyId)}
            >
              <option value="all">
                {isSuperAdmin && (selectedCompanyId === 'all' || !selectedCompanyId)
                  ? t('employees.selectCompanyFirst')
                  : t('attendance.allBranches')}
              </option>
              {branches.map((branch) => (
                <option key={branch.id} value={branch.id}>
                  {branch.name}
                </option>
              ))}
            </Select>
            <SharedInput
              type="date"
              aria-label={t('attendance.fromDate')}
              value={from}
              onChange={(event) => {
                setFrom(event.target.value);
                setPage(1);
              }}
              mb="0"
              maxW={{ base: '100%', md: '160px' }}
            />
            <SharedInput
              type="date"
              aria-label={t('attendance.toDate')}
              value={to}
              onChange={(event) => {
                setTo(event.target.value);
                setPage(1);
              }}
              mb="0"
              maxW={{ base: '100%', md: '160px' }}
            />
          </>
        }
      />

      <SharedDataTable
        columns={[
          { key: 'employeeName', header: t('employees.nameHeader'), isSortable: true },
          { key: 'branchName', header: t('employees.branchHeader'), isSortable: true, render: (row) => row.branchName || t('attendance.notBound') },
          { key: 'attendanceDate', header: t('attendance.dateHeader'), isSortable: true },
          { key: 'checkInAt', header: t('attendance.checkInHeader'), isSortable: true },
          { key: 'checkOutAt', header: t('attendance.checkOutHeader'), render: (row) => row.checkOutAt || '-- : --' },
        ]}
        data={filteredRows}
        isLoading={tableLoading}
        emptyTitle={t('attendance.emptyTitle')}
        emptyDescription={t('attendance.emptyDesc')}
        isFiltered={searchQuery !== '' || branchFilter !== 'all' || from !== '' || to !== ''}
        onClearFilters={() => {
          setSearchQuery('');
          setBranchFilter('all');
          setFrom('');
          setTo('');
          setPage(1);
        }}
        page={page}
        pageSize={pageSize}
        totalCount={searchQuery ? filteredRows.length : totalCount}
        onPageChange={setPage}
        onPageSizeChange={(nextPageSize) => {
          setPageSize(nextPageSize);
          setPage(1);
        }}
      />
    </Flex>
  );
}

