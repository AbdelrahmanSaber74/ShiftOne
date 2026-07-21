import React from 'react';
import {
  Flex,
  FlexProps,
  Icon,
  IconButton,
  Input,
  Select,
  Tooltip,
  useColorModeValue,
} from '@chakra-ui/react';
import { useTranslation } from 'react-i18next';
import { MdFileDownload, MdViewColumn } from 'react-icons/md';

export interface SharedToolbarFilterOption {
  label: string;
  value: string;
}

interface SharedToolbarProps extends FlexProps {
  children?: React.ReactNode;
  searchValue?: string;
  onSearchChange?: (value: string) => void;
  searchPlaceholder?: string;
  statusValue?: string;
  onStatusChange?: (value: string) => void;
  statusOptions?: SharedToolbarFilterOption[];
  onRefresh?: () => void;
  isRefreshing?: boolean;
  selectedCount?: number;
  bulkActions?: React.ReactNode;
  extraFilters?: React.ReactNode;
  onExport?: () => void;
  onColumnVisibility?: () => void;
}

export default function SharedToolbar({
  children,
  searchValue,
  onSearchChange,
  searchPlaceholder,
  statusValue,
  onStatusChange,
  statusOptions,
  selectedCount = 0,
  bulkActions,
  extraFilters,
  onExport,
  onColumnVisibility,
  ...props
}: SharedToolbarProps) {
  const { t } = useTranslation();
  const bg = useColorModeValue('white', 'navy.800');
  const controlBg = useColorModeValue('white', 'navy.900');
  const borderColor = useColorModeValue('gray.200', 'whiteAlpha.100');
  const iconColor = useColorModeValue('secondaryGray.700', 'secondaryGray.200');

  const wrapperProps = {
    direction: { base: 'column', lg: 'row' } as const,
    gap: '12px',
    p: '14px',
    bg,
    border: '1px solid',
    borderColor,
    borderRadius: '8px',
    align: { base: 'stretch', lg: 'center' } as const,
    w: '100%',
    mb: '16px',
    ...props,
  };

  if (children) {
    return <Flex {...wrapperProps}>{children}</Flex>;
  }

  return (
    <Flex {...wrapperProps}>
      <Flex flex="1" gap="10px" direction={{ base: 'column', md: 'row' }}>
        {onSearchChange && (
          <Input
            value={searchValue ?? ''}
            onChange={(event) => onSearchChange(event.target.value)}
            placeholder={searchPlaceholder ?? t('common.search')}
            bg={controlBg}
            borderRadius="8px"
            maxW={{ base: '100%', md: '320px' }}
          />
        )}
        {onStatusChange && statusOptions && (
          <Select
            value={statusValue}
            onChange={(event) => onStatusChange(event.target.value)}
            bg={controlBg}
            borderRadius="8px"
            maxW={{ base: '100%', md: '180px' }}
          >
            {statusOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label.includes('.') ? t(option.label) : option.label}
              </option>
            ))}
          </Select>
        )}
        {extraFilters}
      </Flex>

      <Flex gap="8px" align="center" justify={{ base: 'space-between', md: 'flex-end' }} wrap="wrap">
        {selectedCount > 0 && bulkActions}

        <Tooltip label={onExport ? t('common.export') : t('common.exportFuture')}>
          <IconButton
            aria-label={t('common.export')}
            icon={<Icon as={MdFileDownload as React.ElementType} color={iconColor} />}
            variant="outline"
            borderRadius="8px"
            onClick={onExport}
            isDisabled={!onExport}
          />
        </Tooltip>
        <Tooltip label={onColumnVisibility ? t('common.columns') : t('common.columnsFuture')}>
          <IconButton
            aria-label={t('common.columnVisibility')}
            icon={<Icon as={MdViewColumn as React.ElementType} color={iconColor} />}
            variant="outline"
            borderRadius="8px"
            onClick={onColumnVisibility}
            isDisabled={!onColumnVisibility}
          />
        </Tooltip>
      </Flex>
    </Flex>
  );
}
