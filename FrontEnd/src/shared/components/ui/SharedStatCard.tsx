import React from 'react';
import { Box, Flex, Stat, StatLabel, StatNumber, useColorModeValue } from '@chakra-ui/react';

interface SharedStatCardProps {
  label: string;
  value: string | number;
  icon?: React.ReactNode;
  tone?: 'brand' | 'green' | 'orange' | 'purple' | 'red';
}

export default function SharedStatCard({ label, value, icon, tone = 'brand' }: SharedStatCardProps) {
  const cardBg = useColorModeValue('white', 'navy.800');
  const textColor = useColorModeValue('secondaryGray.900', 'white');
  const labelColor = useColorModeValue('secondaryGray.600', 'secondaryGray.400');
  const borderColor = useColorModeValue('gray.200', 'whiteAlpha.100');

  const toneBg = {
    brand: useColorModeValue('brand.50', 'rgba(23, 105, 224, 0.16)'),
    green: useColorModeValue('green.50', 'rgba(1, 181, 116, 0.15)'),
    orange: useColorModeValue('orange.50', 'rgba(255, 181, 40, 0.15)'),
    purple: useColorModeValue('purple.50', 'rgba(128, 90, 213, 0.15)'),
    red: useColorModeValue('red.50', 'rgba(229, 62, 62, 0.15)'),
  }[tone];

  const toneColor = {
    brand: 'brand.600',
    green: 'green.600',
    orange: 'orange.600',
    purple: 'purple.600',
    red: 'red.600',
  }[tone];

  return (
    <Box bg={cardBg} borderRadius="8px" p="18px" border="1px solid" borderColor={borderColor}>
      <Flex justify="space-between" align="center" gap="16px">
        <Stat minW="0">
          <StatLabel color={labelColor} fontSize="xs" fontWeight="700" textTransform="uppercase" letterSpacing="0">
            {label}
          </StatLabel>
          <StatNumber color={textColor} fontSize="xl" fontWeight="800" mt="6px" letterSpacing="0" noOfLines={1}>
            {value}
          </StatNumber>
        </Stat>
        {icon && (
          <Flex align="center" justify="center" h="40px" w="40px" borderRadius="8px" bg={toneBg} color={toneColor} fontSize="22px" flex="0 0 auto">
            {icon}
          </Flex>
        )}
      </Flex>
    </Box>
  );
}
