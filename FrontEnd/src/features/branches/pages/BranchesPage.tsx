import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { Badge, Button, Flex, Icon, Select, SimpleGrid, useDisclosure, useToast, VStack } from '@chakra-ui/react';
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
import { useAuth } from 'shared/contexts/AuthContext';
import { hasPermission, isPlatformAdmin } from 'shared/utils/authUtils';
import { useTranslation } from 'react-i18next';
import { translateServerMessage } from 'shared/utils/errorUtils';

import BranchLocationPicker from '../components/BranchLocationPicker';
import branchesService from '../services/branchesService';
import companiesService from 'features/companies/services/companiesService';
import workSchedulesService from 'features/work-schedules/services/workSchedulesService';
import type { WorkSchedule } from 'features/work-schedules/types';
import type { Branch, Company } from 'shared/types/api';
import { BranchFormValues, branchSchema } from '../validations/branchSchema';

const emptyBranchForm: BranchFormValues = { companyId: '', workScheduleId: '', name: '', code: '', address: '', latitude: 0, longitude: 0, allowedRadius: 100, isMainBranch: false, isActive: true };

export default function BranchesPage() {
  const toast = useToast();
  const { t } = useTranslation();
  const initialInputRef = useRef<HTMLInputElement>(null);
  const { user } = useAuth();
  const isSuperAdmin = isPlatformAdmin(user);
  const canCreateBranch = hasPermission(user, 'Branches.Create');
  const canEditBranch = hasPermission(user, 'Branches.Edit');
  const canDeleteBranch = hasPermission(user, 'Branches.Delete');
  const schema = useMemo(() => branchSchema(t, !!isSuperAdmin), [isSuperAdmin, t]);

  const [rows, setRows] = useState<Branch[]>([]);
  const [companies, setCompanies] = useState<Company[]>([]);
  const [workSchedules, setWorkSchedules] = useState<WorkSchedule[]>([]);
  const [selectedCompanyId, setSelectedCompanyId] = useState<string>(localStorage.getItem('active_company_id') || 'all');
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [tableLoading, setTableLoading] = useState(false);
  const [deleteLoading, setDeleteLoading] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<StatusFilterValue>('all');
  const { isOpen: isDrawerOpen, onOpen: onDrawerOpen, onClose: onDrawerClose } = useDisclosure();
  const [editingId, setEditingId] = useState<string | undefined>(undefined);
  const { register, control, handleSubmit, reset, setValue, watch, formState: { errors, isSubmitting } } = useForm<BranchFormValues>({ resolver: zodResolver(schema) as any, mode: 'onBlur', reValidateMode: 'onChange', defaultValues: emptyBranchForm });
  const watchedLocation = watch();
  const { isOpen: isDeleteOpen, onOpen: onDeleteOpen, onClose: onDeleteClose } = useDisclosure();
  const [deletingId, setDeletingId] = useState<string | undefined>(undefined);

  useEffect(() => {
    if (!isSuperAdmin) return;
    companiesService.getCompanies({ page: 1, pageSize: 100 }).then((response) => {
      const nextCompanies = response.data || [];
      setCompanies(nextCompanies);
    });
  }, [isSuperAdmin]);

  const targetCompanyId = watch('companyId') || (isSuperAdmin ? selectedCompanyId : '');

  useEffect(() => {
    if (isSuperAdmin && (!targetCompanyId || targetCompanyId === 'all')) {
      setWorkSchedules([]);
      return;
    }
    const params = isSuperAdmin && targetCompanyId && targetCompanyId !== 'all' ? { companyId: targetCompanyId } : {};
    workSchedulesService.getWorkSchedules({ page: 1, pageSize: 200, isActive: true, ...params })
      .then((res) => {
        const list = res.data || [];
        setWorkSchedules(list);
        const currentScheduleId = watch('workScheduleId');
        if (!currentScheduleId && list.length > 0) {
          const defaultWs = list.find((w) => w.isDefault) || list[0];
          if (defaultWs) setValue('workScheduleId', defaultWs.id);
        }
      })
      .catch(() => setWorkSchedules([]));
  }, [isSuperAdmin, targetCompanyId, watch, setValue]);

  const load = useCallback(async (refresh = false) => {
    setTableLoading(true);
    try {
      const response = await branchesService.getBranches({ page, pageSize, keyword: searchQuery, isActive: statusFilterToBoolean(statusFilter), refresh });
      setRows(response.data || []);
      setTotalCount(response.totalCount ?? response.data?.length ?? 0);
    } catch {
      toast({ title: t('common.error'), description: t('branches.loadError'), status: 'error', duration: 3000, isClosable: true });
    } finally { setTableLoading(false); }
  }, [page, pageSize, searchQuery, selectedCompanyId, statusFilter, t, toast]);
  useEffect(() => { void load(); }, [load]);

  const handleAddNew = () => {
    if (!canCreateBranch) return;
    setEditingId(undefined);
    const defaultWsId = workSchedules.find((w) => w.isDefault)?.id || workSchedules[0]?.id || '';
    reset({ ...emptyBranchForm, companyId: isSuperAdmin && selectedCompanyId !== 'all' ? selectedCompanyId : '', workScheduleId: defaultWsId });
    onDrawerOpen();
  };
  const handleEdit = (branch: Branch) => {
    if (!canEditBranch) return;
    setEditingId(branch.id.toString());
    const defaultWsId = workSchedules.find((w) => w.isDefault)?.id || workSchedules[0]?.id || '';
    reset({ ...branch, companyId: branch.companyId?.toString() || selectedCompanyId, workScheduleId: branch.workScheduleId || defaultWsId });
    onDrawerOpen();
  };

  const onSubmit = handleSubmit(async (values) => {
    const payload = { ...values, companyId: isSuperAdmin ? values.companyId : undefined };
    try {
      await branchesService.saveBranch(payload, editingId);
      if (isSuperAdmin && values.companyId) { setSelectedCompanyId(values.companyId); localStorage.setItem('active_company_id', values.companyId); }
      toast({ title: editingId ? t('branches.updated') : t('branches.created'), status: 'success', duration: 2500, isClosable: true });
      onDrawerClose();
      await load();
    } catch (error) {
      toast({ title: t('branches.saveError'), description: translateServerMessage(error, t('branches.saveErrorDesc')), status: 'error', duration: 4500, isClosable: true });
    }
  });

  const handleDeletePrompt = (branch: Branch) => { if (!canDeleteBranch) return; setDeletingId(branch.id.toString()); onDeleteOpen(); };
  const handleDeleteConfirm = async () => {
    if (!deletingId) return;
    setDeleteLoading(true);
    try { await branchesService.deleteBranch(deletingId); toast({ title: t('branches.deleted'), status: 'success', duration: 2500, isClosable: true }); await load(); }
    catch { toast({ title: t('branches.deleteError'), status: 'error', duration: 3000, isClosable: true }); }
    finally { setDeleteLoading(false); setDeletingId(undefined); }
  };

  const rowActions = [
    ...(canEditBranch ? [{ label: t('common.edit'), onClick: handleEdit }] : []),
    ...(canDeleteBranch ? [{ label: t('common.delete'), onClick: handleDeletePrompt, isDestructive: true }] : []),
  ];

  return (
    <Flex direction="column" w="100%">
      <SharedPageHeader title={t('branches.pageTitle')} description={t('branches.pageDesc')} action={canCreateBranch ? <Button leftIcon={<Icon as={MdAdd as React.ElementType} />} variant="brand" onClick={handleAddNew} w={{ base: '100%', md: 'auto' }}>{t('branches.addBranch')}</Button> : undefined} />
      <SharedToolbar
        searchValue={searchQuery}
        onSearchChange={(value) => { setSearchQuery(value); setPage(1); }}
        searchPlaceholder={t('branches.searchPlaceholder')}
        statusValue={statusFilter}
        onStatusChange={(value) => { setStatusFilter(value as StatusFilterValue); setPage(1); }}
        statusOptions={defaultStatusOptions}
        onRefresh={() => void load(true)}
        isRefreshing={tableLoading}
        extraFilters={
          isSuperAdmin ? (
            <Select
              value={selectedCompanyId}
              onChange={(event) => {
                const id = event.target.value;
                setSelectedCompanyId(id);
                localStorage.setItem('active_company_id', id);
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
          ) : undefined
        }
      />
      <SharedDataTable columns={[{ key: 'name', header: t('branches.branchName'), isSortable: true }, { key: 'code', header: t('branches.branchCode'), isSortable: true }, { key: 'address', header: t('branches.addressHeader'), minW: '220px' }, { key: 'allowedRadius', header: t('branches.radiusHeader'), isNumeric: true, isSortable: true }, { key: 'workScheduleName', header: t('branches.shiftName'), render: (row) => row.workScheduleName || "الشيفت الصباحي" }, { key: 'isMainBranch', header: t('branches.mainHeader'), render: (row) => (row.isMainBranch ? <Badge colorScheme="brandScheme">{t('branches.mainHeader')}</Badge> : null), getSortValue: (row) => row.isMainBranch }, { key: 'isActive', header: t('common.status'), render: (row) => <SharedStatusBadge isActive={row.isActive} />, getSortValue: (row) => row.isActive }]} data={rows} isLoading={tableLoading} emptyTitle={t('branches.emptyTitle')} emptyDescription={t('branches.emptyDesc')} emptyActionLabel={canCreateBranch ? t('branches.emptyAction') : undefined} onEmptyAction={canCreateBranch ? handleAddNew : undefined} isFiltered={searchQuery !== '' || statusFilter !== 'all'} onClearFilters={() => { setSearchQuery(''); setStatusFilter('all'); setPage(1); }} actions={rowActions} page={page} pageSize={pageSize} totalCount={totalCount} onPageChange={setPage} onPageSizeChange={(nextPageSize) => { setPageSize(nextPageSize); setPage(1); }} />
      <SharedModal isOpen={isDrawerOpen} onClose={onDrawerClose} title={editingId ? t('branches.titleEdit') : t('branches.titleAdd')} description={t('branches.desc')} isLoading={isSubmitting} initialFocusRef={initialInputRef} footer={<><Button variant="ghost" onClick={onDrawerClose} isDisabled={isSubmitting}>{t('common.cancel')}</Button><Button type="submit" form="branch-form" variant="brand" isLoading={isSubmitting} loadingText={t('branches.saving')}>{t('branches.saveBranch')}</Button></>}>
        <form id="branch-form" onSubmit={onSubmit} noValidate>
          <VStack spacing="16px" align="stretch">
            {isSuperAdmin && <SharedSelect label={t('branches.companyLabel')} isRequired placeholderLabel={t('branches.companyPlaceholder')} options={companies.map((company) => ({ value: company.id, label: company.name }))} error={errors.companyId?.message} {...register('companyId')} />}
            <SharedSelect
              label="جدول مواعيد العمل (الشيفت)"
              isRequired
              placeholderLabel={isSuperAdmin && (!targetCompanyId || targetCompanyId === 'all') ? "اختر الشركة أولاً" : undefined}
              isDisabled={isSuperAdmin && (!targetCompanyId || targetCompanyId === 'all')}
              options={workSchedules.map((ws) => ({ value: ws.id, label: ws.name }))}
              error={errors.workScheduleId?.message}
              {...register('workScheduleId')}
            />
            <SharedInput label={t('branches.branchName')} isRequired error={errors.name?.message} placeholder={t('branches.namePlaceholder')} {...register('name')} />
            <SharedInput label={t('branches.branchCode')} isRequired error={errors.code?.message} placeholder={t('branches.codePlaceholder')} {...register('code')} />
            <SharedInput label={t('branches.geographicalAddress')} isRequired error={errors.address?.message} placeholder={t('branches.addressPlaceholder')} {...register('address')} />
            <BranchLocationPicker latitude={watchedLocation.latitude} longitude={watchedLocation.longitude} radius={watchedLocation.allowedRadius} address={watchedLocation.address} onChange={(location) => { setValue('latitude', location.latitude, { shouldDirty: true, shouldValidate: true }); setValue('longitude', location.longitude, { shouldDirty: true, shouldValidate: true }); if (location.address) setValue('address', location.address, { shouldDirty: true, shouldValidate: true }); }} />
            <SimpleGrid columns={{ base: 1, md: 2 }} gap="14px"><SharedInput label={t('branches.latitude')} type="number" step="any" isRequired error={errors.latitude?.message} {...register('latitude')} /><SharedInput label={t('branches.longitude')} type="number" step="any" isRequired error={errors.longitude?.message} {...register('longitude')} /></SimpleGrid>
            <SharedInput label={t('branches.allowedGeofence')} type="number" isRequired error={errors.allowedRadius?.message} {...register('allowedRadius')} />
            <Controller name="isMainBranch" control={control} render={({ field }) => <SharedCheckbox label={t('branches.setMainBranch')} isChecked={field.value} onChange={(event) => field.onChange(event.target.checked)} />} />
            <Controller name="isActive" control={control} render={({ field }) => <SharedCheckbox label={t('branches.activeBranch')} isChecked={field.value} onChange={(event) => field.onChange(event.target.checked)} />} />
          </VStack>
        </form>
      </SharedModal>
      <SharedConfirmDialog isOpen={isDeleteOpen} onClose={onDeleteClose} onConfirm={handleDeleteConfirm} title={t('branches.deleteTitle')} message={t('branches.deleteMessage')} isDestructive loading={deleteLoading} />
    </Flex>
  );
}


