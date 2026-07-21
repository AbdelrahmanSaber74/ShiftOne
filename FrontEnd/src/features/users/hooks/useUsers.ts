import { useCallback, useMemo, useState } from 'react';
import { AxiosError } from 'axios';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import usersService from 'features/users/services/usersService';
import type { ApiResponse } from 'shared/types/api';
import { queryKeys } from 'shared/query/queryKeys';
import { invalidateMutationResources } from 'shared/query/queryInvalidation';

interface UseUsersFilters {
  page?: number;
  pageSize?: number;
  keyword?: string;
  isActive?: boolean | null;
}

function getErrorMessage(error: unknown): string {
  const axiosError = error as AxiosError<ApiResponse>;
  return axiosError.response?.data?.message || (error instanceof Error ? error.message : 'Unable to load users.');
}

export function useUsers(initialFilters: UseUsersFilters = {}) {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(initialFilters.page ?? 1);
  const [pageSize] = useState(initialFilters.pageSize ?? 10);
  const [keyword, setKeyword] = useState(initialFilters.keyword ?? '');
  const [isActive, setIsActive] = useState<boolean | null>(initialFilters.isActive ?? null);

  const filters = useMemo(() => ({ page, pageSize, keyword, isActive }), [page, pageSize, keyword, isActive]);

  const usersQuery = useQuery({
    queryKey: queryKeys.users.list(filters),
    queryFn: async () => {
      const response = await usersService.getUsers(filters);
      if (!response.success) {
        throw new Error(response.message || 'Unable to load users.');
      }
      return response;
    },
  });

  const invalidateUsers = useCallback(async () => {
    await invalidateMutationResources(queryClient, 'users');
  }, [queryClient]);

  const approveMutation = useMutation({
    mutationFn: usersService.approveUser,
    onSuccess: invalidateUsers,
  });

  const unapproveMutation = useMutation({
    mutationFn: usersService.unapproveUser,
    onSuccess: invalidateUsers,
  });

  const setSearch = useCallback((value: string) => {
    setKeyword(value);
    setPage(1);
  }, []);

  const setStatusFilter = useCallback((value: boolean | null) => {
    setIsActive(value);
    setPage(1);
  }, []);

  const approveUser = useCallback(async (userId: string) => {
    await approveMutation.mutateAsync(userId);
  }, [approveMutation]);

  const unapproveUser = useCallback(async (userId: string) => {
    await unapproveMutation.mutateAsync(userId);
  }, [unapproveMutation]);

  const response = usersQuery.data;
  const users = response?.data?.users ?? [];
  const totalCount = response?.totalCount ?? response?.data?.users?.length ?? 0;
  const loading = usersQuery.isLoading || usersQuery.isFetching || approveMutation.isPending || unapproveMutation.isPending;

  return {
    users,
    page,
    pageSize,
    totalCount,
    keyword,
    isActive,
    loading,
    error: usersQuery.error ? getErrorMessage(usersQuery.error) : '',
    setPage,
    setSearch,
    setStatusFilter,
    reload: usersQuery.refetch,
    approveUser,
    unapproveUser,
  };
}
