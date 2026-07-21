import { Box, Drawer, DrawerBody, DrawerCloseButton, DrawerContent, DrawerOverlay, Flex, Icon, useColorModeValue, useDisclosure } from '@chakra-ui/react';
import { useRef } from 'react';
import { IoMenuOutline } from 'react-icons/io5';
import SidebarContent from 'shared/components/sidebar/components/Content';
import type { AppRoute } from 'shared/types/routes';
import { useTranslation } from 'react-i18next';

interface SidebarProps {
  routes: AppRoute[];
}

export default function Sidebar({ routes }: SidebarProps) {
  const shadow = useColorModeValue('14px 17px 40px 4px rgba(112, 144, 176, 0.08)', 'unset');
  const sidebarBg = useColorModeValue('white', 'navy.800');

  return (
    <Box
      display={{ base: 'none', xl: 'block' }}
      w="300px"
      position="fixed"
      insetStart="0"
      minH="100%"
    >
      <Box bg={sidebarBg} w="300px" h="100vh" minH="100%" overflowY="auto" boxShadow={shadow}>
        <SidebarContent routes={routes} />
      </Box>
    </Box>
  );
}


export function SidebarResponsive({ routes }: SidebarProps) {
  const sidebarBackgroundColor = useColorModeValue('white', 'navy.800');
  const menuColor = useColorModeValue('gray.500', 'white');
  const { isOpen, onOpen, onClose } = useDisclosure();
  const btnRef = useRef<HTMLDivElement | null>(null);
  const { t, i18n } = useTranslation();

  return (
    <Flex display={{ base: 'flex', xl: 'none' }} alignItems="center">
      <Flex ref={btnRef} w="max-content" h="max-content" onClick={onOpen} role="button" aria-label={t('common.openNavigation')}>
        <Icon as={IoMenuOutline as React.ElementType} color={menuColor} my="auto" w="22px" h="22px" me="10px" _hover={{ cursor: 'pointer' }} />
      </Flex>
      <Drawer isOpen={isOpen} onClose={onClose} placement={i18n.language?.startsWith('ar') ? 'right' : 'left'}>
        <DrawerOverlay />
        <DrawerContent w="285px" maxW="285px" bg={sidebarBackgroundColor}>
          <DrawerCloseButton zIndex="3" left={i18n.language?.startsWith('ar') ? '16px' : 'auto'} right={i18n.language?.startsWith('ar') ? 'auto' : '16px'} aria-label={t('common.close')} _focus={{ boxShadow: 'none' }} _hover={{ boxShadow: 'none' }} />
          <DrawerBody maxW="285px" px="0" pb="0">
            <SidebarContent routes={routes} />
          </DrawerBody>
        </DrawerContent>
      </Drawer>
    </Flex>
  );
}



