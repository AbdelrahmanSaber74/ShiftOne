import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import rolesService from '../services/rolesService';
import { queryKeys } from 'shared/query/queryKeys';

export function useRoles() {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');
  const [isActive, setIsActive] = useState<boolean | null>(null);

  const filters = useMemo(() => ({ page, pageSize, keyword, isActive }), [page, pageSize, keyword, isActive]);
  const rolesQuery = useQuery({
    queryKey: queryKeys.roles.list(filters),
    queryFn: () => rolesService.getRoles(filters),
  });

  const response = rolesQuery.data;

  return {
    rows: response?.data || [],
    page,
    pageSize,
    totalCount: response?.totalCount ?? response?.data?.length ?? 0,
    keyword,
    isActive,
    loading: rolesQuery.isLoading || rolesQuery.isFetching,
    setPage,
    setPageSize,
    setKeyword,
    setIsActive,
    reload: rolesQuery.refetch,
  };
}
