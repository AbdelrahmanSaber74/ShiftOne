import { Flex, Text, useColorModeValue } from '@chakra-ui/react';

export default function FooterAuth() {
  const textColor = useColorModeValue('gray.500', 'whiteAlpha.700');
  return (
    <Flex alignItems="center" justifyContent="center" py="24px">
      <Text color={textColor} fontSize="sm">ShiftOne Admin</Text>
    </Flex>
  );
}
