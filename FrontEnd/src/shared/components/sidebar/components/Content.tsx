import { Box, Flex, Stack } from '@chakra-ui/react';
import SidebarBrand from 'shared/components/sidebar/components/Brand';
import SidebarLinks from 'shared/components/sidebar/components/Links';
import type { AppRoute } from 'shared/types/routes';

interface SidebarContentProps {
  routes: AppRoute[];
}

export default function SidebarContent({ routes }: SidebarContentProps) {
  return (
    <Flex direction="column" height="100%" pt="12px" px="16px">
      <SidebarBrand />
      <Stack direction="column" mb="auto" mt="8px">
        <Box>
          <SidebarLinks routes={routes} />
        </Box>
      </Stack>
    </Flex>
  );
}
