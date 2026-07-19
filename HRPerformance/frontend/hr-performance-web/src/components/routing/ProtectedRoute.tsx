import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAppSelector } from '../../store/hooks';
import { selectIsAuthenticated, selectUserRoles } from '../../store/authSlice';

interface ProtectedRouteProps {
  children?: React.ReactNode;
  roles?: string[];
}

export function ProtectedRoute({ children, roles }: ProtectedRouteProps) {
  const isAuthenticated = useAppSelector(selectIsAuthenticated);
  const userRoles = useAppSelector(selectUserRoles);
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (roles && roles.length > 0) {
    const hasRole = roles.some((r) => userRoles.includes(r));
    if (!hasRole) {
      return <Navigate to="/dashboard" replace />;
    }
  }

  return children ? <>{children}</> : <Outlet />;
}

export function PublicRoute({ children }: { children: React.ReactNode }) {
  const isAuthenticated = useAppSelector(selectIsAuthenticated);
  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }
  return <>{children}</>;
}
