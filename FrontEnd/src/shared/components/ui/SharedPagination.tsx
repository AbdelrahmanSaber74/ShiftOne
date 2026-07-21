import React from 'react';
import {
  Button,
  ButtonGroup,
  Flex,
  Icon,
  IconButton,
  Select,
  Text,
  useColorModeValue,
} from '@chakra-ui/react';
import { useTranslation } from 'react-i18next';
import { MdChevronLeft, MdChevronRight } from 'react-icons/md';

interface SharedPaginationProps {
  page: number;
  pageSize: number;
  totalCount: number;
  onPageChange: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
  pageSizeOptions?: number[];
  isDisabled?: boolean;
}

export default function SharedPagination({
  page,
  pageSize,
  totalCount,
  onPageChange,
  onPageSizeChange,
  pageSizeOptions = [10, 20, 50, 100],
  isDisabled = false,
}: SharedPaginationProps) {
  const { t, i18n } = useTranslation();
  const mutedText = useColorModeValue('secondaryGray.600', 'secondaryGray.400');
  const borderColor = useColorModeValue('gray.200', 'whiteAlpha.100');
  const isRtl = i18n.language?.startsWith('ar');
  const totalPages = Math.max(1, Math.ceil(totalCount / Math.max(pageSize, 1)));
  const start = totalCount === 0 ? 0 : (page - 1) * pageSize + 1;
  const end = Math.min(page * pageSize, totalCount);

  return (
    <Flex
      align={{ base: 'stretch', md: 'center' }}
      justify="space-between"
      direction={{ base: 'column', md: 'row' }}
      gap="12px"
      px="16px"
      py="14px"
      borderTop="1px solid"
      borderColor={borderColor}
    >
      <Text color={mutedText} fontSize="sm" fontWeight="500">
        {t('common.showingRange', { start, end, total: totalCount })}
      </Text>
      <Flex align="center" justify={{ base: 'space-between', md: 'flex-end' }} gap="12px">
        {onPageSizeChange && (
          <Select
            size="sm"
            w="84px"
            value={pageSize}
            onChange={(event) => onPageSizeChange(Number(event.target.value))}
            isDisabled={isDisabled}
            borderRadius="8px"
          >
            {pageSizeOptions.map((option) => (
              <option key={option} value={option}>
                {option}
              </option>
            ))}
          </Select>
        )}
        <ButtonGroup size="sm" isAttached={false}>
          <IconButton
            aria-label={t('common.previousPage')}
            icon={<Icon as={(isRtl ? MdChevronRight : MdChevronLeft) as React.ElementType} />}
            variant="outline"
            borderRadius="8px"
            onClick={() => onPageChange(Math.max(1, page - 1))}
            isDisabled={isDisabled || page <= 1}
          />
          <Button variant="outline" borderRadius="8px" isDisabled>
            {t('common.pageProgress', { page, totalPages })}
          </Button>
          <IconButton
            aria-label={t('common.nextPage')}
            icon={<Icon as={(isRtl ? MdChevronLeft : MdChevronRight) as React.ElementType} />}
            variant="outline"
            borderRadius="8px"
            onClick={() => onPageChange(Math.min(totalPages, page + 1))}
            isDisabled={isDisabled || page >= totalPages}
          />
        </ButtonGroup>
      </Flex>
    </Flex>
  );
}