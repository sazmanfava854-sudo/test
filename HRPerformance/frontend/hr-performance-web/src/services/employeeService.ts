import api from './api';
import type {
  ApiResponse,
  EmployeeDto,
  EmployeeSearchRequest,
  PagedResult,
} from '../types';

export const employeeService = {
  async getAll(
    params: EmployeeSearchRequest,
  ): Promise<ApiResponse<PagedResult<EmployeeDto>>> {
    const { data } = await api.get<ApiResponse<PagedResult<EmployeeDto>>>(
      '/employees',
      { params },
    );
    return data;
  },

  async getById(id: string): Promise<ApiResponse<EmployeeDto>> {
    const { data } = await api.get<ApiResponse<EmployeeDto>>(`/employees/${id}`);
    return data;
  },
};
