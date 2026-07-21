import { Center, Spinner } from '@chakra-ui/react';
import { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from 'shared/contexts/AuthContext';

import { hasPermission } from 'shared/utils/authUtils';

interface ProtectedRouteProps {
  children: ReactNode;
  requiredPermission?: string;
}

export default function ProtectedRoute({ children, requiredPermission }: ProtectedRouteProps) {
  const { isAuthenticated, isLoading, user } = useAuth();

  if (isLoading) {
    return (
      <Center h="100vh">
        <Spinner size="xl" color="brand.500" thickness="4px" />
      </Center>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/auth/sign-in" replace />;
  }

  if (requiredPermission && !hasPermission(user, requiredPermission)) {
    return <Navigate to="/admin/default" replace />;
  }

  return <>{children}</>;
}
