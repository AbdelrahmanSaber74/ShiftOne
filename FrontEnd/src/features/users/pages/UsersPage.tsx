import React, { useMemo, useState } from 'react';
import { Avatar, Badge, Box, Flex, useDisclosure, useToast } from '@chakra-ui/react';
import { useTranslation } from 'react-i18next';

import SharedDataTable from 'shared/components/ui/SharedDataTable';
import SharedPageHeader from 'shared/components/ui/SharedPageHeader';
import SharedToolbar from 'shared/components/ui/SharedToolbar';
import SharedConfirmDialog from 'shared/components/ui/SharedConfirmDialog';
import SharedStatusBadge from 'shared/components/ui/SharedStatusBadge';
import { booleanToStatusFilter, statusFilterToBoolean, StatusFilterValue } from 'shared/utils/filterUtils';

import type { UserProfile } from 'shared/types/api';
import { useUsers } from '../hooks/useUsers';
import { formatDate } from 'shared/utils/localeUtils';

export default function UsersPage() {
  const { t, i18n } = useTranslation();
  const toast = useToast();

  const {
    users,
    page,
    pageSize,
    totalCount,
    keyword,
    isActive,
    loading,
    setPage,
    setSearch,
    setStatusFilter,
    reload,
    approveUser,
    unapproveUser,
  } = useUsers();

  const { isOpen: isConfirmOpen, onOpen: onConfirmOpen, onClose: onConfirmClose } = useDisclosure();
  const [selectedUser, setSelectedUser] = useState<UserProfile | null>(null);
  const [actionType, setActionType] = useState<'approve' | 'unapprove' | null>(null);
  const [actionLoading, setActionLoading] = useState(false);

  const statusOptions = useMemo(
    () => [
      { label: t('users.allUsers'), value: 'all' },
      { label: t('users.activeStatus'), value: 'active' },
      { label: t('users.inactiveStatus'), value: 'inactive' },
    ],
    [t]
  );

  const handleActionPrompt = (user: UserProfile, type: 'approve' | 'unapprove') => {
    setSelectedUser(user);
    setActionType(type);
    onConfirmOpen();
  };

  const handleConfirmAction = async () => {
    if (!selectedUser || !actionType) return;
    setActionLoading(true);
    try {
      if (actionType === 'approve') {
        await approveUser(selectedUser.id);
      } else {
        await unapproveUser(selectedUser.id);
      }
      toast({ title: actionType === 'approve' ? t('users.toastApproved') : t('users.toastDeactivated'), status: 'success', duration: 2500, isClosable: true });
    } catch (err) {
      toast({ title: t('users.toastFailed'), description: err instanceof Error ? err.message : t('common.error'), status: 'error', duration: 3500, isClosable: true });
    } finally {
      setActionLoading(false);
      setSelectedUser(null);
      setActionType(null);
    }
  };


  return (
    <Flex direction="column" w="100%">
      <SharedPageHeader title={t('users.title')} description={t('users.description')} />

      <SharedToolbar
        searchValue={keyword}
        onSearchChange={setSearch}
        searchPlaceholder={t('users.searchPlaceholder')}
        statusValue={booleanToStatusFilter(isActive)}
        onStatusChange={(value) => setStatusFilter(statusFilterToBoolean(value as StatusFilterValue))}
        statusOptions={statusOptions}
        onRefresh={reload}
        isRefreshing={loading}
      />

      <SharedDataTable
        columns={[
          {
            key: 'user',
            header: t('users.userHeader'),
            isSortable: true,
            getSortValue: (user) => [user.firstName, user.lastName].filter(Boolean).join(' '),
            render: (user) => {
              const displayName = [user.firstName, user.lastName].filter(Boolean).join(' ') || t('users.unnamed');
              const dateStr = user.createdOn ? formatDate(user.createdOn, i18n.language) : null;
              return (
                <Flex align="center" gap="12px">
                  <Avatar size="sm" name={displayName} src={user.imagePath || undefined} />
                  <Box minW="0">
                    <Box fontWeight="700">{displayName}</Box>
                    <Box fontSize="xs" color="gray.500">{dateStr ? t('users.created', { date: dateStr }) : t('users.createdUnknown')}</Box>
                  </Box>
                </Flex>
              );
            },
          },
          {
            key: 'contact',
            header: t('users.contactHeader'),
            render: (user) => (
              <Box>
                <Box>{user.email || t('users.noEmail')}</Box>
                <Box fontSize="xs" color="gray.500">{user.phoneNumber || t('users.noPhone')}</Box>
              </Box>
            ),
          },
          {
            key: 'status',
            header: t('users.statusHeader'),
            render: (user) => (
              <Flex gap="6px" wrap="wrap">
                <SharedStatusBadge isActive={!!user.isActive} />
                {user.isProtected && <Badge colorScheme="purple">Protected</Badge>}
                <Badge colorScheme={user.emailConfirmed ? 'green' : 'yellow'}>{user.emailConfirmed ? t('users.emailVerified') : t('users.emailPending')}</Badge>
                <Badge colorScheme={user.phoneConfirmed ? 'green' : 'yellow'}>{user.phoneConfirmed ? t('users.phoneVerified') : t('users.phonePending')}</Badge>
              </Flex>
            ),
          },
          {
            key: 'roles',
            header: t('users.rolesHeader'),
            render: (user) => (
              <Flex gap="6px" wrap="wrap">
                {(user.roles || []).map((role) => <Badge key={role} colorScheme="brandScheme">{role}</Badge>)}
              </Flex>
            ),
          },
        ]}
        data={users}
        isLoading={loading}
        emptyTitle={t('users.emptyTitle')}
        emptyDescription={t('users.emptyDesc')}
        isFiltered={keyword !== '' || isActive !== null}
        onClearFilters={() => {
          setSearch('');
          setStatusFilter(null);
        }}
        actions={[
          { label: t('users.deactivate'), onClick: (user) => handleActionPrompt(user, 'unapprove'), isDestructive: true, isDisabled: (user) => !!user.isProtected },
          { label: t('users.approve'), onClick: (user) => handleActionPrompt(user, 'approve'), isDisabled: (user) => !!user.isProtected && !!user.isActive },
        ]}
        page={page}
        pageSize={pageSize}
        totalCount={totalCount}
        onPageChange={setPage}
      />

      {selectedUser && actionType && (
        <SharedConfirmDialog
          isOpen={isConfirmOpen}
          onClose={onConfirmClose}
          onConfirm={handleConfirmAction}
          title={actionType === 'approve' ? t('users.approve') : t('users.deactivate')}
          message={
            actionType === 'approve'
              ? t('users.approveConfirm', { name: `${selectedUser.firstName} ${selectedUser.lastName}`.trim() || t('users.unnamed') })
              : t('users.deactivateConfirm', { name: `${selectedUser.firstName} ${selectedUser.lastName}`.trim() || t('users.unnamed') })
          }
          isDestructive={actionType === 'unapprove'}
          loading={actionLoading}
        />
      )}
    </Flex>
  );
}


