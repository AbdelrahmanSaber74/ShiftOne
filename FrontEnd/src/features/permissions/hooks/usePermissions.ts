import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import permissionsService from '../services/permissionsService';
import { queryKeys } from 'shared/query/queryKeys';

export function usePermissions() {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [keyword, setKeyword] = useState('');

  const filters = useMemo(() => ({ page, pageSize, keyword }), [page, pageSize, keyword]);
  const permissionsQuery = useQuery({
    queryKey: queryKeys.permissions.list(filters),
    queryFn: () => permissionsService.getPermissions(filters),
  });

  const response = permissionsQuery.data;

  return {
    rows: response?.data || [],
    page,
    pageSize,
    totalCount: response?.totalCount ?? response?.data?.length ?? 0,
    keyword,
    loading: permissionsQuery.isLoading || permissionsQuery.isFetching,
    setPage,
    setPageSize,
    setKeyword,
    reload: permissionsQuery.refetch,
  };
}
