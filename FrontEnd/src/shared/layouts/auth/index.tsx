import { Box, useColorModeValue } from '@chakra-ui/react';
import { Navigate, Route, Routes } from 'react-router-dom';
import routes from 'routes';

import { useTranslation } from 'react-i18next';

export default function AuthLayout() {
  const authBg = useColorModeValue('white', 'navy.900');
  const { i18n } = useTranslation();
  document.documentElement.dir = i18n.language?.startsWith('ar') ? 'rtl' : 'ltr';

  return (
    <Box bg={authBg} minH="100vh" position="relative" w="100%">
      <Box mx="auto" minH="100vh">
        <Routes>
          {routes.filter((route) => route.layout === '/auth').map((route) => (
            <Route path={route.path} element={route.component} key={`${route.layout}${route.path}`} />
          ))}
          <Route path="/" element={<Navigate to="/auth/sign-in" replace />} />
        </Routes>
      </Box>
    </Box>
  );
}
