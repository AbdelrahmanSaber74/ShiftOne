import React, { useMemo, useState } from 'react';
import {
  Box,
  Checkbox,
  Flex,
  Icon,
  IconButton,
  Menu,
  MenuButton,
  MenuItem,
  MenuList,
  Table,
  TableContainer,
  Tbody,
  Td,
  Text,
  Th,
  Thead,
  Tr,
  useColorModeValue,
} from '@chakra-ui/react';
import { MdArrowDownward, MdArrowUpward, MdMoreVert, MdUnfoldMore } from 'react-icons/md';
import { useTranslation } from 'react-i18next';
import SharedPagination from './SharedPagination';
import { SharedEmptyState, SharedLoading, SharedNoResults } from './SharedStates';

type SortDirection = 'asc' | 'desc';

export interface SharedDataTableColumn<T> {
  key: string;
  header: string;
  isNumeric?: boolean;
  isSortable?: boolean;
  minW?: string | number;
  render?: (item: T) => React.ReactNode;
  getSortValue?: (item: T) => string | number | boolean | Date | null | undefined;
}

export interface SharedDataTableAction<T> {
  label: string;
  icon?: React.ReactElement;
  onClick: (item: T) => void;
  isDestructive?: boolean;
  isDisabled?: boolean | ((item: T) => boolean);
}

interface SharedDataTableProps<T> {
  columns: SharedDataTableColumn<T>[];
  data: T[];
  getRowId?: (item: T, index: number) => string | number;
  isLoading?: boolean;
  emptyTitle?: string;
  emptyDescription?: string;
  emptyActionLabel?: string;
  onEmptyAction?: () => void;
  noResultsTitle?: string;
  noResultsDescription?: string;
  isFiltered?: boolean;
  onClearFilters?: () => void;
  actions?: SharedDataTableAction<T>[];
  selectedRowIds?: Array<string | number>;
  onSelectionChange?: (ids: Array<string | number>) => void;
  page?: number;
  pageSize?: number;
  totalCount?: number;
  onPageChange?: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
}

function defaultGetRowId<T extends { id?: string | number }>(item: T, index: number) {
  return item.id ?? index;
}

function normalizeSortValue(value: unknown) {
  if (value instanceof Date) return value.getTime();
  if (typeof value === 'boolean') return value ? 1 : 0;
  if (typeof value === 'number') return value;
  return String(value ?? '').toLowerCase();
}

export default function SharedDataTable<T extends { id?: string | number }>({
  columns,
  data,
  getRowId = defaultGetRowId,
  isLoading = false,
  emptyTitle,
  emptyDescription,
  emptyActionLabel,
  onEmptyAction,
  noResultsTitle,
  noResultsDescription,
  isFiltered = false,
  onClearFilters,
  actions = [],
  selectedRowIds,
  onSelectionChange,
  page,
  pageSize,
  totalCount,
  onPageChange,
  onPageSizeChange,
}: SharedDataTableProps<T>) {
  const { t } = useTranslation();
  const [sort, setSort] = useState<{ key: string; direction: SortDirection } | null>(null);
  const hasSelection = !!onSelectionChange;

  const tableBg = useColorModeValue('white', 'navy.800');
  const borderColor = useColorModeValue('gray.200', 'whiteAlpha.100');
  const textColor = useColorModeValue('secondaryGray.900', 'white');
  const mutedText = useColorModeValue('secondaryGray.600', 'secondaryGray.400');
  const rowHoverBg = useColorModeValue('gray.50', 'whiteAlpha.50');
  const menuBg = useColorModeValue('white', 'navy.800');
  const destructiveHoverBg = useColorModeValue('red.50', 'whiteAlpha.100');
  const shadowBg = useColorModeValue('0px 10px 30px rgba(112, 144, 176, 0.04)', 'none');
  const theadBg = useColorModeValue('gray.50/50', 'whiteAlpha.20');
  const headerTextColor = useColorModeValue('gray.500', 'gray.400');

  const selectedSet = useMemo(() => new Set(selectedRowIds ?? []), [selectedRowIds]);
  const rowIds = useMemo(() => data.map((item, index) => getRowId(item, index)), [data, getRowId]);
  const allVisibleSelected = rowIds.length > 0 && rowIds.every((id) => selectedSet.has(id));
  const someVisibleSelected = rowIds.some((id) => selectedSet.has(id)) && !allVisibleSelected;

  const sortedData = useMemo(() => {
    if (!sort) return data;
    const column = columns.find((item) => item.key === sort.key);
    if (!column) return data;

    return [...data].sort((first, second) => {
      const firstValue = normalizeSortValue(column.getSortValue ? column.getSortValue(first) : (first as Record<string, unknown>)[column.key]);
      const secondValue = normalizeSortValue(column.getSortValue ? column.getSortValue(second) : (second as Record<string, unknown>)[column.key]);
      if (firstValue > secondValue) return sort.direction === 'asc' ? 1 : -1;
      if (firstValue < secondValue) return sort.direction === 'asc' ? -1 : 1;
      return 0;
    });
  }, [columns, data, sort]);

  const toggleSort = (column: SharedDataTableColumn<T>) => {
    if (!column.isSortable) return;
    setSort((current) => {
      if (current?.key !== column.key) return { key: column.key, direction: 'asc' };
      if (current.direction === 'asc') return { key: column.key, direction: 'desc' };
      return null;
    });
  };

  const handleToggleAll = () => {
    if (!onSelectionChange) return;
    const next = new Set(selectedSet);
    if (allVisibleSelected) {
      rowIds.forEach((id) => next.delete(id));
    } else {
      rowIds.forEach((id) => next.add(id));
    }
    onSelectionChange(Array.from(next));
  };

  const handleToggleRow = (rowId: string | number) => {
    if (!onSelectionChange) return;
    const next = new Set(selectedSet);
    if (next.has(rowId)) {
      next.delete(rowId);
    } else {
      next.add(rowId);
    }
    onSelectionChange(Array.from(next));
  };

  if (isLoading) {
    return (
      <Box bg={tableBg} border="1px solid" borderColor={borderColor} borderRadius="8px" p="16px">
        <SharedLoading rows={8} height="42px" />
      </Box>
    );
  }

  if (data.length === 0) {
    return isFiltered ? (
      <SharedNoResults
        title={noResultsTitle}
        description={noResultsDescription}
        onClear={onClearFilters}
      />
    ) : (
      <SharedEmptyState
        title={emptyTitle ?? t('common.noRecords')}
        description={emptyDescription ?? t('common.noRecordsDescription')}
        actionLabel={emptyActionLabel}
        onAction={onEmptyAction}
      />
    );
  }

  return (
    <Box 
      bg={tableBg} 
      border="1px solid" 
      borderColor={borderColor} 
      borderRadius="8px" 
      overflow="hidden"
      boxShadow={shadowBg}
    >
      <TableContainer maxW="100%" overflowX="auto">
        <Table size="md" variant="simple">
          <Thead bg={theadBg} position="sticky" top="0" zIndex="1">
            <Tr>
              {hasSelection && (
                <Th w="48px" borderColor={borderColor} py="16px">
                  <Checkbox
                    colorScheme="brandScheme"
                    isChecked={allVisibleSelected}
                    isIndeterminate={someVisibleSelected}
                    onChange={handleToggleAll}
                    aria-label={t('common.selectVisibleRows')}
                  />
                </Th>
              )}
              {columns.map((column) => {
                const activeSort = sort?.key === column.key ? sort.direction : null;
                return (
                  <Th
                    key={column.key}
                    borderColor={borderColor}
                    color={headerTextColor}
                    fontSize="11px"
                    fontWeight="700"
                    letterSpacing="0.8px"
                    textTransform="uppercase"
                    py="16px"
                    minW={column.minW}
                    isNumeric={column.isNumeric}
                    cursor={column.isSortable ? 'pointer' : 'default'}
                    onClick={() => toggleSort(column)}
                  >
                    <Flex align="center" justify={column.isNumeric ? 'flex-end' : 'flex-start'} gap="6px">
                      <Text as="span">{column.header}</Text>
                      {column.isSortable && (
                        <Icon
                          as={
                            (activeSort === 'asc'
                              ? MdArrowUpward
                              : activeSort === 'desc'
                              ? MdArrowDownward
                              : MdUnfoldMore) as React.ElementType
                          }
                          boxSize="14px"
                          color={activeSort ? 'brand.500' : mutedText}
                        />
                      )}
                    </Flex>
                  </Th>
                );
              })}
              {actions.length > 0 && (
                <Th borderColor={borderColor} w="72px" textAlign="center" py="16px" color={headerTextColor} fontSize="11px" fontWeight="700" letterSpacing="0.8px" textTransform="uppercase">
                  {t('common.actions')}
                </Th>
              )}
            </Tr>
          </Thead>
          <Tbody>
            {sortedData.map((item, rowIndex) => {
              const rowId = getRowId(item, rowIndex);
              return (
                <Tr 
                  key={rowId} 
                  _hover={{ bg: rowHoverBg }}
                  transition="background-color 0.15s ease"
                >
                  {hasSelection && (
                    <Td borderColor={borderColor} py="15px">
                      <Checkbox
                        colorScheme="brandScheme"
                        isChecked={selectedSet.has(rowId)}
                        onChange={() => handleToggleRow(rowId)}
                        aria-label={t('common.selectRow', { index: rowIndex + 1 })}
                      />
                    </Td>
                  )}
                  {columns.map((column) => (
                    <Td
                      key={column.key}
                      borderColor={borderColor}
                      color={textColor}
                      fontSize="sm"
                      fontWeight="500"
                      py="15px"
                      isNumeric={column.isNumeric}
                    >
                      {column.render ? column.render(item) : String((item as Record<string, unknown>)[column.key] ?? '')}
                    </Td>
                  ))}
                  {actions.length > 0 && (
                    <Td borderColor={borderColor} textAlign="center">
                      <Menu isLazy placement="bottom-end">
                        <MenuButton
                          as={IconButton}
                          aria-label={t('common.rowActions')}
                          icon={<Icon as={MdMoreVert as React.ElementType} />}
                          size="sm"
                          variant="ghost"
                          borderRadius="8px"
                        />
                        <MenuList bg={menuBg} borderColor={borderColor} p="4px" minW="180px">
                          {actions.map((action) => (
                            <MenuItem
                              key={action.label}
                              icon={action.icon}
                              onClick={() => action.onClick(item)}
                              isDisabled={typeof action.isDisabled === 'function' ? action.isDisabled(item) : action.isDisabled}
                              color={action.isDestructive ? 'red.500' : textColor}
                              borderRadius="6px"
                              fontSize="sm"
                              fontWeight="500"
                              _hover={{ bg: action.isDestructive ? destructiveHoverBg : rowHoverBg }}
                            >
                              {action.label}
                            </MenuItem>
                          ))}
                        </MenuList>
                      </Menu>
                    </Td>
                  )}
                </Tr>
              );
            })}
          </Tbody>
        </Table>
      </TableContainer>
      {page !== undefined && pageSize !== undefined && totalCount !== undefined && onPageChange && (
        <SharedPagination
          page={page}
          pageSize={pageSize}
          totalCount={totalCount}
          onPageChange={onPageChange}
          onPageSizeChange={onPageSizeChange}
          isDisabled={isLoading}
        />
      )}
    </Box>
  );
}
