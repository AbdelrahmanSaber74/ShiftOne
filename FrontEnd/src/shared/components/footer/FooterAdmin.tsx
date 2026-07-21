import { Flex, Text, useColorModeValue } from '@chakra-ui/react';

export default function FooterAdmin() {
  const textColor = useColorModeValue('gray.500', 'whiteAlpha.700');
  return (
    <Flex zIndex="3" alignItems="center" justifyContent="center" px={{ base: '30px', md: '50px' }} pb="30px">
      <Text color={textColor} textAlign="center" fontSize="sm">
        &copy; {new Date().getFullYear()} <Text as="span" fontWeight="600">ShiftOne. All Rights Reserved.</Text>
      </Text>
    </Flex>
  );
}
