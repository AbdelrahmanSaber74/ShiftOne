import React from 'react';
import { Badge } from '@chakra-ui/react';
import { useTranslation } from 'react-i18next';

interface StatusBadgeProps {
  isActive: boolean;
  activeLabel?: string;
  inactiveLabel?: string;
}

export default function StatusBadge({ isActive, activeLabel, inactiveLabel }: StatusBadgeProps) {
  const { t } = useTranslation();
  const label = isActive
    ? (activeLabel ?? t('common.active'))
    : (inactiveLabel ?? t('common.inactive'));

  return (
    <Badge colorScheme={isActive ? 'green' : 'red'}>
      {label}
    </Badge>
  );
}
