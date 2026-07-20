import { Navigate } from 'react-router-dom';
import { useAppSelector } from '../../store/hooks';
import { selectUserRoles } from '../../store/authSlice';
import EmployeeDashboard from '../../pages/dashboard/EmployeeDashboard';

export default function DashboardHome() {
  const roles = useAppSelector(selectUserRoles);

  if (roles.includes('SuperAdministrator') || roles.includes('OrganizationAdministrator')) {
    return <Navigate to="/dashboard/admin" replace />;
  }
  if (roles.includes('Manager')) {
    return <Navigate to="/dashboard/manager" replace />;
  }
  return <EmployeeDashboard />;
}
