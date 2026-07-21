import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { Button, Flex, Icon, Select, useDisclosure, useToast, VStack } from '@chakra-ui/react';
import { MdAdd } from 'react-icons/md';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

import SharedDataTable from 'shared/components/ui/SharedDataTable';
import SharedPageHeader from 'shared/components/ui/SharedPageHeader';
import SharedToolbar from 'shared/components/ui/SharedToolbar';
import SharedModal from 'shared/components/ui/SharedModal';
import SharedStatusBadge from 'shared/components/ui/SharedStatusBadge';
import { SharedCheckbox, SharedSelect } from 'shared/components/ui/SharedFormElements';
import { defaultStatusOptions, StatusFilterValue } from 'shared/utils/filterUtils';
import { useTranslation } from 'react-i18next';
import { translateServerMessage } from 'shared/utils/errorUtils';

import subscriptionsService from '../services/subscriptionsService';
import companiesService from 'features/companies/services/companiesService';
import plansService from 'features/plans/services/plansService';
import type { Company, CompanySubscription, SubscriptionPlan } from 'shared/types/api';
import { SubscriptionFormValues, subscriptionSchema } from '../validations/subscriptionSchema';

const emptySubscriptionForm: SubscriptionFormValues = { companyId: '', planId: '', isActive: true };

export default function SubscriptionsPage() {
  const toast = useToast();
  const { t } = useTranslation();
  const initialSelectRef = useRef<HTMLSelectElement>(null);
  const schema = useMemo(() => subscriptionSchema(t), [t]);

  const [rows, setRows] = useState<CompanySubscription[]>([]);
  const [companies, setCompanies] = useState<Company[]>([]);
  const [plans, setPlans] = useState<SubscriptionPlan[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [tableLoading, setTableLoading] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [companyFilter, setCompanyFilter] = useState('all');
  const [planFilter, setPlanFilter] = useState('all');
  const [statusFilter, setStatusFilter] = useState<StatusFilterValue>('all');
  const { isOpen: isDrawerOpen, onOpen: onDrawerOpen, onClose: onDrawerClose } = useDisclosure();
  const { register, control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<SubscriptionFormValues>({ resolver: zodResolver(schema) as any, mode: 'onBlur', reValidateMode: 'onChange', defaultValues: emptySubscriptionForm });

  const loadReferenceData = useCallback(async () => {
    const [companiesResponse, plansResponse] = await Promise.all([companiesService.getCompanies({ page: 1, pageSize: 100, isActive: true }), plansService.getPlans({ page: 1, pageSize: 100, isActive: true })]);
    setCompanies(companiesResponse.data || []);
    setPlans(plansResponse.data || []);
  }, []);

  const load = useCallback(async () => {
    setTableLoading(true);
    try {
      const response = await subscriptionsService.getSubscriptions({ page, pageSize, companyId: companyFilter === 'all' ? undefined : companyFilter });
      setRows(response.data || []);
      setTotalCount(response.totalCount ?? response.data?.length ?? 0);
      await loadReferenceData();
    } catch {
      toast({ title: t('common.error'), description: t('subscriptions.loadError'), status: 'error', duration: 3000, isClosable: true });
    } finally {
      setTableLoading(false);
    }
  }, [companyFilter, loadReferenceData, page, pageSize, t, toast]);

  useEffect(() => { void load(); }, [load]);

  const filteredRows = useMemo(() => {
    const query = searchQuery.trim().toLowerCase();
    return rows.filter((row) => {
      const matchesSearch = !query || row.companyName.toLowerCase().includes(query) || row.planName.toLowerCase().includes(query);
      const matchesPlan = planFilter === 'all' || row.planId?.toString() === planFilter;
      const matchesStatus = statusFilter === 'all' || (statusFilter === 'active' ? row.isActive : !row.isActive);
      return matchesSearch && matchesPlan && matchesStatus;
    });
  }, [planFilter, rows, searchQuery, statusFilter]);

  const handleAddNew = () => { reset(emptySubscriptionForm); onDrawerOpen(); };
  const onSubmit = handleSubmit(async (values) => {
    try {
      await subscriptionsService.assignSubscription(values);
      toast({ title: t('subscriptions.assigned'), status: 'success', duration: 2500, isClosable: true });
      onDrawerClose();
      await load();
    } catch (err) {
      toast({ title: t('subscriptions.assignError'), description: translateServerMessage(err, t('common.error')), status: 'error', duration: 4500, isClosable: true });
    }
  });

  return (
    <Flex direction="column" w="100%">
      <SharedPageHeader title={t('subscriptions.pageTitle')} description={t('subscriptions.pageDesc')} action={<Button leftIcon={<Icon as={MdAdd as React.ElementType} />} variant="brand" onClick={handleAddNew} w={{ base: '100%', md: 'auto' }}>{t('subscriptions.assignPlanBtn')}</Button>} />
      <SharedToolbar searchValue={searchQuery} onSearchChange={setSearchQuery} searchPlaceholder={t('subscriptions.searchPlaceholder')} statusValue={statusFilter} onStatusChange={(value) => setStatusFilter(value as StatusFilterValue)} statusOptions={defaultStatusOptions} onRefresh={load} isRefreshing={tableLoading} extraFilters={<><Select value={companyFilter} onChange={(event) => { setCompanyFilter(event.target.value); setPage(1); }} maxW={{ base: '100%', md: '220px' }} borderRadius="8px"><option value="all">{t('subscriptions.allCompanies')}</option>{companies.map((company) => <option key={company.id} value={company.id}>{company.name}</option>)}</Select><Select value={planFilter} onChange={(event) => setPlanFilter(event.target.value)} maxW={{ base: '100%', md: '180px' }} borderRadius="8px"><option value="all">{t('subscriptions.allPlans')}</option>{plans.map((plan) => <option key={plan.id} value={plan.id}>{plan.name}</option>)}</Select></>} />
      <SharedDataTable columns={[{ key: 'companyName', header: t('companies.companyName'), isSortable: true }, { key: 'planName', header: t('plans.planName'), isSortable: true }, { key: 'startsOn', header: t('subscriptions.startsHeader'), isSortable: true }, { key: 'endsOn', header: t('subscriptions.endsHeader'), render: (row) => row.endsOn || t('common.openEnded') }, { key: 'isActive', header: t('common.status'), render: (row) => <SharedStatusBadge isActive={row.isActive} />, getSortValue: (row) => row.isActive }]} data={filteredRows} isLoading={tableLoading} emptyTitle={t('subscriptions.emptyTitle')} emptyDescription={t('subscriptions.emptyDesc')} emptyActionLabel={t('subscriptions.emptyAction')} onEmptyAction={handleAddNew} isFiltered={searchQuery !== '' || companyFilter !== 'all' || planFilter !== 'all' || statusFilter !== 'all'} onClearFilters={() => { setSearchQuery(''); setCompanyFilter('all'); setPlanFilter('all'); setStatusFilter('all'); setPage(1); }} page={page} pageSize={pageSize} totalCount={searchQuery || planFilter !== 'all' || statusFilter !== 'all' ? filteredRows.length : totalCount} onPageChange={setPage} onPageSizeChange={(nextPageSize) => { setPageSize(nextPageSize); setPage(1); }} />
      <SharedModal isOpen={isDrawerOpen} onClose={onDrawerClose} title={t('subscriptions.titleAssign')} description={t('subscriptions.desc')} isLoading={isSubmitting} initialFocusRef={initialSelectRef} footer={<><Button variant="ghost" onClick={onDrawerClose} isDisabled={isSubmitting}>{t('common.cancel')}</Button><Button type="submit" form="subscription-form" variant="brand" isLoading={isSubmitting} loadingText={t('subscriptions.assigning')}>{t('subscriptions.assignPlan')}</Button></>}>
        <form id="subscription-form" onSubmit={onSubmit} noValidate>
          <VStack spacing="16px" align="stretch">
            <SharedSelect label={t('subscriptions.selectCompany')} isRequired placeholderLabel={t('subscriptions.chooseCompany')} options={companies.map((company) => ({ label: company.name, value: company.id }))} error={errors.companyId?.message} {...register('companyId')} />
            <SharedSelect label={t('subscriptions.selectPlan')} isRequired placeholderLabel={t('subscriptions.choosePlan')} options={plans.map((plan) => ({ label: plan.name, value: plan.id }))} error={errors.planId?.message} {...register('planId')} />
            <Controller name="isActive" control={control} render={({ field }) => <SharedCheckbox label={t('subscriptions.activeSubscription')} isChecked={field.value} onChange={(event) => field.onChange(event.target.checked)} />} />
          </VStack>
        </form>
      </SharedModal>
    </Flex>
  );
}

