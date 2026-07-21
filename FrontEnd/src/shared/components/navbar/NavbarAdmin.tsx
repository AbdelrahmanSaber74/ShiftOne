import { Box, Flex, useColorModeValue } from '@chakra-ui/react';
import AdminNavbarLinks from 'shared/components/navbar/NavbarLinksAdmin';

interface AdminNavbarProps {
  brandText: string;
  breadcrumb?: string;
  secondary?: boolean;
  message?: string;
}

export default function AdminNavbar({ secondary = false }: AdminNavbarProps) {
  const navbarBg = useColorModeValue('rgba(248, 250, 252, 0.92)', 'rgba(11, 17, 32, 0.86)');
  const borderColor = useColorModeValue('gray.200', 'whiteAlpha.100');

  return (
    <Box
      position="fixed"
      bg={navbarBg}
      backdropFilter="blur(16px)"
      borderBottom="1px solid"
      borderColor={borderColor}
      insetStart={{ base: '0', xl: '300px' }}
      insetEnd="0"
      px={{ base: '16px', md: '24px' }}
      py="12px"
      top="0"
      zIndex="10"
    >
      <Flex w="100%" alignItems="center" justify="flex-end" gap="10px">
        <Box w={{ base: '100%', md: 'unset' }}>
          <AdminNavbarLinks secondary={secondary} />
        </Box>
      </Flex>
    </Box>
  );
}
