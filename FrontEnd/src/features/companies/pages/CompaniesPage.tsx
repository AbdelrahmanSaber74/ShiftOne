import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { Button, Flex, Icon, Select, useDisclosure, useToast, VStack } from '@chakra-ui/react';
import { MdAdd } from 'react-icons/md';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

import SharedDataTable from 'shared/components/ui/SharedDataTable';
import SharedPageHeader from 'shared/components/ui/SharedPageHeader';
import SharedToolbar from 'shared/components/ui/SharedToolbar';
import SharedModal from 'shared/components/ui/SharedModal';
import SharedConfirmDialog from 'shared/components/ui/SharedConfirmDialog';
import SharedStatusBadge from 'shared/components/ui/SharedStatusBadge';
import { SharedCheckbox, SharedInput, SharedSelect } from 'shared/components/ui/SharedFormElements';
import { defaultStatusOptions, statusFilterToBoolean, StatusFilterValue } from 'shared/utils/filterUtils';
import { useTranslation } from 'react-i18next';
import { translateServerMessage } from 'shared/utils/errorUtils';

import companiesService from '../services/companiesService';
import plansService from 'features/plans/services/plansService';
import type { Company, SubscriptionPlan } from 'shared/types/api';
import { CompanyFormValues, createCompanySchema, updateCompanySchema } from '../validations/companySchema';

const emptyCompanyForm: CompanyFormValues = {
  name: '',
  code: '',
  email: '',
  phoneNumber: '',
  address: '',
  planId: '',
  adminEmail: '',
  adminPassword: '',
  isActive: true,
};

export default function CompaniesPage() {
  const toast = useToast();
  const { t } = useTranslation();
  const initialInputRef = useRef<HTMLInputElement>(null);

  const [rows, setRows] = useState<Company[]>([]);
  const [plans, setPlans] = useState<SubscriptionPlan[]>([]);
  const [tableLoading, setTableLoading] = useState(false);
  const [deleteLoading, setDeleteLoading] = useState(false);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [searchQuery, setSearchQuery] = useState('');
  const [planFilter, setPlanFilter] = useState('all');
  const [statusFilter, setStatusFilter] = useState<StatusFilterValue>('all');

  const { isOpen: isDrawerOpen, onOpen: onDrawerOpen, onClose: onDrawerClose } = useDisclosure();
  const [editingId, setEditingId] = useState<string | undefined>(undefined);
  const schema = useMemo(() => (editingId ? updateCompanySchema(t) : createCompanySchema(t)), [editingId, t]);
  const { register, control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<CompanyFormValues>({
    resolver: zodResolver(schema) as any,
    mode: 'onBlur',
    reValidateMode: 'onChange',
    defaultValues: emptyCompanyForm,
  });

  const { isOpen: isDeleteOpen, onOpen: onDeleteOpen, onClose: onDeleteClose } = useDisclosure();
  const [deletingId, setDeletingId] = useState<string | undefined>(undefined);

  const loadPlans = useCallback(async () => {
    const plansResponse = await plansService.getPlans({ page: 1, pageSize: 100, isActive: true });
    setPlans(plansResponse.data || []);
  }, []);

  const load = useCallback(async () => {
    setTableLoading(true);
    try {
      const companiesResponse = await companiesService.getCompanies({ page, pageSize, keyword: searchQuery, isActive: statusFilterToBoolean(statusFilter) });
      setRows(companiesResponse.data || []);
      setTotalCount(companiesResponse.totalCount ?? companiesResponse.data?.length ?? 0);
      await loadPlans();
    } catch {
      toast({ title: t('common.error'), description: t('companies.loadError'), status: 'error', duration: 3000, isClosable: true });
    } finally {
      setTableLoading(false);
    }
  }, [loadPlans, page, pageSize, searchQuery, statusFilter, t, toast]);

  useEffect(() => { void load(); }, [load]);

  const filteredRows = useMemo(() => {
    if (planFilter === 'all') return rows;
    const planName = plans.find((plan) => plan.id.toString() === planFilter)?.name;
    return rows.filter((row) => row.currentPlanName === planName);
  }, [planFilter, plans, rows]);

  const handleAddNew = () => {
    setEditingId(undefined);
    reset(emptyCompanyForm);
    onDrawerOpen();
  };

  const handleEdit = (company: Company) => {
    setEditingId(company.id.toString());
    reset({
      name: company.name,
      code: company.code,
      email: company.email || '',
      phoneNumber: company.phoneNumber || '',
      address: company.address || '',
      planId: company.currentPlanId || '',
      adminEmail: '',
      adminPassword: '',
      isActive: company.isActive,
    });
    onDrawerOpen();
  };

  const onSubmit = handleSubmit(async (values) => {
    const payload = editingId
      ? { name: values.name, code: values.code, email: values.email || null, phoneNumber: values.phoneNumber || null, address: values.address || null, isActive: values.isActive, planId: values.planId || null }
      : { ...values, email: values.email || null, phoneNumber: values.phoneNumber || null, address: values.address || null, planId: values.planId || null };
    try {
      await companiesService.saveCompany(payload, editingId);
      toast({ title: editingId ? t('companies.updated') : t('companies.registered'), status: 'success', duration: 2500, isClosable: true });
      onDrawerClose();
      await load();
    } catch (err) {
      toast({ title: t('companies.saveError'), description: translateServerMessage(err, t('common.error')), status: 'error', duration: 4500, isClosable: true });
    }
  });

  const handleDeletePrompt = (company: Company) => { setDeletingId(company.id.toString()); onDeleteOpen(); };
  const handleDeleteConfirm = async () => {
    if (!deletingId) return;
    setDeleteLoading(true);
    try {
      await companiesService.deleteCompany(deletingId);
      toast({ title: t('companies.deleted'), status: 'success', duration: 2500, isClosable: true });
      await load();
    } catch {
      toast({ title: t('companies.deleteError'), status: 'error', duration: 3000, isClosable: true });
    } finally {
      setDeleteLoading(false);
      setDeletingId(undefined);
    }
  };

  return (
    <Flex direction="column" w="100%">
      <SharedPageHeader title={t('companies.pageTitle')} description={t('companies.pageDesc')} action={<Button leftIcon={<Icon as={MdAdd as React.ElementType} />} variant="brand" onClick={handleAddNew} w={{ base: '100%', md: 'auto' }}>{t('companies.addCompany')}</Button>} />
      <SharedToolbar
        searchValue={searchQuery}
        onSearchChange={(value) => { setSearchQuery(value); setPage(1); }}
        searchPlaceholder={t('companies.searchPlaceholder')}
        statusValue={statusFilter}
        onStatusChange={(value) => { setStatusFilter(value as StatusFilterValue); setPage(1); }}
        statusOptions={defaultStatusOptions}
        onRefresh={load}
        isRefreshing={tableLoading}
        extraFilters={<Select value={planFilter} onChange={(event) => setPlanFilter(event.target.value)} maxW={{ base: '100%', md: '180px' }} borderRadius="8px"><option value="all">{t('companies.allPlans')}</option>{plans.map((plan) => <option key={plan.id} value={plan.id}>{plan.name}</option>)}</Select>}
      />
      <SharedDataTable
        columns={[{ key: 'name', header: t('companies.companyName'), isSortable: true }, { key: 'code', header: t('companies.uniqueCode'), isSortable: true }, { key: 'email', header: t('companies.email') }, { key: 'currentPlanName', header: t('companies.planAssignment'), render: (row) => row.currentPlanName || t('branches.notSet') }, { key: 'isActive', header: t('common.status'), render: (row) => <SharedStatusBadge isActive={row.isActive} />, getSortValue: (row) => row.isActive }]}
        data={filteredRows}
        isLoading={tableLoading}
        emptyTitle={t('companies.emptyTitle')}
        emptyDescription={t('companies.emptyDesc')}
        emptyActionLabel={t('companies.emptyAction')}
        onEmptyAction={handleAddNew}
        isFiltered={searchQuery !== '' || planFilter !== 'all' || statusFilter !== 'all'}
        onClearFilters={() => { setSearchQuery(''); setPlanFilter('all'); setStatusFilter('all'); setPage(1); }}
        actions={[{ label: t('common.edit'), onClick: handleEdit }, { label: t('common.delete'), onClick: handleDeletePrompt, isDestructive: true }]}
        page={page}
        pageSize={pageSize}
        totalCount={planFilter === 'all' ? totalCount : filteredRows.length}
        onPageChange={setPage}
        onPageSizeChange={(nextPageSize) => { setPageSize(nextPageSize); setPage(1); }}
      />
      <SharedModal isOpen={isDrawerOpen} onClose={onDrawerClose} title={editingId ? t('companies.titleEdit') : t('companies.titleAdd')} description={t('companies.desc')} isLoading={isSubmitting} initialFocusRef={initialInputRef} footer={<><Button variant="ghost" onClick={onDrawerClose} isDisabled={isSubmitting}>{t('common.cancel')}</Button><Button type="submit" form="company-form" variant="brand" isLoading={isSubmitting} loadingText={t('companies.saving')}>{t('companies.saveCompany')}</Button></>}>
        <form id="company-form" onSubmit={onSubmit} noValidate>
          <VStack spacing="16px" align="stretch">
            <SharedInput label={t('companies.companyName')} isRequired error={errors.name?.message} placeholder={t('companies.namePlaceholder')} {...register('name')} />
            <SharedInput label={t('companies.uniqueCode')} isRequired isDisabled={!!editingId} error={errors.code?.message} placeholder={t('companies.codePlaceholder')} {...register('code')} />
            <SharedInput label={t('companies.email')} type="email" error={errors.email?.message} placeholder={t('companies.emailPlaceholder')} {...register('email')} />
            <SharedInput label={t('companies.phoneNumber')} error={errors.phoneNumber?.message} placeholder={t('companies.phonePlaceholder')} {...register('phoneNumber')} />
            <SharedInput label={t('companies.address')} error={errors.address?.message} placeholder={t('companies.addressPlaceholder')} {...register('address')} />
            <SharedSelect label={t('companies.planAssignment')} placeholderLabel={t('companies.selectPlan')} options={plans.map((plan) => ({ label: plan.name, value: plan.id }))} error={errors.planId?.message} {...register('planId')} />
            {!editingId && <><SharedInput label={t('companies.adminEmail')} type="email" isRequired error={errors.adminEmail?.message} placeholder={t('companies.adminEmailPlaceholder')} {...register('adminEmail')} /><SharedInput label={t('companies.adminPassword')} type="password" isRequired error={errors.adminPassword?.message} placeholder={t('common.securePassword')} {...register('adminPassword')} /></>}
            <Controller name="isActive" control={control} render={({ field }) => <SharedCheckbox label={t('companies.activeCompany')} isChecked={field.value} onChange={(event) => field.onChange(event.target.checked)} />} />
          </VStack>
        </form>
      </SharedModal>
      <SharedConfirmDialog isOpen={isDeleteOpen} onClose={onDeleteClose} onConfirm={handleDeleteConfirm} title={t('companies.deleteTitle')} message={t('companies.deleteMessage')} isDestructive loading={deleteLoading} />
    </Flex>
  );
}

