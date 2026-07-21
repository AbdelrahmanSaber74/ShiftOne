import { Center, Spinner } from '@chakra-ui/react';
import { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from 'shared/contexts/AuthContext';

interface PublicRouteProps {
  children: ReactNode;
}

export default function PublicRoute({ children }: PublicRouteProps) {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return (
      <Center h="100vh">
        <Spinner size="xl" color="brand.500" thickness="4px" />
      </Center>
    );
  }

  if (isAuthenticated) {
    return <Navigate to="/admin/default" replace />;
  }

  return <>{children}</>;
}
