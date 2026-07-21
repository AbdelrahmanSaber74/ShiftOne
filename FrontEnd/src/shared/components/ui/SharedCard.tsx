import React from 'react';
import { Box, BoxProps, useColorModeValue } from '@chakra-ui/react';

export default function SharedCard({ children, ...props }: BoxProps) {
  const bg = useColorModeValue('white', 'navy.800');
  const borderColor = useColorModeValue('gray.200', 'whiteAlpha.100');

  return (
    <Box bg={bg} border="1px solid" borderColor={borderColor} borderRadius="8px" p="20px" {...props}>
      {children}
    </Box>
  );
}
