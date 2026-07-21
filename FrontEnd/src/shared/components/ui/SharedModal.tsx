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

interface SharedModalProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  description?: string;
  children: React.ReactNode;
  footer?: React.ReactNode;
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl' | '3xl' | '4xl' | '5xl' | '6xl' | 'full';
  maxW?: string;
  isLoading?: boolean;
  initialFocusRef?: React.RefObject<any>;
}

const mapDrawerSize = (size: SharedModalProps['size']) => {
  if (size === 'xs' || size === 'sm') return 'sm';
  if (size === 'md' || size === 'lg') return 'md';
  if (size === 'xl' || size === '2xl' || size === '3xl') return 'lg';
  if (size === 'full') return 'full';
  return 'xl';
};

export default function SharedModal({
  isOpen,
  onClose,
  title,
  description,
  children,
  footer,
  size = 'lg',
  isLoading = false,
  initialFocusRef,
}: SharedModalProps) {
  const { t, i18n } = useTranslation();
  const isRtl = i18n.language?.startsWith('ar');

  const contentBg = useColorModeValue('white', 'navy.900');
  const titleColor = useColorModeValue('secondaryGray.900', 'white');
  const mutedText = useColorModeValue('secondaryGray.600', 'secondaryGray.400');
  const borderColor = useColorModeValue('gray.200', 'whiteAlpha.100');
  const footerBg = useColorModeValue('secondaryGray.50', 'navy.800');

  return (
    <Drawer
      isOpen={isOpen}
      onClose={onClose}
      size={mapDrawerSize(size)}
      placement={isRtl ? 'left' : 'right'}
      initialFocusRef={initialFocusRef}
      closeOnOverlayClick={!isLoading}
      closeOnEsc={!isLoading}
    >
      <DrawerOverlay backdropFilter="blur(4px)" bg="blackAlpha.500" />
      <DrawerContent bg={contentBg}>
        <DrawerCloseButton
          top="18px"
          left={isRtl ? '18px' : 'auto'}
          right={isRtl ? 'auto' : '18px'}
          borderRadius="8px"
          aria-label={t('common.close')}
          isDisabled={isLoading}
          _hover={{ bg: useColorModeValue('secondaryGray.100', 'whiteAlpha.100') }}
        />
        <DrawerHeader borderBottom="1px solid" borderColor={borderColor} py="18px" pl={isRtl ? '56px' : '22px'} pr={isRtl ? '22px' : '56px'}>
          <Heading as="h2" fontSize="lg" color={titleColor} fontWeight="800" letterSpacing="0">
            {title}
          </Heading>
          {description && (
            <Text color={mutedText} fontSize="sm" fontWeight="500" mt="4px" lineHeight="1.6">
              {description}
            </Text>
          )}
        </DrawerHeader>

        <DrawerBody py="22px" px="22px" overflowY="auto">
          <Box maxW="100%">{children}</Box>
        </DrawerBody>

        {footer && (
          <DrawerFooter
            borderTop="1px solid"
            borderColor={borderColor}
            gap="10px"
            py="14px"
            px="22px"
            bg={footerBg}
            justifyContent="flex-end"
          >
            {footer}
          </DrawerFooter>
        )}
      </DrawerContent>
    </Drawer>
  );
}




