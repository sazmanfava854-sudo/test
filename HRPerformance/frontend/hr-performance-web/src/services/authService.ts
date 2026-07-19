import api from './api';
import type {
  ApiResponse,
  LoginRequest,
  LoginResponse,
  RefreshTokenRequest,
} from '../types';

export const authService = {
  async login(request: LoginRequest): Promise<ApiResponse<LoginResponse>> {
    const { data } = await api.post<ApiResponse<LoginResponse>>(
      '/auth/login',
      request,
    );
    return data;
  },

  async refresh(
    request: RefreshTokenRequest,
  ): Promise<ApiResponse<LoginResponse>> {
    const { data } = await api.post<ApiResponse<LoginResponse>>(
      '/auth/refresh',
      request,
    );
    return data;
  },

  async getMe(): Promise<unknown> {
    const { data } = await api.get('/auth/me');
    return data;
  },
};
