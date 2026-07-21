import React from 'react';
import 'shared/assets/css/App.css';
import { Navigate, Route, Routes } from 'react-router-dom';
import { ChakraProvider, extendTheme } from '@chakra-ui/react';
import AuthLayout from 'shared/layouts/auth';
import AdminLayout from 'shared/layouts/admin';
import initialTheme from 'shared/theme/theme';
import { AuthProvider } from 'shared/contexts/AuthContext';
import ProtectedRoute from 'shared/components/common/ProtectedRoute';
import PublicRoute from 'shared/components/common/PublicRoute';
import './i18n'; // Initialize i18n
import { useTranslation } from 'react-i18next';
import { QueryClientProvider } from '@tanstack/react-query';
import { queryClient } from 'shared/query/queryClient';

export default function App() {
  const { i18n } = useTranslation();
  const isRtl = i18n.language?.startsWith('ar');

  const currentTheme = React.useMemo(() => {
    const dir = isRtl ? 'rtl' : 'ltr';
    return extendTheme({ ...initialTheme, direction: dir });
  }, [isRtl]);

  return (
    <ChakraProvider
      theme={currentTheme}
      toastOptions={{
        defaultOptions: {
          position: isRtl ? 'top-left' : 'top-right',
          duration: 3000,
          isClosable: true,
        },
      }}
    >
      <QueryClientProvider client={queryClient}>
        <AuthProvider>
        <Routes>
          <Route
            path="auth/*"
            element={(
              <PublicRoute>
                <AuthLayout />
              </PublicRoute>
            )}
          />
          <Route
            path="admin/*"
            element={(
              <ProtectedRoute>
                <AdminLayout />
              </ProtectedRoute>
            )}
          />
          <Route path="/" element={<Navigate to="/admin/default" replace />} />
          <Route path="*" element={<Navigate to="/admin/default" replace />} />
        </Routes>
        </AuthProvider>
      </QueryClientProvider>
    </ChakraProvider>
  );
}


