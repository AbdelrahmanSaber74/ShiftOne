import React from 'react';
import {
  Alert,
  AlertIcon,
  Box,
  Button,
  Flex,
  Heading,
  Icon,
  Skeleton,
  Text,
  useColorModeValue,
} from '@chakra-ui/react';
import { useTranslation } from 'react-i18next';
import { MdInbox, MdSearchOff } from 'react-icons/md';

interface SharedLoadingProps {
  rows?: number;
  height?: string;
}

export function SharedLoading({ rows = 5, height = '40px' }: SharedLoadingProps) {
  return (
    <Box py="10px" w="100%">
      {[...Array(rows)].map((_, index) => (
        <Skeleton key={index} height={height} my="12px" borderRadius="8px" />
      ))}
    </Box>
  );
}

interface SharedEmptyStateProps {
  title: string;
  description?: string;
  actionLabel?: string;
  onAction?: () => void;
}

export function SharedEmptyState({
  title,
  description,
  actionLabel,
  onAction,
}: SharedEmptyStateProps) {
  const textColor = useColorModeValue('navy.700', 'white');
  const descColor = useColorModeValue('secondaryGray.600', 'secondaryGray.400');
  const iconColor = useColorModeValue('gray.300', 'gray.600');
  const borderColor = useColorModeValue('gray.200', 'whiteAlpha.100');
  const bg = useColorModeValue('gray.50', 'whiteAlpha.50');

  return (
    <Flex
      direction="column"
      align="center"
      justify="center"
      py="60px"
      px="20px"
      borderRadius="8px"
      border="2px dashed"
      borderColor={borderColor}
      bg={bg}
      w="100%"
      textAlign="center"
    >
      <Icon as={MdInbox as React.ElementType} boxSize="48px" color={iconColor} mb="16px" />
      <Heading as="h4" size="md" color={textColor} fontWeight="700">
        {title}
      </Heading>
      {description && (
        <Text color={descColor} fontSize="sm" mt="8px" maxW="400px">
          {description}
        </Text>
      )}
      {actionLabel && onAction && (
        <Button variant="brand" size="sm" mt="20px" onClick={onAction}>
          {actionLabel}
        </Button>
      )}
    </Flex>
  );
}

interface SharedNoResultsProps {
  title?: string;
  description?: string;
  onClear?: () => void;
}

export function SharedNoResults({ title, description, onClear }: SharedNoResultsProps) {
  const { t } = useTranslation();
  const textColor = useColorModeValue('navy.700', 'white');
  const descColor = useColorModeValue('secondaryGray.600', 'secondaryGray.400');

  return (
    <Flex direction="column" align="center" justify="center" py="48px" px="20px" w="100%" textAlign="center">
      <Icon as={MdSearchOff as React.ElementType} boxSize="40px" color="red.400" mb="12px" />
      <Heading as="h4" size="sm" color={textColor} fontWeight="700">
        {title ?? t('common.noMatchingResults')}
      </Heading>
      <Text color={descColor} fontSize="sm" mt="6px" maxW="360px">
        {description ?? t('common.noMatchingResultsDescription')}
      </Text>
      {onClear && (
        <Button variant="ghost" size="sm" mt="16px" onClick={onClear}>
          {t('common.clearFilters')}
        </Button>
      )}
    </Flex>
  );
}

interface SharedErrorStateProps {
  message?: string;
  onRetry?: () => void;
}

export function SharedErrorState({ message, onRetry }: SharedErrorStateProps) {
  const { t } = useTranslation();

  return (
    <Alert status="error" borderRadius="8px" p="16px" alignItems="flex-start">
      <AlertIcon mt="3px" />
      <Flex direction="column" gap="8px" w="100%">
        <Box fontWeight="700">{t('common.error')}</Box>
        {message && <Text fontSize="sm">{message}</Text>}
        {onRetry && (
          <Button size="xs" colorScheme="red" variant="outline" alignSelf="flex-start" onClick={onRetry} mt="4px">
            {t('common.tryAgain')}
          </Button>
        )}
      </Flex>
    </Alert>
  );
}