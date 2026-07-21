import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { Box, Button, Divider, Flex, Heading, Icon, SimpleGrid, useColorModeValue, useDisclosure, useToast, VStack } from '@chakra-ui/react';
import { MdAdd } from 'react-icons/md';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

import SharedDataTable from 'shared/components/ui/SharedDataTable';
import SharedPageHeader from 'shared/components/ui/SharedPageHeader';
import SharedToolbar from 'shared/components/ui/SharedToolbar';
import SharedModal from 'shared/components/ui/SharedModal';
import SharedConfirmDialog from 'shared/components/ui/SharedConfirmDialog';
import SharedStatusBadge from 'shared/components/ui/SharedStatusBadge';
import { SharedCheckbox, SharedInput, SharedSelect, SharedTextarea } from 'shared/components/ui/SharedFormElements';
import { defaultStatusOptions, statusFilterToBoolean, StatusFilterValue } from 'shared/utils/filterUtils';
import { useTranslation } from 'react-i18next';
import { translateServerMessage } from 'shared/utils/errorUtils';

import plansService from '../services/plansService';
import type { SubscriptionPlan } from 'shared/types/api';
import { PlanFormValues, planSchema } from '../validations/planSchema';

const emptyPlanForm: PlanFormValues = { name: '', description: '', price: 0, billingPeriod: 'Monthly', maxBranches: 1, maxEmployees: 1, maxHRUsers: 1, maxCompanyAdmins: 1, isActive: true };

export default function PlansPage() {
  const toast = useToast();
  const { t } = useTranslation();
  const initialInputRef = useRef<HTMLInputElement>(null);
  const borderColor = useColorModeValue('gray.200', 'whiteAlpha.100');
  const schema = useMemo(() => planSchema(t), [t]);

  const [rows, setRows] = useState<SubscriptionPlan[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [tableLoading, setTableLoading] = useState(false);
  const [deleteLoading, setDeleteLoading] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<StatusFilterValue>('all');
  const { isOpen: isDrawerOpen, onOpen: onDrawerOpen, onClose: onDrawerClose } = useDisclosure();
  const [editingId, setEditingId] = useState<string | undefined>(undefined);
  const { register, control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<PlanFormValues>({ resolver: zodResolver(schema) as any, mode: 'onBlur', reValidateMode: 'onChange', defaultValues: emptyPlanForm });
  const { isOpen: isDeleteOpen, onOpen: onDeleteOpen, onClose: onDeleteClose } = useDisclosure();
  const [deletingId, setDeletingId] = useState<string | undefined>(undefined);

  const load = useCallback(async () => {
    setTableLoading(true);
    try {
      const response = await plansService.getPlans({ page, pageSize, isActive: statusFilterToBoolean(statusFilter) });
      setRows(response.data || []);
      setTotalCount(response.totalCount ?? response.data?.length ?? 0);
    } catch {
      toast({ title: t('common.error'), description: t('plans.loadError'), status: 'error', duration: 3000, isClosable: true });
    } finally {
      setTableLoading(false);
    }
  }, [page, pageSize, statusFilter, t, toast]);

  useEffect(() => { void load(); }, [load]);

  const filteredRows = useMemo(() => {
    const query = searchQuery.trim().toLowerCase();
    if (!query) return rows;
    return rows.filter((row) => row.name.toLowerCase().includes(query) || row.description.toLowerCase().includes(query));
  }, [rows, searchQuery]);

  const handleAddNew = () => { setEditingId(undefined); reset(emptyPlanForm); onDrawerOpen(); };
  const handleEdit = (plan: SubscriptionPlan) => { setEditingId(plan.id.toString()); reset({ ...emptyPlanForm, ...plan }); onDrawerOpen(); };

  const onSubmit = handleSubmit(async (values) => {
    try {
      await plansService.savePlan(values, editingId);
      toast({ title: editingId ? t('plans.updated') : t('plans.created'), status: 'success', duration: 2500, isClosable: true });
      onDrawerClose();
      await load();
    } catch (err) {
      toast({ title: t('plans.saveError'), description: translateServerMessage(err, t('common.error')), status: 'error', duration: 4500, isClosable: true });
    }
  });

  const handleDeletePrompt = (plan: SubscriptionPlan) => { setDeletingId(plan.id.toString()); onDeleteOpen(); };
  const handleDeleteConfirm = async () => {
    if (!deletingId) return;
    setDeleteLoading(true);
    try { await plansService.deletePlan(deletingId); toast({ title: t('plans.deleted'), status: 'success', duration: 2500, isClosable: true }); await load(); }
    catch { toast({ title: t('plans.deleteError'), status: 'error', duration: 3000, isClosable: true }); }
    finally { setDeleteLoading(false); setDeletingId(undefined); }
  };

  return (
    <Flex direction="column" w="100%">
      <SharedPageHeader title={t('plans.pageTitle')} description={t('plans.pageDesc')} action={<Button leftIcon={<Icon as={MdAdd as React.ElementType} />} variant="brand" onClick={handleAddNew} w={{ base: '100%', md: 'auto' }}>{t('plans.addPlan')}</Button>} />
      <SharedToolbar searchValue={searchQuery} onSearchChange={setSearchQuery} searchPlaceholder={t('plans.searchPlaceholder')} statusValue={statusFilter} onStatusChange={(value) => { setStatusFilter(value as StatusFilterValue); setPage(1); }} statusOptions={defaultStatusOptions} onRefresh={load} isRefreshing={tableLoading} />
      <SharedDataTable columns={[{ key: 'name', header: t('plans.planName'), isSortable: true }, { key: 'price', header: t('plans.price'), isSortable: true, render: (row) => `$${row.price}`, getSortValue: (row) => row.price }, { key: 'billingPeriod', header: t('plans.billingPeriod'), isSortable: true }, { key: 'maxBranches', header: t('plans.maxBranches'), isNumeric: true, isSortable: true }, { key: 'maxEmployees', header: t('plans.maxEmployees'), isNumeric: true, isSortable: true }, { key: 'isActive', header: t('common.status'), render: (row) => <SharedStatusBadge isActive={row.isActive} />, getSortValue: (row) => row.isActive }]} data={filteredRows} isLoading={tableLoading} emptyTitle={t('plans.emptyTitle')} emptyDescription={t('plans.emptyDesc')} emptyActionLabel={t('plans.emptyAction')} onEmptyAction={handleAddNew} isFiltered={searchQuery !== '' || statusFilter !== 'all'} onClearFilters={() => { setSearchQuery(''); setStatusFilter('all'); setPage(1); }} actions={[{ label: t('common.edit'), onClick: handleEdit }, { label: t('common.delete'), onClick: handleDeletePrompt, isDestructive: true }]} page={page} pageSize={pageSize} totalCount={searchQuery ? filteredRows.length : totalCount} onPageChange={setPage} onPageSizeChange={(nextPageSize) => { setPageSize(nextPageSize); setPage(1); }} />
      <SharedModal isOpen={isDrawerOpen} onClose={onDrawerClose} title={editingId ? t('plans.titleEdit') : t('plans.titleAdd')} description={t('plans.desc')} isLoading={isSubmitting} initialFocusRef={initialInputRef} footer={<><Button variant="ghost" onClick={onDrawerClose} isDisabled={isSubmitting}>{t('common.cancel')}</Button><Button type="submit" form="plan-form" variant="brand" isLoading={isSubmitting} loadingText={t('plans.saving')}>{t('plans.savePlan')}</Button></>}>
        <form id="plan-form" onSubmit={onSubmit} noValidate>
          <VStack spacing="24px" align="stretch">
            <Box><Heading as="h4" size="xs" textTransform="uppercase" letterSpacing="0.5px" color="secondaryGray.600" mb="12px">{t('plans.generalInfo')}</Heading><VStack spacing="14px" align="stretch"><SharedInput label={t('plans.planName')} isRequired error={errors.name?.message} placeholder={t('plans.namePlaceholder')} {...register('name')} /><SharedTextarea label={t('plans.description')} error={errors.description?.message} placeholder={t('plans.descriptionPlaceholder')} {...register('description')} /></VStack></Box>
            <Divider borderColor={borderColor} />
            <Box><Heading as="h4" size="xs" textTransform="uppercase" letterSpacing="0.5px" color="secondaryGray.600" mb="12px">{t('plans.pricing')}</Heading><SimpleGrid columns={{ base: 1, md: 2 }} gap="14px"><SharedInput label={t('plans.price')} type="number" isRequired error={errors.price?.message} placeholder="0.00" {...register('price')} /><SharedSelect label={t('plans.billingPeriod')} isRequired error={errors.billingPeriod?.message} placeholderLabel={t('plans.selectPeriod')} options={[{ value: 'Monthly', label: t('plans.monthly') }, { value: 'Annual', label: t('plans.annual') }, { value: 'Weekly', label: t('plans.weekly') }, { value: 'Lifetime', label: t('plans.lifetime') }]} {...register('billingPeriod')} /></SimpleGrid></Box>
            <Divider borderColor={borderColor} />
            <Box><Heading as="h4" size="xs" textTransform="uppercase" letterSpacing="0.5px" color="secondaryGray.600" mb="12px">{t('plans.usageLimits')}</Heading><SimpleGrid columns={{ base: 1, md: 2 }} gap="14px"><SharedInput label={t('plans.maxEmployees')} type="number" isRequired error={errors.maxEmployees?.message} placeholder="10" {...register('maxEmployees')} /><SharedInput label={t('plans.maxBranches')} type="number" isRequired error={errors.maxBranches?.message} placeholder="2" {...register('maxBranches')} /><SharedInput label={t('plans.maxCompanyAdmins')} type="number" isRequired error={errors.maxCompanyAdmins?.message} placeholder="1" {...register('maxCompanyAdmins')} /><SharedInput label={t('plans.maxHRStaff')} type="number" isRequired error={errors.maxHRUsers?.message} placeholder="2" {...register('maxHRUsers')} /></SimpleGrid></Box>
            <Divider borderColor={borderColor} />
            <Box><Heading as="h4" size="xs" textTransform="uppercase" letterSpacing="0.5px" color="secondaryGray.600" mb="12px">{t('plans.status')}</Heading><Controller name="isActive" control={control} render={({ field }) => <SharedCheckbox label={t('plans.activePlan')} isChecked={field.value} onChange={(event) => field.onChange(event.target.checked)} />} /></Box>
          </VStack>
        </form>
      </SharedModal>
      <SharedConfirmDialog isOpen={isDeleteOpen} onClose={onDeleteClose} onConfirm={handleDeleteConfirm} title={t('plans.deleteTitle')} message={t('plans.deleteMessage')} isDestructive loading={deleteLoading} />
    </Flex>
  );
}

