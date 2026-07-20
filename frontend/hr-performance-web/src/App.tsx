import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import MainLayout from './components/layout/MainLayout';
import LoginPage from './pages/auth/LoginPage';
import DashboardHome from './components/routing/DashboardHome';
import EmployeeDashboard from './pages/dashboard/EmployeeDashboard';
import ManagerDashboard from './pages/dashboard/ManagerDashboard';
import AdminDashboard from './pages/dashboard/AdminDashboard';
import EmployeeListPage from './pages/employees/EmployeeListPage';
import EvaluationPage from './pages/evaluations/EvaluationPage';
import AppealsPage from './pages/appeals/AppealsPage';
import SettingsPage from './pages/settings/SettingsPage';
import ReportsPage from './pages/reports/ReportsPage';
import { ProtectedRoute, PublicRoute } from './components/routing/ProtectedRoute';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route
          path="/login"
          element={
            <PublicRoute>
              <LoginPage />
            </PublicRoute>
          }
        />

        <Route element={<ProtectedRoute />}>
          <Route element={<MainLayout />}>
            <Route path="/dashboard" element={<DashboardHome />} />
            <Route path="/dashboard/employee" element={<EmployeeDashboard />} />
            <Route
              path="/dashboard/manager"
              element={
                <ProtectedRoute roles={['Manager', 'OrganizationAdministrator', 'SuperAdministrator']}>
                  <ManagerDashboard />
                </ProtectedRoute>
              }
            />
            <Route
              path="/dashboard/admin"
              element={
                <ProtectedRoute roles={['OrganizationAdministrator', 'SuperAdministrator']}>
                  <AdminDashboard />
                </ProtectedRoute>
              }
            />
            <Route
              path="/employees"
              element={
                <ProtectedRoute roles={['Manager', 'OrganizationAdministrator', 'SuperAdministrator']}>
                  <EmployeeListPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/evaluations"
              element={
                <ProtectedRoute roles={['Manager', 'OrganizationAdministrator', 'SuperAdministrator']}>
                  <EvaluationPage />
                </ProtectedRoute>
              }
            />
            <Route path="/appeals" element={<AppealsPage />} />
            <Route path="/reports" element={<ReportsPage />} />
            <Route
              path="/settings"
              element={
                <ProtectedRoute roles={['OrganizationAdministrator', 'SuperAdministrator']}>
                  <SettingsPage />
                </ProtectedRoute>
              }
            />
          </Route>
        </Route>

        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
