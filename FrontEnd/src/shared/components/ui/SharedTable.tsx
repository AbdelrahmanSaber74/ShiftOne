import React from 'react';
import SharedDataTable, {
  SharedDataTableAction as RowActionDefinition,
  SharedDataTableColumn as ColumnDefinition,
} from './SharedDataTable';

export type { ColumnDefinition, RowActionDefinition };

type SharedTableProps<T extends { id?: string | number }> = React.ComponentProps<typeof SharedDataTable<T>> & {
  currentPage?: number;
  totalPages?: number;
};

export default function SharedTable<T extends { id?: string | number }>({
  currentPage,
  totalPages,
  page,
  pageSize,
  totalCount,
  ...props
}: SharedTableProps<T>) {
  const resolvedPage = page ?? currentPage;
  const resolvedPageSize = pageSize ?? 10;
  const resolvedTotalCount = totalCount ?? (totalPages && resolvedPageSize ? totalPages * resolvedPageSize : undefined);

  return (
    <SharedDataTable
      {...props}
      page={resolvedPage}
      pageSize={resolvedPage !== undefined ? resolvedPageSize : undefined}
      totalCount={resolvedPage !== undefined ? resolvedTotalCount : undefined}
    />
  );
}

