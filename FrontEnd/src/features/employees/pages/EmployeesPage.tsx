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

import employeesService from '../services/employeesService';
import branchesService from 'features/branches/services/branchesService';
import companiesService from 'features/companies/services/companiesService';
import type { Branch, Company, Employee } from 'shared/types/api';
import { EmployeeFormValues, employeeSchema } from '../validations/employeeSchema';

const emptyEmployeeForm: EmployeeFormValues = { firstName: '', lastName: '', email: '', phoneNumber: '', companyId: '', branchId: '', role: 'Employee', password: '', isActive: true };

export default function EmployeesPage() {
  const toast = useToast();
  const { t } = useTranslation();
  const initialInputRef = useRef<HTMLInputElement>(null);
  const { user } = useAuth();
  const isSuperAdmin = isPlatformAdmin(user);
  const canCreateEmployee = hasPermission(user, 'Employees.Create');
  const canEditEmployee = hasPermission(user, 'Employees.Edit');
  const canDeleteEmployee = hasPermission(user, 'Employees.Delete');
  const canResetDevice = hasPermission(user, 'Devices.Reset');
  const canManageOperatorRoles = isSuperAdmin || user?.roles?.includes('CompanyAdmin');
  const [editingId, setEditingId] = useState<string | undefined>(undefined);
  const schema = useMemo(() => employeeSchema(t, !!isSuperAdmin, !!editingId), [editingId, isSuperAdmin, t]);

  const [rows, setRows] = useState<Employee[]>([]);
  const [branches, setBranches] = useState<Branch[]>([]);
  const [companies, setCompanies] = useState<Company[]>([]);
  const [selectedCompanyId, setSelectedCompanyId] = useState<string>(localStorage.getItem('active_company_id') || 'all');
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [tableLoading, setTableLoading] = useState(false);
  const [deleteLoading, setDeleteLoading] = useState(false);
  const [resetLoading, setResetLoading] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [branchFilter, setBranchFilter] = useState('all');
  const [roleFilter, setRoleFilter] = useState('all');
  const [statusFilter, setStatusFilter] = useState<StatusFilterValue>('all');
  const { isOpen: isDrawerOpen, onOpen: onDrawerOpen, onClose: onDrawerClose } = useDisclosure();
  const { register, control, handleSubmit, reset, setValue, watch, formState: { errors, isSubmitting } } = useForm<EmployeeFormValues>({ resolver: zodResolver(schema) as any, mode: 'onBlur', reValidateMode: 'onChange', defaultValues: emptyEmployeeForm });
  const watchedRole = watch('role');
  const watchedCompanyId = watch('companyId');
  const { isOpen: isDeleteOpen, onOpen: onDeleteOpen, onClose: onDeleteClose } = useDisclosure();
  const [deletingId, setDeletingId] = useState<string | undefined>(undefined);
  const { isOpen: isResetOpen, onOpen: onResetOpen, onClose: onResetClose } = useDisclosure();
  const [resettingId, setResettingId] = useState<string | undefined>(undefined);
  const branchOptions = useMemo(() => branches.map((branch) => ({ label: branch.name, value: branch.id })), [branches]);
  const roleOptions = useMemo(() => [
    { label: t('employees.employeeRole'), value: 'Employee' },
    ...(canManageOperatorRoles ? [{ label: t('employees.hrRole'), value: 'HR' }, { label: t('employees.companyAdminRole'), value: 'CompanyAdmin' }] : []),
  ], [canManageOperatorRoles, t]);

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
      const employeesParams = { 
        page, 
        pageSize, 
        keyword: searchQuery, 
        isActive: statusFilterToBoolean(statusFilter), 
        branchId: branchFilter === 'all' ? undefined : branchFilter,
        refresh
      };
      
      let nextBranches: Branch[] = [];
      if (!isSuperAdmin || (selectedCompanyId && selectedCompanyId !== 'all')) {
        const branchesResponse = await branchesService.getBranches({ page: 1, pageSize: 100, isActive: true, refresh });
        nextBranches = branchesResponse.data || [];
      }
      
      const employeesResponse = await employeesService.getEmployees(employeesParams);
      setRows(employeesResponse.data || []);
      setTotalCount(employeesResponse.totalCount ?? employeesResponse.data?.length ?? 0);
      setBranches(nextBranches);
    } catch {
      toast({ title: t('common.error'), description: t('employees.loadError'), status: 'error', duration: 3000, isClosable: true });
    } finally { setTableLoading(false); }
  }, [branchFilter, isSuperAdmin, page, pageSize, searchQuery, selectedCompanyId, statusFilter, t, toast]);
  useEffect(() => { void load(); }, [load]);
  const filteredRows = useMemo(() => roleFilter === 'all' ? rows : rows.filter((row) => row.roles.includes(roleFilter)), [roleFilter, rows]);

  const handleAddNew = () => { if (!canCreateEmployee) return; setEditingId(undefined); reset({ ...emptyEmployeeForm, companyId: isSuperAdmin ? selectedCompanyId : '', role: 'Employee' }); onDrawerOpen(); };
  const handleEdit = async (employee: Employee) => {
    if (!canEditEmployee) return;
    setEditingId(employee.id.toString());
    const nextCompanyId = employee.companyId || selectedCompanyId || '';
    
    if (isSuperAdmin && nextCompanyId && nextCompanyId !== 'all') {
      try {
        localStorage.setItem('active_company_id', nextCompanyId);
        const branchesResponse = await branchesService.getBranches({ page: 1, pageSize: 100, isActive: true });
        setBranches(branchesResponse.data || []);
      } catch (err) {
        console.error("Failed to load branches for editing", err);
      }
    }
    
    reset({
      firstName: employee.firstName,
      lastName: employee.lastName,
      email: employee.email,
      phoneNumber: employee.phoneNumber || '',
      companyId: nextCompanyId,
      branchId: employee.branchId || '',
      role: employee.roles[0] || 'Employee',
      password: '',
      isActive: employee.isActive
    });
    
    if (isSuperAdmin && nextCompanyId) {
      setSelectedCompanyId(nextCompanyId);
    }
    onDrawerOpen();
  };

  const onSubmit = handleSubmit(async (values) => {
    if (isSuperAdmin && values.companyId) { localStorage.setItem('active_company_id', values.companyId); setSelectedCompanyId(values.companyId); }
    const { companyId: _companyId, password, ...employeePayload } = values;
    const payload = { ...employeePayload, branchId: values.branchId || null, ...(password ? { password } : {}) };
    try {
      await employeesService.saveEmployee(payload, editingId);
      toast({ title: editingId ? t('employees.updated') : t('employees.saved'), status: 'success', duration: 2500, isClosable: true });
      onDrawerClose();
      await load();
    } catch (err) {
      toast({ title: t('employees.saveError'), description: translateServerMessage(err, t('common.error')), status: 'error', duration: 4500, isClosable: true });
    }
  });

  const handleDeletePrompt = (employee: Employee) => { if (!canDeleteEmployee) return; setDeletingId(employee.id.toString()); onDeleteOpen(); };
  const handleDeleteConfirm = async () => { if (!deletingId) return; setDeleteLoading(true); try { await employeesService.deleteEmployee(deletingId); toast({ title: t('employees.deleted'), status: 'success', duration: 2500, isClosable: true }); await load(); } catch { toast({ title: t('employees.deleteError'), status: 'error', duration: 3000, isClosable: true }); } finally { setDeleteLoading(false); setDeletingId(undefined); } };
  const handleResetPrompt = (employee: Employee) => { if (!canResetDevice) return; setResettingId(employee.id.toString()); onResetOpen(); };
  const handleResetConfirm = async () => { if (!resettingId) return; setResetLoading(true); try { const response = await employeesService.resetDevice(resettingId); toast({ title: response.success ? t('employees.resetSuccess') : t('employees.resetFailed'), description: response.message, status: response.success ? 'success' : 'error', duration: 3000, isClosable: true }); await load(); } catch { toast({ title: t('employees.resetError'), status: 'error', duration: 3000, isClosable: true }); } finally { setResetLoading(false); setResettingId(undefined); } };

  const isActionDisabled = useCallback((employee: Employee) => {
    if (employee.id === user?.id) return true;
    const isHr = user?.roles?.includes('HR');
    if (isHr && !employee.roles.includes('Employee')) return true;
    return false;
  }, [user]);

  const rowActions = [
    ...(canEditEmployee ? [{ label: t('common.edit'), onClick: handleEdit, isDisabled: isActionDisabled }] : []),
    ...(canResetDevice ? [{ label: t('employees.resetDevice'), onClick: handleResetPrompt, isDisabled: isActionDisabled }] : []),
    ...(canDeleteEmployee ? [{ label: t('common.delete'), onClick: handleDeletePrompt, isDestructive: true, isDisabled: isActionDisabled }] : []),
  ];

  return (
    <Flex direction="column" w="100%">
      <SharedPageHeader title={t('employees.pageTitle')} description={t('employees.pageDesc')} action={canCreateEmployee ? <Button leftIcon={<Icon as={MdAdd as React.ElementType} />} variant="brand" onClick={handleAddNew} w={{ base: '100%', md: 'auto' }}>{t('employees.addEmployee')}</Button> : undefined} />
      <SharedToolbar
        searchValue={searchQuery}
        onSearchChange={(value) => { setSearchQuery(value); setPage(1); }}
        searchPlaceholder={t('employees.searchPlaceholder')}
        statusValue={statusFilter}
        onStatusChange={(value) => { setStatusFilter(value as StatusFilterValue); setPage(1); }}
        statusOptions={defaultStatusOptions}
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
                  : t('employees.allBranches')}
              </option>
              {branches.map((branch) => (
                <option key={branch.id} value={branch.id}>
                  {branch.name}
                </option>
              ))}
            </Select>
            <Select
              value={roleFilter}
              onChange={(event) => setRoleFilter(event.target.value)}
              maxW={{ base: '100%', md: '180px' }}
              borderRadius="8px"
            >
              <option value="all">{t('employees.allRoles')}</option>
              <option value="Employee">{t('employees.employeeRole')}</option>
              <option value="HR">{t('employees.hr')}</option>
              <option value="CompanyAdmin">{t('employees.companyAdmin')}</option>
            </Select>
          </>
        }
      />
      <SharedDataTable columns={[{ key: 'firstName', header: t('employees.nameHeader'), isSortable: true, render: (row) => `${row.firstName} ${row.lastName}`, getSortValue: (row) => `${row.firstName} ${row.lastName}` }, { key: 'email', header: t('employees.email'), isSortable: true }, { key: 'branchName', header: t('employees.branchHeader'), render: (row) => row.branchName || t('employees.notAssigned') }, { key: 'roles', header: t('employees.roleHeader'), render: (row) => row.roles.join(', ') }, { key: 'hasBoundDevice', header: t('employees.deviceHeader'), render: (row) => row.hasBoundDevice ? <Badge colorScheme="brandScheme">{t('employees.bound')}</Badge> : <Badge>{t('employees.none')}</Badge>, getSortValue: (row) => row.hasBoundDevice }, { key: 'isActive', header: t('common.status'), render: (row) => <SharedStatusBadge isActive={row.isActive} />, getSortValue: (row) => row.isActive }]} data={filteredRows} isLoading={tableLoading} emptyTitle={t('employees.emptyTitle')} emptyDescription={t('employees.emptyDesc')} emptyActionLabel={canCreateEmployee ? t('employees.emptyAction') : undefined} onEmptyAction={canCreateEmployee ? handleAddNew : undefined} isFiltered={searchQuery !== '' || branchFilter !== 'all' || roleFilter !== 'all' || statusFilter !== 'all'} onClearFilters={() => { setSearchQuery(''); setBranchFilter('all'); setRoleFilter('all'); setStatusFilter('all'); setPage(1); }} actions={rowActions} page={page} pageSize={pageSize} totalCount={roleFilter === 'all' ? totalCount : filteredRows.length} onPageChange={setPage} onPageSizeChange={(nextPageSize) => { setPageSize(nextPageSize); setPage(1); }} />
      <SharedModal isOpen={isDrawerOpen} onClose={onDrawerClose} title={editingId ? t('employees.titleEdit') : t('employees.titleAdd')} description={t('employees.desc')} isLoading={isSubmitting} initialFocusRef={initialInputRef} footer={<><Button variant="ghost" onClick={onDrawerClose} isDisabled={isSubmitting}>{t('common.cancel')}</Button><Button type="submit" form="employee-form" variant="brand" isLoading={isSubmitting} loadingText={t('employees.saving')}>{t('employees.saveEmployee')}</Button></>}>
        <form id="employee-form" onSubmit={onSubmit} noValidate>
          <VStack spacing="16px" align="stretch">
            <SimpleGrid columns={{ base: 1, md: 2 }} gap="14px"><SharedInput label={t('employees.firstName')} isRequired error={errors.firstName?.message} placeholder={t('employees.firstNamePlaceholder')} {...register('firstName')} /><SharedInput label={t('employees.lastName')} isRequired error={errors.lastName?.message} placeholder={t('employees.lastNamePlaceholder')} {...register('lastName')} /></SimpleGrid>
            <SharedInput label={t('employees.email')} type="email" isRequired error={errors.email?.message} placeholder={t('employees.emailPlaceholder')} {...register('email')} />
            <SharedInput label={t('employees.phoneNumber')} error={errors.phoneNumber?.message} placeholder={t('employees.phonePlaceholder')} {...register('phoneNumber')} />
            {isSuperAdmin && <SharedSelect label={t('employees.assignCompany')} isRequired placeholderLabel={t('employees.selectCompanyFirst')} options={companies.map((company) => ({ label: company.name, value: company.id }))} error={errors.companyId?.message} {...register('companyId', { onChange: (event) => { const companyId = event.target.value; setValue('branchId', '', { shouldDirty: true, shouldValidate: true }); setSelectedCompanyId(companyId); if (companyId) localStorage.setItem('active_company_id', companyId); else localStorage.removeItem('active_company_id'); setPage(1); } })} />}
            <SharedSelect label={t('employees.assignBranch')} isRequired={watchedRole === 'Employee'} placeholderLabel={isSuperAdmin && !watchedCompanyId ? t('employees.selectCompanyFirst') : t('employees.selectBranch')} options={branchOptions} error={errors.branchId?.message} isDisabled={isSuperAdmin && !watchedCompanyId} {...register('branchId')} />
            <SharedSelect label={t('employees.assignRole')} isRequired error={errors.role?.message} options={roleOptions} {...register('role')} />
            {(!editingId || watchedRole === 'CompanyAdmin' || watchedRole === 'HR') && <SharedInput label={editingId ? t('employees.setNewPassword') : t('employees.accountPassword')} type="password" isRequired={!editingId} error={errors.password?.message} placeholder={t('common.securePassword')} {...register('password')} />}
            <Controller name="isActive" control={control} render={({ field }) => <SharedCheckbox label={t('employees.activeAccount')} isChecked={field.value} onChange={(event) => field.onChange(event.target.checked)} />} />
          </VStack>
        </form>
      </SharedModal>
      <SharedConfirmDialog isOpen={isDeleteOpen} onClose={onDeleteClose} onConfirm={handleDeleteConfirm} title={t('employees.deleteTitle')} message={t('employees.deleteMessage')} isDestructive loading={deleteLoading} />
      <SharedConfirmDialog isOpen={isResetOpen} onClose={onResetClose} onConfirm={handleResetConfirm} title={t('employees.resetTitle')} message={t('employees.resetMessage')} loading={resetLoading} />
    </Flex>
  );
}



