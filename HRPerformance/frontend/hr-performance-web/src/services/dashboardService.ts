import api from './api';
import type {
  AdminDashboardDto,
  ApiResponse,
  EmployeeDashboardDto,
  ManagerDashboardDto,
} from '../types';

export const dashboardService = {
  async getEmployeeDashboard(): Promise<ApiResponse<EmployeeDashboardDto>> {
    const { data } = await api.get<ApiResponse<EmployeeDashboardDto>>(
      '/dashboard/employee',
    );
    return data;
  },

  async getManagerDashboard(): Promise<ApiResponse<ManagerDashboardDto>> {
    const { data } = await api.get<ApiResponse<ManagerDashboardDto>>(
      '/dashboard/manager',
    );
    return data;
  },

  async getAdminDashboard(): Promise<ApiResponse<AdminDashboardDto>> {
    const { data } = await api.get<ApiResponse<AdminDashboardDto>>(
      '/dashboard/admin',
    );
    return data;
  },
};
