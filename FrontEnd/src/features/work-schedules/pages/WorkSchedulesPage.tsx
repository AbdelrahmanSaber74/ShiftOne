import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { Badge, Box, Button, Divider, Flex, Heading, HStack, Icon, SimpleGrid, useDisclosure, useToast, VStack } from '@chakra-ui/react';
import { MdAdd, MdStar } from 'react-icons/md';
import { Controller, useFieldArray, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useTranslation } from 'react-i18next';

import SharedDataTable from 'shared/components/ui/SharedDataTable';
import SharedPageHeader from 'shared/components/ui/SharedPageHeader';
import SharedToolbar from 'shared/components/ui/SharedToolbar';
import SharedModal from 'shared/components/ui/SharedModal';
import SharedConfirmDialog from 'shared/components/ui/SharedConfirmDialog';
import SharedStatusBadge from 'shared/components/ui/SharedStatusBadge';
import { SharedCheckbox, SharedInput, SharedSelect, SharedTextarea } from 'shared/components/ui/SharedFormElements';
import { defaultStatusOptions, statusFilterToBoolean, StatusFilterValue } from 'shared/utils/filterUtils';
import { useAuth } from 'shared/contexts/AuthContext';
import { hasPermission, isPlatformAdmin } from 'shared/utils/authUtils';
import { translateServerMessage } from 'shared/utils/errorUtils';
import companiesService from 'features/companies/services/companiesService';
import branchesService from 'features/branches/services/branchesService';
import type { Branch, Company } from 'shared/types/api';

import workSchedulesService from '../services/workSchedulesService';
import type { WorkSchedule } from '../types';
import { WorkScheduleFormValues, workScheduleSchema } from '../validations/workScheduleSchema';

const dayValues = [0, 1, 2, 3, 4, 5, 6];
const defaultDays = dayValues.map((dayOfWeek) => ({ dayOfWeek, isWorkingDay: dayOfWeek !== 5, startTime: '09:00', endTime: '17:00', lateGraceMinutes: 10, earlyLeaveGraceMinutes: 10, minimumWorkingMinutes: 480, overtimeEnabled: true }));
const emptyForm: WorkScheduleFormValues = { companyId: '', branchId: '', name: '', description: '', timeZoneId: 'Arab Standard Time', isDefault: false, isActive: true, days: defaultDays };

export default function WorkSchedulesPage() {
  const { t } = useTranslation();
  const toast = useToast();
  const { user } = useAuth();
  const initialInputRef = useRef<HTMLInputElement>(null);
  const isPlatformUser = isPlatformAdmin(user);
  const canCreate = hasPermission(user, 'WorkSchedules.Create');
  const canEdit = hasPermission(user, 'WorkSchedules.Edit');
  const canDelete = hasPermission(user, 'WorkSchedules.Delete');
  const canAssign = hasPermission(user, 'WorkSchedules.Assign');
  const schema = useMemo(() => workScheduleSchema(t, isPlatformUser), [isPlatformUser, t]);

  const [rows, setRows] = useState<WorkSchedule[]>([]);
  const [companies, setCompanies] = useState<Company[]>([]);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [tableLoading, setTableLoading] = useState(false);
  const [deleteLoading, setDeleteLoading] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<StatusFilterValue>('all');
  const [editingId, setEditingId] = useState<string | undefined>();
  const [deletingId, setDeletingId] = useState<string | undefined>();
  const { isOpen: isDrawerOpen, onOpen: onDrawerOpen, onClose: onDrawerClose } = useDisclosure();
  const { isOpen: isDeleteOpen, onOpen: onDeleteOpen, onClose: onDeleteClose } = useDisclosure();

  const { register, control, handleSubmit, reset, setValue, watch, formState: { errors, isSubmitting } } = useForm<WorkScheduleFormValues>({ resolver: zodResolver(schema) as any, mode: 'onBlur', reValidateMode: 'onChange', defaultValues: emptyForm });
  const { fields } = useFieldArray({ control, name: 'days' });
  const selectedCompanyId = watch('companyId');
  const prevCompanyIdRef = useRef(selectedCompanyId);

  useEffect(() => {
    if (!isPlatformUser) return;
    companiesService.getCompanies({ page: 1, pageSize: 200, isActive: true }).then((response) => setCompanies(response.data || []));
  }, [isPlatformUser]);



  const load = useCallback(async () => {
    setTableLoading(true);
    try {
      const response = await workSchedulesService.getWorkSchedules({ page, pageSize, keyword: searchQuery, isActive: statusFilterToBoolean(statusFilter) });
      setRows(response.data || []);
      setTotalCount(response.totalCount ?? response.data?.length ?? 0);
    } catch {
      toast({ title: t('common.error'), description: t('workSchedules.loadError'), status: 'error', duration: 3000, isClosable: true });
    } finally { setTableLoading(false); }
  }, [page, pageSize, searchQuery, statusFilter, t, toast]);

  useEffect(() => { void load(); }, [load]);

  const handleAddNew = () => { if (!canCreate) return; setEditingId(undefined); reset(emptyForm); onDrawerOpen(); };
  const handleEdit = (item: WorkSchedule) => {
    if (!canEdit) return;
    setEditingId(item.id);
    reset({
      companyId: item.companyId?.toString() || selectedCompanyId || '',
      name: item.name || '',
      description: item.description || '',
      timeZoneId: item.timeZoneId || 'Arab Standard Time',
      isDefault: Boolean(item.isDefault),
      isActive: Boolean(item.isActive),
      days: item.days?.length
        ? item.days.map((day) => ({
            ...day,
            dayOfWeek: normalizeDayOfWeek(day.dayOfWeek),
            isWorkingDay: Boolean(day.isWorkingDay),
            startTime: normalizeTime(day.startTime),
            endTime: normalizeTime(day.endTime),
            lateGraceMinutes: Number(day.lateGraceMinutes || 0),
            earlyLeaveGraceMinutes: Number(day.earlyLeaveGraceMinutes || 0),
            minimumWorkingMinutes: Number(day.minimumWorkingMinutes || 0),
            overtimeEnabled: Boolean(day.overtimeEnabled),
          }))
        : defaultDays
    });
    onDrawerOpen();
  };

  const onFormError = (formErrors: any) => {
    console.error('Work schedule validation errors:', formErrors);
    const extractMessages = (obj: any): string[] => {
      let msgs: string[] = [];
      if (!obj) return msgs;
      if (typeof obj === 'object') {
        if (obj.message && typeof obj.message === 'string') {
          msgs.push(obj.message);
        }
        for (const key of Object.keys(obj)) {
          if (key !== 'message') {
            msgs = msgs.concat(extractMessages(obj[key]));
          }
        }
      }
      return msgs;
    };

    const allMsgs = Array.from(new Set(extractMessages(formErrors)));
    const detailMessage = allMsgs.length > 0 ? allMsgs.join(' | ') : '';

    toast({
      title: t('common.error'),
      description: detailMessage || 'يرجى التأكد من ملء جميع الحقول المطلوبة ومواعيد العمل بشكل صحيح',
      status: 'error',
      duration: 7000,
      isClosable: true,
    });
  };

  const onSubmit = handleSubmit(async (values) => {
    try {
      const sanitizedPayload = {
        ...values,
        days: values.days.map((day) => ({
          ...day,
          startTime: day.isWorkingDay ? formatTimeSpan(day.startTime) : null,
          endTime: day.isWorkingDay ? formatTimeSpan(day.endTime) : null,
        })),
      };
      await workSchedulesService.saveWorkSchedule(sanitizedPayload as any, editingId);
      toast({ title: editingId ? t('workSchedules.updated') : t('workSchedules.created'), status: 'success', duration: 2500, isClosable: true });
      onDrawerClose();
      await load();
    } catch (error) {
      toast({ title: t('workSchedules.saveError'), description: translateServerMessage(error, t('common.error')), status: 'error', duration: 4500, isClosable: true });
    }
  }, onFormError);

  const rowActions = [
    ...(canEdit ? [{ label: t('common.edit'), onClick: handleEdit }] : []),
    ...(canAssign ? [{ label: t('workSchedules.setDefault'), onClick: async (row: WorkSchedule) => { await workSchedulesService.setDefault(row.id); await load(); } }] : []),
    ...(canDelete ? [{ label: t('common.delete'), onClick: (row: WorkSchedule) => { setDeletingId(row.id); onDeleteOpen(); }, isDestructive: true }] : []),
  ];

  const confirmDelete = async () => {
    if (!deletingId) return;
    setDeleteLoading(true);
    try { await workSchedulesService.deleteWorkSchedule(deletingId); toast({ title: t('workSchedules.deleted'), status: 'success', duration: 2500, isClosable: true }); await load(); }
    catch { toast({ title: t('workSchedules.deleteError'), status: 'error', duration: 3000, isClosable: true }); }
    finally { setDeleteLoading(false); setDeletingId(undefined); onDeleteClose(); }
  };

  return (
    <Flex direction="column" w="100%">
      <SharedPageHeader title={t('workSchedules.pageTitle')} description={t('workSchedules.pageDesc')} action={canCreate ? <Button leftIcon={<Icon as={MdAdd as React.ElementType} />} variant="brand" onClick={handleAddNew}>{t('workSchedules.add')}</Button> : undefined} />
      <SharedToolbar searchValue={searchQuery} onSearchChange={(value) => { setSearchQuery(value); setPage(1); }} searchPlaceholder={t('workSchedules.searchPlaceholder')} statusValue={statusFilter} onStatusChange={(value) => { setStatusFilter(value as StatusFilterValue); setPage(1); }} statusOptions={defaultStatusOptions} onRefresh={load} isRefreshing={tableLoading} />
      <SharedDataTable columns={[{ key: 'name', header: t('workSchedules.name'), isSortable: true }, { key: 'companyName', header: t('workSchedules.company'), isSortable: true }, { key: 'workingDaysCount', header: t('workSchedules.workingDays'), isNumeric: true }, { key: 'isDefault', header: t('workSchedules.default'), render: (row) => row.isDefault ? <Badge colorScheme="blue" borderRadius="full"><Icon as={MdStar as React.ElementType} /> {t('workSchedules.default')}</Badge> : null }, { key: 'isActive', header: t('common.status'), render: (row) => <SharedStatusBadge isActive={row.isActive} /> }]} data={rows} isLoading={tableLoading} emptyTitle={t('workSchedules.emptyTitle')} emptyDescription={t('workSchedules.emptyDesc')} emptyActionLabel={canCreate ? t('workSchedules.emptyAction') : undefined} onEmptyAction={canCreate ? handleAddNew : undefined} isFiltered={searchQuery !== '' || statusFilter !== 'all'} onClearFilters={() => { setSearchQuery(''); setStatusFilter('all'); setPage(1); }} actions={rowActions} page={page} pageSize={pageSize} totalCount={totalCount} onPageChange={setPage} onPageSizeChange={(next) => { setPageSize(next); setPage(1); }} />
      <SharedModal isOpen={isDrawerOpen} onClose={onDrawerClose} title={editingId ? t('workSchedules.editTitle') : t('workSchedules.addTitle')} description={t('workSchedules.formDesc')} isLoading={isSubmitting} initialFocusRef={initialInputRef} size="4xl" footer={<><Button variant="ghost" onClick={onDrawerClose} isDisabled={isSubmitting}>{t('common.cancel')}</Button><Button type="button" onClick={() => void onSubmit()} variant="brand" isLoading={isSubmitting}>{t('workSchedules.save')}</Button></>}>
        <form id="work-schedule-form" onSubmit={onSubmit} noValidate>
          <VStack spacing="18px" align="stretch">
            {isPlatformUser && <SharedSelect label={t('workSchedules.company')} isRequired placeholderLabel={t('workSchedules.selectCompany')} options={companies.map((company) => ({ value: company.id, label: company.name }))} error={errors.companyId?.message} {...register('companyId')} />}
            <SharedInput label={t('workSchedules.name')} isRequired error={errors.name?.message} {...register('name')} />
            <SharedTextarea label={t('workSchedules.description')} error={errors.description?.message} {...register('description')} />
            <HStack spacing="24px" pt="4px"><Controller name="isDefault" control={control} render={({ field }) => <SharedCheckbox label={t('workSchedules.makeDefault')} isChecked={field.value} onChange={(event) => field.onChange(event.target.checked)} />} /><Controller name="isActive" control={control} render={({ field }) => <SharedCheckbox label={t('workSchedules.active')} isChecked={field.value} onChange={(event) => field.onChange(event.target.checked)} />} /></HStack>
            <Divider />
            <Heading as="h3" size="sm">{t('workSchedules.weeklyRules')}</Heading>
            {fields.map((field, index) => (
              <Box key={field.id} border="1px solid" borderColor="gray.200" borderRadius="10px" p="14px" bg="gray.50" _dark={{ bg: 'navy.800', borderColor: 'whiteAlpha.200' }}>
                <Flex justify="space-between" align="center" mb="12px">
                  <Controller
                    name={`days.${index}.isWorkingDay`}
                    control={control}
                    render={({ field: checkbox }) => (
                      <SharedCheckbox
                        label={t(`workSchedules.days.${field.dayOfWeek}`)}
                        isChecked={checkbox.value}
                        onChange={(event) => {
                          const checked = event.target.checked;
                          checkbox.onChange(checked);
                          if (checked) {
                            const currentStart = watch(`days.${index}.startTime`);
                            const currentEnd = watch(`days.${index}.endTime`);
                            if (!currentStart) setValue(`days.${index}.startTime`, '09:00');
                            if (!currentEnd) setValue(`days.${index}.endTime`, '17:00');
                          }
                        }}
                      />
                    )}
                  />
                  <Controller
                    name={`days.${index}.overtimeEnabled`}
                    control={control}
                    render={({ field: overtime }) => (
                      <SharedCheckbox
                        label={t('workSchedules.overtime')}
                        isChecked={overtime.value}
                        onChange={(event) => overtime.onChange(event.target.checked)}
                      />
                    )}
                  />
                </Flex>
                <SimpleGrid columns={{ base: 1, md: 5 }} gap="12px" alignItems="start">
                  <Box minW="130px">
                    <SharedInput label={t('workSchedules.start')} type="time" error={(errors.days?.[index] as any)?.startTime?.message} {...register(`days.${index}.startTime`)} />
                  </Box>
                  <Box minW="130px">
                    <SharedInput label={t('workSchedules.end')} type="time" error={(errors.days?.[index] as any)?.endTime?.message} {...register(`days.${index}.endTime`)} />
                  </Box>
                  <SharedInput label={t('workSchedules.lateGrace')} type="number" error={(errors.days?.[index] as any)?.lateGraceMinutes?.message} {...register(`days.${index}.lateGraceMinutes`)} />
                  <SharedInput label={t('workSchedules.earlyLeaveGrace')} type="number" error={(errors.days?.[index] as any)?.earlyLeaveGraceMinutes?.message} {...register(`days.${index}.earlyLeaveGraceMinutes`)} />
                  <SharedInput label={t('workSchedules.minimumMinutes')} type="number" error={(errors.days?.[index] as any)?.minimumWorkingMinutes?.message} {...register(`days.${index}.minimumWorkingMinutes`)} />
                </SimpleGrid>
              </Box>
            ))}
          </VStack>
        </form>
      </SharedModal>
      <SharedConfirmDialog isOpen={isDeleteOpen} onClose={onDeleteClose} onConfirm={confirmDelete} title={t('workSchedules.deleteTitle')} message={t('workSchedules.deleteMessage')} isDestructive loading={deleteLoading} />
    </Flex>
  );
}

const dayNameToNumber: Record<string, number> = {
  sunday: 0,
  monday: 1,
  tuesday: 2,
  wednesday: 3,
  thursday: 4,
  friday: 5,
  saturday: 6,
};

function normalizeDayOfWeek(val: any): number {
  if (typeof val === 'number') return val;
  if (typeof val === 'string') {
    const parsed = parseInt(val, 10);
    if (!isNaN(parsed)) return parsed;
    const lower = val.toLowerCase();
    if (lower in dayNameToNumber) return dayNameToNumber[lower];
  }
  return 0;
}

function normalizeTime(value?: any): string {
  if (!value) return '';
  const str = String(value).trim();
  const parts = str.split(':');
  if (parts.length >= 2) {
    const hours = parts[0].padStart(2, '0');
    const minutes = parts[1].padStart(2, '0');
    return `${hours}:${minutes}`;
  }
  return str;
}

function formatTimeSpan(timeStr?: string | null): string | null {
  if (!timeStr || timeStr.trim() === '') return null;
  const clean = timeStr.trim();
  const parts = clean.split(':');
  if (parts.length === 2) {
    return `${parts[0].padStart(2, '0')}:${parts[1].padStart(2, '0')}:00`;
  }
  if (parts.length >= 3) {
    return `${parts[0].padStart(2, '0')}:${parts[1].padStart(2, '0')}:${parts[2].slice(0, 2).padStart(2, '0')}`;
  }
  return null;
}
