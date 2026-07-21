import React, { useMemo, useRef, useState } from 'react';
import { Badge, Box, Button, Flex, Icon, Text, useDisclosure, useToast, VStack } from '@chakra-ui/react';
import { MdAdd } from 'react-icons/md';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

import SharedConfirmDialog from 'shared/components/ui/SharedConfirmDialog';
import SharedDataTable from 'shared/components/ui/SharedDataTable';
import SharedModal from 'shared/components/ui/SharedModal';
import { SharedInput, SharedTextarea } from 'shared/components/ui/SharedFormElements';
import SharedPageHeader from 'shared/components/ui/SharedPageHeader';
import SharedToolbar from 'shared/components/ui/SharedToolbar';
import type { Permission } from 'shared/types/api';
import { useTranslation } from 'react-i18next';
import { useQueryClient } from '@tanstack/react-query';
import { translateServerMessage } from 'shared/utils/errorUtils';
import { invalidateMutationResources } from 'shared/query/queryInvalidation';

import { permissionKindOptions } from '../constants/permissionConstants';
import { usePermissions } from '../hooks/usePermissions';
import permissionsService from '../services/permissionsService';
import { PermissionFormValues, permissionSchema } from '../validations/permissionValidation';

type PermissionKindFilter = 'all' | 'system' | 'custom';
const emptyForm: PermissionFormValues = { name: '', description: '' };

export default function PermissionsPage() {
  const toast = useToast();
  const queryClient = useQueryClient();
  const { t, i18n } = useTranslation();
  const schema = useMemo(() => permissionSchema(t), [t]);
  const initialInputRef = useRef<HTMLInputElement>(null);
  const { rows, page, pageSize, totalCount, keyword, loading, setPage, setPageSize, setKeyword, reload } = usePermissions();
  const [kindFilter, setKindFilter] = useState<PermissionKindFilter>('all');
  const [editingPermission, setEditingPermission] = useState<Permission | null>(null);
  const [deletingPermission, setDeletingPermission] = useState<Permission | null>(null);
  const [deleting, setDeleting] = useState(false);
  const { isOpen: isDrawerOpen, onOpen: onDrawerOpen, onClose: onDrawerClose } = useDisclosure();
  const { isOpen: isDeleteOpen, onOpen: onDeleteOpen, onClose: onDeleteClose } = useDisclosure();
  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<PermissionFormValues>({ resolver: zodResolver(schema) as any, mode: 'onBlur', reValidateMode: 'onChange', defaultValues: emptyForm });

  const filteredRows = useMemo(() => {
    if (kindFilter === 'system') return rows.filter((permission) => permission.isSystemPermission);
    if (kindFilter === 'custom') return rows.filter((permission) => !permission.isSystemPermission);
    return rows;
  }, [kindFilter, rows]);

  const handleAdd = () => { setEditingPermission(null); reset(emptyForm); onDrawerOpen(); };
  const handleEdit = (permission: Permission) => { setEditingPermission(permission); reset({ name: permission.name, description: permission.description || '' }); onDrawerOpen(); };
  const onSubmit = handleSubmit(async (values) => {
    try {
      await permissionsService.savePermission(values, editingPermission?.id);
      toast({ title: editingPermission ? t('permissions.updated') : t('permissions.created'), status: 'success', duration: 2500, isClosable: true });
      onDrawerClose();
      await invalidateMutationResources(queryClient, 'permissions');
      await reload();
    } catch (error) {
      toast({ title: t('permissions.saveError'), description: translateServerMessage(error, t('common.error')), status: 'error', duration: 4500, isClosable: true });
    }
  });
  const handleDeletePrompt = (permission: Permission) => { setDeletingPermission(permission); onDeleteOpen(); };
  const handleDeleteConfirm = async () => {
    if (!deletingPermission) return;
    setDeleting(true);
    try { await permissionsService.deletePermission(deletingPermission.id); toast({ title: t('permissions.deleted'), status: 'success', duration: 2500, isClosable: true }); await invalidateMutationResources(queryClient, 'permissions'); await reload(); }
    catch (error) { toast({ title: t('permissions.deleteError'), description: translateServerMessage(error, t('common.error')), status: 'error', duration: 4500, isClosable: true }); }
    finally { setDeleting(false); setDeletingPermission(null); }
  };

  return (
    <Flex direction="column" w="100%">
      <SharedPageHeader title={t('permissions.pageTitle')} description={t('permissions.pageDesc')} action={<Button leftIcon={<Icon as={MdAdd as React.ElementType} />} variant="brand" onClick={handleAdd} w={{ base: '100%', md: 'auto' }}>{t('permissions.addPermission')}</Button>} />
      <SharedToolbar searchValue={keyword} onSearchChange={(value) => { setKeyword(value); setPage(1); }} searchPlaceholder={t('permissions.searchPlaceholder')} statusValue={kindFilter} onStatusChange={(value) => { setKindFilter(value as PermissionKindFilter); setPage(1); }} statusOptions={permissionKindOptions} onRefresh={reload} isRefreshing={loading} />
      <SharedDataTable columns={[{ key: 'name', header: t('permissions.permissionHeader'), isSortable: true, render: (permission) => <Flex align="center" gap="8px"><Text fontWeight="700">{permission.name}</Text>{permission.isSystemPermission && <Badge colorScheme="purple">{t('permissions.system')}</Badge>}</Flex> }, { key: 'description', header: t('permissions.descriptionHeader'), minW: '260px', render: (permission) => permission.description || t('roles.noDescription') }, { key: 'createdOn', header: t('permissions.createdHeader'), isSortable: true, render: (permission) => permission.createdOn ? new Date(permission.createdOn).toLocaleDateString(i18n.language?.startsWith('ar') ? 'ar-EG' : 'en-US') : t('common.unknown') }]} data={filteredRows} isLoading={loading} emptyTitle={t('permissions.emptyTitle')} emptyDescription={t('permissions.emptyDesc')} emptyActionLabel={t('permissions.emptyAction')} onEmptyAction={handleAdd} isFiltered={keyword !== '' || kindFilter !== 'all'} onClearFilters={() => { setKeyword(''); setKindFilter('all'); setPage(1); }} actions={[{ label: t('common.edit'), onClick: handleEdit }, { label: t('common.delete'), onClick: handleDeletePrompt, isDestructive: true, isDisabled: (permission) => permission.isSystemPermission }]} page={page} pageSize={pageSize} totalCount={kindFilter === 'all' ? totalCount : filteredRows.length} onPageChange={setPage} onPageSizeChange={(next) => { setPageSize(next); setPage(1); }} />
      <SharedModal isOpen={isDrawerOpen} onClose={onDrawerClose} title={editingPermission ? t('permissions.titleEdit') : t('permissions.titleAdd')} description={t('permissions.desc')} isLoading={isSubmitting} initialFocusRef={initialInputRef} footer={<><Button variant="ghost" onClick={onDrawerClose} isDisabled={isSubmitting}>{t('common.cancel')}</Button><Button type="submit" form="permission-form" variant="brand" isLoading={isSubmitting}>{t('permissions.savePermission')}</Button></>}>
        <form id="permission-form" onSubmit={onSubmit} noValidate>
          <VStack spacing="16px" align="stretch">
            <SharedInput label={t('permissions.permissionName')} isRequired isDisabled={!!editingPermission?.isSystemPermission} error={errors.name?.message} placeholder={t('permissions.namePlaceholder')} {...register('name')} />
            <SharedTextarea label={t('permissions.description')} error={errors.description?.message} placeholder={t('permissions.descriptionPlaceholder')} {...register('description')} />
            {editingPermission?.isSystemPermission && <Box color="gray.500" fontSize="sm">{t('permissions.systemProtected')}</Box>}
          </VStack>
        </form>
      </SharedModal>
      <SharedConfirmDialog isOpen={isDeleteOpen} onClose={onDeleteClose} onConfirm={handleDeleteConfirm} title={t('permissions.deleteTitle')} message={t('permissions.deleteMessage')} isDestructive loading={deleting} />
    </Flex>
  );
}

