import React from 'react';
import { Box, Heading, Text, useColorModeValue } from '@chakra-ui/react';

interface PageShellProps {
  title: string;
  description: string;
  children: React.ReactNode;
}

export default function PageShell({ title, description, children }: PageShellProps) {
  const textColor = useColorModeValue('secondaryGray.900', 'white');
  const mutedText = useColorModeValue('secondaryGray.600', 'secondaryGray.400');

  return (
    <Box pt={{ base: '130px', md: '80px', xl: '80px' }}>
      <Box mb="24px">
        <Heading size="lg" color={textColor}>{title}</Heading>
        <Text color={mutedText} mt="6px">{description}</Text>
      </Box>
      {children}
    </Box>
  );
}
