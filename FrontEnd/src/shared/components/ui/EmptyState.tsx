import { Alert, AlertIcon, Button, Flex, Text, useColorModeValue } from '@chakra-ui/react';

interface EmptyStateProps {
  title: string;
  description?: string;
  actionLabel?: string;
  onAction?: () => void;
  status?: 'info' | 'warning' | 'success' | 'error' | 'loading';
}

export default function EmptyState({ title, description, actionLabel, onAction, status = 'info' }: EmptyStateProps) {
  const textColor = useColorModeValue('secondaryGray.900', 'white');

  return (
    <Alert status={status} borderRadius="8px" alignItems="flex-start">
      <AlertIcon mt="3px" />
      <Flex direction="column" gap="8px">
        <Text color={textColor} fontWeight="700">{title}</Text>
        {description ? <Text fontSize="sm">{description}</Text> : null}
        {actionLabel && onAction ? (
          <Button size="sm" alignSelf="flex-start" onClick={onAction}>
            {actionLabel}
          </Button>
        ) : null}
      </Flex>
    </Alert>
  );
}
