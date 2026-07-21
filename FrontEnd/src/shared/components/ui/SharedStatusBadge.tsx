import React from 'react';
import { Flex, Box, Text, useColorModeValue } from '@chakra-ui/react';
import { useTranslation } from 'react-i18next';

interface SharedStatusBadgeProps {
  isActive: boolean;
  activeLabel?: string;
  inactiveLabel?: string;
}

export default function SharedStatusBadge({ isActive, activeLabel, inactiveLabel }: SharedStatusBadgeProps) {
  const { t } = useTranslation();

  const label = isActive
    ? (activeLabel ?? t('common.active'))
    : (inactiveLabel ?? t('common.inactive'));

  const bg = useColorModeValue(
    isActive ? 'green.50' : 'red.50',
    isActive ? 'rgba(72, 187, 120, 0.1)' : 'rgba(245, 101, 101, 0.1)'
  );

  const textColor = useColorModeValue(
    isActive ? 'green.700' : 'red.700',
    isActive ? 'green.300' : 'red.300'
  );

  const dotColor = isActive ? 'green.500' : 'red.500';

  return (
    <Flex
      align="center"
      bg={bg}
      color={textColor}
      borderRadius="full"
      px="10px"
      py="4px"
      w="fit-content"
      gap="6px"
    >
      <Box h="6px" w="6px" borderRadius="full" bg={dotColor} />
      <Text fontSize="xs" fontWeight="700" lineHeight="1">
        {label}
      </Text>
    </Flex>
  );
}