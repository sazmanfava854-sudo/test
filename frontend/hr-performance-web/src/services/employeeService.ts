import api from './api';
import type {
  ApiResponse,
  EmployeeDto,
  EmployeeLookupDto,
  EmployeeLookupRequest,
  EmployeeSearchRequest,
  PagedResult,
} from '../types';
import { unwrapApiData, unwrapPagedItems, readApiSuccess, readApiMessage } from '../utils/apiResponse';

export const employeeService = {
  async lookup(
    params: EmployeeLookupRequest,
  ): Promise<ApiResponse<PagedResult<EmployeeLookupDto>>> {
    const { data } = await api.get('/employees/lookup', { params });
    const payload = unwrapApiData<PagedResult<EmployeeLookupDto>>(data);
    const items = payload ? unwrapPagedItems<EmployeeLookupDto>({ data: payload }) : unwrapPagedItems<EmployeeLookupDto>(data);
    const paged = payload ?? (data as PagedResult<EmployeeLookupDto>);
    return {
      success: readApiSuccess(data),
      message: readApiMessage(data),
      data: {
        items,
        totalCount: paged?.totalCount ?? items.length,
        pageNumber: paged?.pageNumber ?? params.pageNumber ?? 1,
        pageSize: paged?.pageSize ?? params.pageSize ?? 20,
        totalPages: paged?.totalPages ?? 1,
        hasNext: paged?.hasNext ?? false,
        hasPrevious: paged?.hasPrevious ?? false,
      },
    };
  },

  async syncRosterFromMis(): Promise<{
    success: boolean;
    message?: string;
    inserted?: number;
    updated?: number;
    total?: number;
  }> {
    const { data } = await api.post('/employees/sync-from-mis');
    return data;
  },

  async getSummary(): Promise<{
    success?: boolean;
    employeesInDatabase?: number;
    lastEmployeeRosterSyncAt?: string | null;
    isRosterSyncRunning?: boolean;
    message?: string;
  }> {
    const { data } = await api.get('/employees/summary');
    return data;
  },

  async getAll(
    params: EmployeeSearchRequest,
  ): Promise<ApiResponse<PagedResult<EmployeeDto>>> {
    const { data } = await api.get('/employees', { params });
    const payload = unwrapApiData<PagedResult<EmployeeDto>>(data) ?? unwrapApiData<PagedResult<EmployeeDto>>({ data });
    const items = payload ? unwrapPagedItems<EmployeeDto>({ data: payload }) : unwrapPagedItems<EmployeeDto>(data);
    const paged = payload ?? (data as PagedResult<EmployeeDto>);
    return {
      success: readApiSuccess(data),
      message: readApiMessage(data),
      data: paged
        ? {
            ...paged,
            items,
            totalCount: paged.totalCount ?? (paged as { TotalCount?: number }).TotalCount ?? items.length,
            pageNumber: paged.pageNumber ?? (paged as { PageNumber?: number }).PageNumber ?? 1,
            pageSize: paged.pageSize ?? (paged as { PageSize?: number }).PageSize ?? items.length,
            totalPages: paged.totalPages ?? (paged as { TotalPages?: number }).TotalPages ?? 1,
            hasNext: paged.hasNext ?? (paged as { HasNext?: boolean }).HasNext ?? false,
            hasPrevious: paged.hasPrevious ?? (paged as { HasPrevious?: boolean }).HasPrevious ?? false,
          }
        : { items, totalCount: items.length, pageNumber: 1, pageSize: items.length, totalPages: 1, hasNext: false, hasPrevious: false },
    };
  },

  async getById(id: string): Promise<ApiResponse<EmployeeDto>> {
    const { data } = await api.get<ApiResponse<EmployeeDto>>(`/employees/${id}`);
    return data;
  },
};
