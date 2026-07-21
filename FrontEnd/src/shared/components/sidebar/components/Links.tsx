import { Box, Flex, HStack, Text, useColorModeValue } from '@chakra-ui/react';
import { NavLink, useLocation } from 'react-router-dom';
import type { AppRoute } from 'shared/types/routes';
import { useTranslation } from 'react-i18next';
import { useAuth } from 'shared/contexts/AuthContext';
import { hasAnyPermission } from 'shared/utils/authUtils';

interface SidebarLinksProps {
  routes: AppRoute[];
}

export default function SidebarLinks({ routes }: SidebarLinksProps) {
  const location = useLocation();
  const activeColor = useColorModeValue('gray.700', 'white');
  const inactiveColor = useColorModeValue('secondaryGray.600', 'secondaryGray.400');
  const activeIcon = useColorModeValue('brand.500', 'white');
  const brandColor = useColorModeValue('brand.500', 'brand.400');
  const activeBg = useColorModeValue('blackAlpha.50', 'whiteAlpha.100');
  const { t } = useTranslation();
  const { user } = useAuth();
  const canSeeRoute = (route: AppRoute) => !route.permissions?.length || hasAnyPermission(user, route.permissions);

  return (
    <Flex direction="column" gap="4px">
      {routes.filter((route) => route.layout === '/admin' && route.path !== '/profile' && canSeeRoute(route)).map((route) => {
        const to = `${route.layout}${route.path}`;
        const isActive = location.pathname === to || location.pathname.includes(route.path);
        const title = route.translationKey ? t(route.translationKey) : route.name;

        return (
          <NavLink key={to} to={to} aria-current={isActive ? 'page' : undefined}>
            <HStack spacing="16px" py="10px" ps="12px" borderRadius="8px" bg={isActive ? activeBg : 'transparent'}>
              {route.icon ? <Box color={isActive ? activeIcon : inactiveColor}>{route.icon}</Box> : null}
              <Text flex="1" color={isActive ? activeColor : inactiveColor} fontWeight={isActive ? '700' : '500'}>
                {title}
              </Text>
              <Box h="28px" w="4px" bg={isActive ? brandColor : 'transparent'} borderRadius="4px" />
            </HStack>
          </NavLink>
        );
      })}
    </Flex>
  );
}