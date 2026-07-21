import React from 'react';
import {
  Box,
  Drawer,
  DrawerBody,
  DrawerCloseButton,
  DrawerContent,
  DrawerFooter,
  DrawerHeader,
  DrawerOverlay,
  Heading,
  Text,
  useColorModeValue,
} from '@chakra-ui/react';
import { useTranslation } from 'react-i18next';

interface SharedDrawerProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  description?: string;
  children: React.ReactNode;
  footer?: React.ReactNode;
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | 'full';
}

export default function SharedDrawer({
  isOpen,
  onClose,
  title,
  description,
  children,
  footer,
  size = 'md',
}: SharedDrawerProps) {
  const { t, i18n } = useTranslation();
  const isRtl = i18n.language?.startsWith('ar');

  const contentBg = useColorModeValue('white', 'navy.900');
  const titleColor = useColorModeValue('secondaryGray.900', 'white');
  const mutedText = useColorModeValue('secondaryGray.600', 'secondaryGray.400');
  const borderColor = useColorModeValue('gray.200', 'whiteAlpha.100');

  return (
    <Drawer isOpen={isOpen} onClose={onClose} placement={isRtl ? 'left' : 'right'} size={size}>
      <DrawerOverlay backdropFilter="blur(4px)" />
      <DrawerContent bg={contentBg}>
        <DrawerCloseButton top="18px" left={isRtl ? '18px' : 'auto'} right={isRtl ? 'auto' : '18px'} borderRadius="8px" aria-label={t('common.close')} />
        <DrawerHeader borderBottom="1px solid" borderColor={borderColor} py="18px" pl={isRtl ? '56px' : '22px'} pr={isRtl ? '22px' : '56px'}>
          <Heading as="h2" fontSize="lg" color={titleColor} fontWeight="800" letterSpacing="0">
            {title}
          </Heading>
          {description && (
            <Text color={mutedText} fontSize="sm" fontWeight="500" mt="4px">
              {description}
            </Text>
          )}
        </DrawerHeader>
        <DrawerBody py="22px" px="22px">
          <Box maxW="100%">{children}</Box>
        </DrawerBody>
        {footer && (
          <DrawerFooter borderTop="1px solid" borderColor={borderColor} gap="10px" py="14px" px="22px">
            {footer}
          </DrawerFooter>
        )}
      </DrawerContent>
    </Drawer>
  );
}


