import { Box, Flex, Text, useColorModeValue } from '@chakra-ui/react';

export default function SidebarBrand() {
  const logoColor = useColorModeValue('navy.700', 'white');

  return (
    <Flex align="center" direction="column" py="28px">
      <Text fontSize="24px" fontWeight="800" color={logoColor} letterSpacing="0">
        SHIFT<Text as="span" color="brand.500">ONE</Text>
      </Text>
      <Box h="1px" w="100%" bg={useColorModeValue('gray.200', 'whiteAlpha.200')} mt="20px" />
    </Flex>
  );
}
