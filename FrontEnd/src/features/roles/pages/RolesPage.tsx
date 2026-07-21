import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { Badge, Box, Button, Checkbox, Flex, Icon, SimpleGrid, Text, useDisclosure, useToast, VStack } from '@chakra-ui/react';
import { MdAdd } from 'react-icons/md';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

import SharedDataTable from 'shared/components/ui/SharedDataTable';
import SharedPageHeader from 'shared/components/ui/SharedPageHeader';
import SharedToolbar from 'shared/components/ui/SharedToolbar';
import SharedModal from 'shared/components/ui/SharedModal';
import SharedConfirmDialog from 'shared/components/ui/SharedConfirmDialog';
import SharedStatusBadge from 'shared/components/ui/SharedStatusBadge';
import { SharedCheckbox, SharedInput, SharedTextarea } from 'shared/components/ui/SharedFormElements';
import { booleanToStatusFilter, statusFilterToBoolean, StatusFilterValue } from 'shared/utils/filterUtils';
import { useTranslation } from 'react-i18next';
import { useQueryClient } from '@tanstack/react-query';
import { translateServerMessage } from 'shared/utils/errorUtils';
import { invalidateMutationResources } from 'shared/query/queryInvalidation';
import type { Permission, Role } from 'shared/types/api';

import { useRoles } from '../hooks/useRoles';
import rolesService from '../services/rolesService';
import permissionsService from 'features/permissions/services/permissionsService';
import { roleStatusOptions } from '../constants/roleConstants';
import { RoleFormValues, roleSchema } from '../validations/roleValidation';

const emptyForm: RoleFormValues = { name: '', description: '', isActive: true, permissionIds: [] };

export default function RolesPage() {
  const toast = useToast();
  const queryClient = useQueryClient();
  const { t } = useTranslation();
  const initialInputRef = useRef<HTMLInputElement>(null);
  const schema = useMemo(() => roleSchema(t), [t]);
  const { rows, page, pageSize, totalCount, keyword, isActive, loading, setPage, setPageSize, setKeyword, setIsActive, reload } = useRoles();
  const [permissions, setPermissions] = useState<Permission[]>([]);
  const [editingRole, setEditingRole] = useState<Role | null>(null);
  const [deleting, setDeleting] = useState(false);
  const [deletingRole, setDeletingRole] = useState<Role | null>(null);
  const { isOpen: isDrawerOpen, onOpen: onDrawerOpen, onClose: onDrawerClose } = useDisclosure();
  const { isOpen: isDeleteOpen, onOpen: onDeleteOpen, onClose: onDeleteClose } = useDisclosure();
  const { register, control, handleSubmit, reset, watch, setValue, formState: { errors, isSubmitting } } = useForm<RoleFormValues>({ resolver: zodResolver(schema) as any, mode: 'onBlur', reValidateMode: 'onChange', defaultValues: emptyForm });
  const selectedPermissionIds = watch('permissionIds');

  const loadPermissions = useCallback(async () => {
    const response = await permissionsService.getPermissions({ page: 1, pageSize: 500 });
    setPermissions(response.data || []);
  }, []);
  useEffect(() => { void loadPermissions(); }, [loadPermissions]);
  const permissionIdByName = useMemo(() => new Map(permissions.map((permission) => [permission.name, permission.id])), [permissions]);

  const handleAdd = () => { setEditingRole(null); reset(emptyForm); onDrawerOpen(); };
  const handleEdit = (role: Role) => {
    setEditingRole(role);
    reset({ name: role.name, description: role.description || '', isActive: role.isActive, permissionIds: role.permissions.map((permissionName) => permissionIdByName.get(permissionName)).filter(Boolean) as string[] });
    onDrawerOpen();
  };

  const onSubmit = handleSubmit(async (values) => {
    try {
      const response = await rolesService.saveRole({ name: values.name, description: values.description || '', isActive: values.isActive }, editingRole?.id);
      const savedRoleId = response.data?.id || editingRole?.id;
      if (savedRoleId) await rolesService.assignPermissions(savedRoleId, values.permissionIds);
      toast({ title: editingRole ? t('roles.updated') : t('roles.created'), status: 'success', duration: 2500, isClosable: true });
      onDrawerClose();
      await invalidateMutationResources(queryClient, 'roles');
      await reload();
    } catch (error) {
      toast({ title: t('roles.saveError'), description: translateServerMessage(error, t('common.error')), status: 'error', duration: 4500, isClosable: true });
    }
  });

  const handleDeletePrompt = (role: Role) => { setDeletingRole(role); onDeleteOpen(); };
  const handleDeleteConfirm = async () => {
    if (!deletingRole) return;
    setDeleting(true);
    try { await rolesService.deleteRole(deletingRole.id); toast({ title: t('roles.deactivated'), status: 'success', duration: 2500, isClosable: true }); await invalidateMutationResources(queryClient, 'roles'); await reload(); }
    catch (error) { toast({ title: t('roles.deactivateError'), description: translateServerMessage(error, t('common.error')), status: 'error', duration: 4500, isClosable: true }); }
    finally { setDeleting(false); setDeletingRole(null); }
  };

  const togglePermission = (permissionId: string) => {
    setValue('permissionIds', selectedPermissionIds.includes(permissionId) ? selectedPermissionIds.filter((id) => id !== permissionId) : [...selectedPermissionIds, permissionId], { shouldDirty: true, shouldValidate: true });
  };

  return (
    <Flex direction="column" w="100%">
      <SharedPageHeader title={t('roles.pageTitle')} description={t('roles.pageDesc')} action={<Button leftIcon={<Icon as={MdAdd as React.ElementType} />} variant="brand" onClick={handleAdd} w={{ base: '100%', md: 'auto' }}>{t('roles.addRole')}</Button>} />
      <SharedToolbar searchValue={keyword} onSearchChange={(value) => { setKeyword(value); setPage(1); }} searchPlaceholder={t('roles.searchPlaceholder')} statusValue={booleanToStatusFilter(isActive)} onStatusChange={(value) => { setIsActive(statusFilterToBoolean(value as StatusFilterValue)); setPage(1); }} statusOptions={roleStatusOptions} onRefresh={reload} isRefreshing={loading} />
      <SharedDataTable columns={[{ key: 'name', header: t('roles.roleHeader'), isSortable: true, render: (role) => <Flex align="center" gap="8px"><Text fontWeight="700">{role.name}</Text>{role.isProtected && <Badge colorScheme="purple">{t('roles.protected')}</Badge>}{role.isSystemRole && <Badge>{t('roles.system')}</Badge>}</Flex> }, { key: 'description', header: t('roles.descriptionHeader'), minW: '220px' }, { key: 'permissions', header: t('roles.permissionsHeader'), render: (role) => role.permissions.length }, { key: 'isActive', header: t('common.status'), render: (role) => <SharedStatusBadge isActive={role.isActive} />, getSortValue: (role) => role.isActive }]} data={rows} isLoading={loading} emptyTitle={t('roles.emptyTitle')} emptyDescription={t('roles.emptyDesc')} emptyActionLabel={t('roles.emptyAction')} onEmptyAction={handleAdd} isFiltered={keyword !== '' || isActive !== null} onClearFilters={() => { setKeyword(''); setIsActive(null); setPage(1); }} actions={[{ label: t('common.edit'), onClick: handleEdit }, { label: t('common.deactivate'), onClick: handleDeletePrompt, isDestructive: true, isDisabled: (role) => role.isSystemRole }]} page={page} pageSize={pageSize} totalCount={totalCount} onPageChange={setPage} onPageSizeChange={(next) => { setPageSize(next); setPage(1); }} />
      <SharedModal isOpen={isDrawerOpen} onClose={onDrawerClose} title={editingRole ? t('roles.titleEdit') : t('roles.titleAdd')} description={t('roles.desc')} isLoading={isSubmitting} initialFocusRef={initialInputRef} footer={<><Button variant="ghost" onClick={onDrawerClose} isDisabled={isSubmitting}>{t('common.cancel')}</Button><Button type="submit" form="role-form" variant="brand" isLoading={isSubmitting}>{t('roles.saveRole')}</Button></>}>
        <form id="role-form" onSubmit={onSubmit} noValidate>
          <VStack spacing="16px" align="stretch">
            <SharedInput label={t('roles.roleName')} isRequired isDisabled={!!editingRole?.isProtected} error={errors.name?.message} placeholder={t('roles.namePlaceholder')} {...register('name')} />
            <SharedTextarea label={t('roles.description')} error={errors.description?.message} placeholder={t('roles.descriptionPlaceholder')} {...register('description')} />
            <Controller name="isActive" control={control} render={({ field }) => <SharedCheckbox label={t('roles.activeRole')} isChecked={field.value} isDisabled={!!editingRole?.isProtected} onChange={(event) => field.onChange(event.target.checked)} />} />
            <Box mt="8px"><Text fontSize="sm" fontWeight="800" mb="10px">{t('roles.permissions')}</Text><SimpleGrid columns={{ base: 1, md: 2 }} gap="8px" maxH="320px" overflowY="auto" border="1px solid" borderColor="gray.200" borderRadius="8px" p="12px">{permissions.map((permission) => <Checkbox key={permission.id} colorScheme="brandScheme" isChecked={selectedPermissionIds.includes(permission.id)} isDisabled={!!editingRole?.isProtected} onChange={() => togglePermission(permission.id)}><Box><Text fontSize="sm" fontWeight="700">{permission.name}</Text><Text fontSize="xs" color="gray.500">{permission.description || t('roles.noDescription')}</Text></Box></Checkbox>)}</SimpleGrid></Box>
          </VStack>
        </form>
      </SharedModal>
      <SharedConfirmDialog isOpen={isDeleteOpen} onClose={onDeleteClose} onConfirm={handleDeleteConfirm} title={t('roles.deleteTitle')} message={t('roles.deleteMessage')} isDestructive loading={deleting} />
    </Flex>
  );
}

