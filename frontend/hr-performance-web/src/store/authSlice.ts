import { createSlice, createAsyncThunk, type PayloadAction } from '@reduxjs/toolkit';
import type { UserDto, LoginRequest } from '../types';
import { authService } from '../services/authService';

interface AuthState {
  user: UserDto | null;
  accessToken: string | null;
  refreshToken: string | null;
  expiresAt: string | null;
  isAuthenticated: boolean;
  loading: boolean;
  error: string | null;
}

const loadStoredAuth = (): Partial<AuthState> => {
  try {
    const stored = localStorage.getItem('hr-auth');
    if (stored) {
      const parsed = JSON.parse(stored) as AuthState;
      return {
        user: parsed.user,
        accessToken: parsed.accessToken,
        refreshToken: parsed.refreshToken,
        expiresAt: parsed.expiresAt,
        isAuthenticated: !!parsed.accessToken,
      };
    }
  } catch {
    localStorage.removeItem('hr-auth');
  }
  return {};
};

const initialState: AuthState = {
  user: null,
  accessToken: null,
  refreshToken: null,
  expiresAt: null,
  isAuthenticated: false,
  loading: false,
  error: null,
  ...loadStoredAuth(),
};

const persistAuth = (state: AuthState) => {
  localStorage.setItem(
    'hr-auth',
    JSON.stringify({
      user: state.user,
      accessToken: state.accessToken,
      refreshToken: state.refreshToken,
      expiresAt: state.expiresAt,
    }),
  );
};

const normalizeLoginPayload = (raw: unknown) => {
  const envelope = (raw ?? {}) as Record<string, unknown>;
  const data = (envelope.data ?? envelope.Data ?? envelope) as Record<string, unknown>;
  const accessToken = String(data.accessToken ?? data.AccessToken ?? '');
  const refreshToken = String(data.refreshToken ?? data.RefreshToken ?? '');
  const expiresAt = String(data.expiresAt ?? data.ExpiresAt ?? '');
  const userRaw = (data.user ?? data.User ?? null) as Record<string, unknown> | null;
  if (!accessToken || !refreshToken) return null;
  return {
    accessToken,
    refreshToken,
    expiresAt,
    user: userRaw
      ? {
          id: String(userRaw.id ?? userRaw.Id ?? ''),
          userName: String(userRaw.userName ?? userRaw.UserName ?? ''),
          email: String(userRaw.email ?? userRaw.Email ?? ''),
          firstName: String(userRaw.firstName ?? userRaw.FirstName ?? ''),
          lastName: String(userRaw.lastName ?? userRaw.LastName ?? ''),
          fullName: String(userRaw.fullName ?? userRaw.FullName ?? ''),
          organizationId: (userRaw.organizationId ?? userRaw.OrganizationId) as string | undefined,
          employeeId: (userRaw.employeeId ?? userRaw.EmployeeId) as string | undefined,
          roles: (userRaw.roles ?? userRaw.Roles ?? []) as string[],
        }
      : null,
  };
};

export const login = createAsyncThunk(
  'auth/login',
  async (credentials: LoginRequest, { rejectWithValue }) => {
    try {
      const response = await authService.login(credentials);
      const success = Boolean(response.success ?? (response as { Success?: boolean }).Success);
      const message = response.message ?? (response as { Message?: string }).Message;
      const payload = normalizeLoginPayload(response.data ?? (response as { Data?: unknown }).Data ?? response);
      if (!success || !payload) {
        return rejectWithValue(message ?? 'خطا در ورود — accessToken در پاسخ سرور یافت نشد');
      }
      return payload;
    } catch (err: unknown) {
      const axiosErr = err as {
        response?: { status?: number; data?: { message?: string } };
        message?: string;
      };
      const serverMessage = axiosErr.response?.data?.message;
      if (serverMessage) return rejectWithValue(serverMessage);
      if (axiosErr.response?.status === 500 || axiosErr.response?.status === 503)
        return rejectWithValue('خطای سرور — اتصال SQL و فایل app\\appsettings.Development.json را بررسی کنید');
      const message = axiosErr.message ?? 'خطا در ارتباط با سرور';
      return rejectWithValue(message);
    }
  },
);

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    logout(state) {
      state.user = null;
      state.accessToken = null;
      state.refreshToken = null;
      state.expiresAt = null;
      state.isAuthenticated = false;
      state.error = null;
      localStorage.removeItem('hr-auth');
    },
    setTokens(
      state,
      action: PayloadAction<{
        accessToken: string;
        refreshToken: string;
        expiresAt: string;
      }>,
    ) {
      state.accessToken = action.payload.accessToken;
      state.refreshToken = action.payload.refreshToken;
      state.expiresAt = action.payload.expiresAt;
      state.isAuthenticated = true;
      persistAuth(state);
    },
    clearError(state) {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(login.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(login.fulfilled, (state, action) => {
        state.loading = false;
        if (action.payload.user) state.user = action.payload.user;
        state.accessToken = action.payload.accessToken;
        state.refreshToken = action.payload.refreshToken;
        state.expiresAt = action.payload.expiresAt;
        state.isAuthenticated = true;
        persistAuth(state);
      })
      .addCase(login.rejected, (state, action) => {
        state.loading = false;
        state.error = (action.payload as string) ?? 'خطا در ورود';
      });
  },
});

export const { logout, setTokens, clearError } = authSlice.actions;
export default authSlice.reducer;

export const selectUser = (state: { auth: AuthState }) => state.auth.user;
export const selectIsAuthenticated = (state: { auth: AuthState }) =>
  state.auth.isAuthenticated;
export const selectAuthLoading = (state: { auth: AuthState }) =>
  state.auth.loading;
export const selectAuthError = (state: { auth: AuthState }) => state.auth.error;
export const selectUserRoles = (state: { auth: AuthState }) =>
  state.auth.user?.roles ?? [];
