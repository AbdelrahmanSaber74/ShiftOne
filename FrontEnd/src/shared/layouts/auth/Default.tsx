import React, { ReactNode } from 'react';
import { Box, Flex, Grid, useColorModeValue } from '@chakra-ui/react';
import Footer from 'shared/components/footer/FooterAuth';
import { useTranslation } from 'react-i18next';
import LanguageSwitcher from 'shared/components/navbar/LanguageSwitcher';

interface AuthIllustrationProps {
  children: ReactNode;
  illustrationBackground: string;
}

export default function AuthIllustration({ children, illustrationBackground }: AuthIllustrationProps) {
  const { i18n } = useTranslation();
  const isRtl = i18n.language?.startsWith('ar');

  const bg = useColorModeValue('white', 'navy.900');
  const illustrationBgColor = useColorModeValue('brand.50', 'navy.800');

  return (
    <Grid
      minH="100vh"
      templateColumns={{ base: '1fr', lg: '1.1fr 0.9fr' }}
      bg={bg}
    >
      {/* Form & Header Column */}
      <Flex
        direction="column"
        minH="100vh"
        px={{ base: '24px', md: '40px', xl: '80px' }}
        py="30px"
        justify="space-between"
      >
        {/* Top Header */}
        <Flex justify="flex-end" align="center" w="100%" mt="40px" mb="10px">
          <LanguageSwitcher />
        </Flex>

        {/* Center Form Content */}
        <Flex justify="center" align="center" flex="1" w="100%">
          {children}
        </Flex>

        {/* Footer */}
        <Box mt="20px">
          <Footer />
        </Box>
      </Flex>

      {/* Illustration Column */}
      <Box
        display={{ base: 'none', lg: 'block' }}
        bg={illustrationBgColor}
        position="relative"
        overflow="hidden"
      >
        <Flex
          bg={`url(${illustrationBackground})`}
          w="100%"
          h="100%"
          bgSize="cover"
          bgPosition="center"
          borderBottomStartRadius={isRtl ? 'none' : '150px'}
          borderBottomEndRadius={isRtl ? '150px' : 'none'}
        />
      </Box>
    </Grid>
  );
}
