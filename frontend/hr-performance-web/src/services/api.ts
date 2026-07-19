import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios';
import { store } from '../store/store';
import { logout, setTokens } from '../store/authSlice';

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
  maxRedirects: 0,
  validateStatus: (status) => status >= 200 && status < 300,
});

let isRefreshing = false;
let failedQueue: Array<{
  resolve: (token: string) => void;
  reject: (error: unknown) => void;
}> = [];

const processQueue = (error: unknown, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else if (token) {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const { accessToken } = store.getState().auth;
  if (accessToken && config.headers) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }
  return config;
});

const isHtmlApiResponse = (response: { headers?: Record<string, unknown>; data?: unknown }) => {
  const contentType = String(response.headers?.['content-type'] ?? '');
  if (contentType.includes('text/html')) return true;
  return typeof response.data === 'string' && response.data.trimStart().startsWith('<!');
};

api.interceptors.response.use(
  (response) => {
    if (isHtmlApiResponse(response)) {
      return Promise.reject(
        Object.assign(new Error('پاسخ HTML به‌جای JSON — توکن JWT ارسال نشده یا منقضی شده'), {
          response: { status: 401, data: { message: 'احراز هویت ناموفق — دوباره وارد شوید' } },
        }),
      );
    }
    return response;
  },
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & {
      _retry?: boolean;
    };

    const status = error.response?.status;
    if (status === 302 || status === 301 || status === 307 || status === 308) {
      store.dispatch(logout());
      return Promise.reject(
        Object.assign(new Error('احراز هویت ناموفق — سرور API را به نسخه 2.8.9-dev به‌روز کنید'), {
          response: { status: 401, data: { message: 'احراز هویت ناموفق — دوباره وارد شوید' } },
        }),
      );
    }

    if (status !== 401 || originalRequest._retry) {
      return Promise.reject(error);
    }

    if (originalRequest.url?.includes('/auth/login')) {
      return Promise.reject(error);
    }

    const { refreshToken, accessToken } = store.getState().auth;

    if (!refreshToken || !accessToken) {
      store.dispatch(logout());
      return Promise.reject(error);
    }

    if (isRefreshing) {
      return new Promise<string>((resolve, reject) => {
        failedQueue.push({ resolve, reject });
      }).then((token) => {
        if (originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${token}`;
        }
        return api(originalRequest);
      });
    }

    originalRequest._retry = true;
    isRefreshing = true;

    try {
      const response = await axios.post('/api/auth/refresh', {
        accessToken,
        refreshToken,
      });

      const data = response.data?.data ?? response.data;
      const newAccessToken = data.accessToken as string;
      const newRefreshToken = data.refreshToken as string;
      const expiresAt = data.expiresAt as string;

      store.dispatch(
        setTokens({
          accessToken: newAccessToken,
          refreshToken: newRefreshToken,
          expiresAt,
        }),
      );

      processQueue(null, newAccessToken);

      if (originalRequest.headers) {
        originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
      }
      return api(originalRequest);
    } catch (refreshError) {
      processQueue(refreshError, null);
      store.dispatch(logout());
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  },
);

export default api;
