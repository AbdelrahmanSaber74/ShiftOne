import { Box, Portal } from '@chakra-ui/react';
import { useMemo, useState } from 'react';
import { Navigate, Route, Routes, useLocation } from 'react-router-dom';
import Footer from 'shared/components/footer/FooterAdmin';
import Navbar from 'shared/components/navbar/NavbarAdmin';
import Sidebar from 'shared/components/sidebar/Sidebar';
import { SidebarContext } from 'shared/contexts/SidebarContext';
import routes from 'routes';
import { useAuth } from 'shared/contexts/AuthContext';
import type { AppRoute } from 'shared/types/routes';
import { useTranslation } from 'react-i18next';
import { hasAnyPermission } from 'shared/utils/authUtils';

export default function AdminLayout() {
  const [toggleSidebar, setToggleSidebar] = useState(false);
  const location = useLocation();
  const { t } = useTranslation();
  const { user } = useAuth();

  const canSeeRoute = (route: AppRoute) => !route.permissions?.length || hasAnyPermission(user, route.permissions);
  const availableRoutes = routes.filter((route) => route.layout !== '/admin' || canSeeRoute(route));

  const activeRoute = useMemo(
    () => availableRoutes.find((route) => route.layout === '/admin' && location.pathname.includes(route.path)),
    [availableRoutes, location.pathname],
  );

  const activeRouteTitle = activeRoute?.translationKey ? t(activeRoute.translationKey) : activeRoute?.name || t('navigation.dashboard');
  const breadcrumb = activeRoute?.breadcrumbKey ? t(activeRoute.breadcrumbKey) : activeRouteTitle;

  return (
    <SidebarContext.Provider value={{ toggleSidebar, setToggleSidebar }}>
      <Sidebar routes={availableRoutes} />
      <Box
        minHeight="100vh"
        overflow="auto"
        position="relative"
        w={{ base: '100%', xl: 'calc(100% - 300px)' }}
        ms={{ base: '0', xl: '300px' }}
        me="0"
      >
        <Portal>
          <Navbar brandText={activeRouteTitle} breadcrumb={breadcrumb} />
        </Portal>
        <Box mx="auto" p={{ base: '16px', md: '24px' }} minH="100vh" pt={{ base: '104px', md: '88px' }}>
          <Routes>
            {availableRoutes.filter((route) => route.layout === '/admin').map((route) => (
              <Route path={route.path} element={route.component} key={`${route.layout}${route.path}`} />
            ))}
            <Route path="/" element={<Navigate to="/admin/default" replace />} />
          </Routes>
        </Box>
        <Footer />
      </Box>
    </SidebarContext.Provider>
  );
}

