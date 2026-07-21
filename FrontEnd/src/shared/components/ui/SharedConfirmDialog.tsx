import React from 'react';
import {
  Button,
  Modal,
  ModalOverlay,
  ModalContent,
  ModalHeader,
  ModalBody,
  ModalFooter,
  ModalCloseButton,
  Text,
  useColorModeValue,
} from '@chakra-ui/react';
import { useTranslation } from 'react-i18next';

interface SharedConfirmDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => Promise<void> | void;
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  isDestructive?: boolean;
  loading?: boolean;
}

export default function SharedConfirmDialog({
  isOpen,
  onClose,
  onConfirm,
  title,
  message,
  confirmLabel,
  cancelLabel,
  isDestructive = false,
  loading = false,
}: SharedConfirmDialogProps) {
  const { t, i18n } = useTranslation();
  const isRtl = i18n.language?.startsWith('ar');
  const titleColor = useColorModeValue('secondaryGray.900', 'white');
  const mutedText = useColorModeValue('secondaryGray.600', 'secondaryGray.400');
  const contentBg = useColorModeValue('white', 'navy.900');
  const borderColor = useColorModeValue('gray.200', 'whiteAlpha.100');

  const handleConfirm = async () => {
    await onConfirm();
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} isCentered size="md">
      <ModalOverlay backdropFilter="blur(4px)" />
      <ModalContent bg={contentBg} borderRadius="8px" border="1px solid" borderColor={borderColor} overflow="hidden">
        <ModalHeader color={titleColor} fontWeight="800" fontSize="lg" pl={isRtl ? '48px' : '24px'} pr={isRtl ? '24px' : '48px'}>
          {title}
        </ModalHeader>
        <ModalCloseButton top="14px" left={isRtl ? '14px' : 'auto'} right={isRtl ? 'auto' : '14px'} borderRadius="8px" aria-label={t('common.close')} />
        <ModalBody pt="0">
          <Text color={mutedText} fontSize="sm" lineHeight="1.7" fontWeight="500">
            {message}
          </Text>
        </ModalBody>
        <ModalFooter gap="10px" borderTop="1px solid" borderColor={borderColor}>
          <Button variant="ghost" onClick={onClose} isDisabled={loading}>
            {cancelLabel ?? t('common.cancel')}
          </Button>
          <Button colorScheme={isDestructive ? 'red' : 'brand'} variant={isDestructive ? 'solid' : 'brand'} onClick={handleConfirm} isLoading={loading}>
            {confirmLabel ?? (isDestructive ? t('common.delete') : t('common.confirm'))}
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}


