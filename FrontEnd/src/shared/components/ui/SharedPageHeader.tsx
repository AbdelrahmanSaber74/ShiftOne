import React from 'react';
import { Box, Flex, Heading, Text, useColorModeValue } from '@chakra-ui/react';

interface SharedPageHeaderProps {
  title: string;
  description: string;
  action?: React.ReactNode;
}

export default function SharedPageHeader({ title, description, action }: SharedPageHeaderProps) {
  const titleColor = useColorModeValue('secondaryGray.900', 'white');
  const descColor = useColorModeValue('secondaryGray.600', 'secondaryGray.400');

  return (
    <Flex
      direction={{ base: 'column', md: 'row' }}
      justify="space-between"
      align={{ base: 'flex-start', md: 'center' }}
      gap="16px"
      mb="18px"
      w="100%"
    >
      <Box maxW="720px">
        <Heading as="h1" size="lg" color={titleColor} fontWeight="800" letterSpacing="0">
          {title}
        </Heading>
        <Text color={descColor} fontSize="sm" mt="5px" fontWeight="500">
          {description}
        </Text>
      </Box>
      {action && <Box alignSelf={{ base: 'stretch', md: 'auto' }}>{action}</Box>}
    </Flex>
  );
}
