export const EmployeeStatus = {
  Active: 1,
  Inactive: 2,
  Suspended: 3,
  Terminated: 4,
} as const;

export type EmployeeStatus = (typeof EmployeeStatus)[keyof typeof EmployeeStatus];

export const ScoreType = {
  Positive: 1,
  Negative: 2,
} as const;

export type ScoreType = (typeof ScoreType)[keyof typeof ScoreType];

export const AppealStatus = {
  Pending: 1,
  Approved: 2,
  Rejected: 3,
} as const;

export type AppealStatus = (typeof AppealStatus)[keyof typeof AppealStatus];

export const WorkflowStatus = {
  Pending: 1,
  Approved: 2,
  Rejected: 3,
} as const;

export type WorkflowStatus = (typeof WorkflowStatus)[keyof typeof WorkflowStatus];

export const EmploymentType = {
  FullTime: 1,
  PartTime: 2,
  Contract: 3,
  Intern: 4,
} as const;

export type EmploymentType = (typeof EmploymentType)[keyof typeof EmploymentType];

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

export interface EmployeeLookupDto {
  id: string;
  personnelCode: string;
  fullName: string;
}

export interface EmployeeLookupRequest {
  query?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface LoginRequest {
  userName: string;
  password: string;
}

export interface RefreshTokenRequest {
  accessToken: string;
  refreshToken: string;
}

export interface UserDto {
  id: string;
  userName: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  organizationId?: string;
  employeeId?: string;
  roles: string[];
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserDto;
}

export interface EmployeeDto {
  id: string;
  personnelCode: string;
  nationalCode: string;
  firstName: string;
  lastName: string;
  fullName: string;
  position?: string;
  status: EmployeeStatus;
  currentScore: number;
  monthlyScore: number;
  yearlyScore: number;
  ranking?: number;
  departmentName?: string;
  managerName?: string;
  employmentDate: string;
  photoPath?: string;
}

export interface EmployeeSearchRequest {
  searchTerm?: string;
  departmentId?: string;
  status?: EmployeeStatus;
  pageNumber?: number;
  pageSize?: number;
}

export interface ScoreTrendDto {
  label: string;
  score: number;
}

export interface AttendanceSummaryDto {
  date: string;
  isPresent: boolean;
  delayMinutes: number;
  isAbsent: boolean;
}

export interface EmployeeDashboardDto {
  currentScore: number;
  monthlyScore: number;
  yearlyScore: number;
  ranking?: number;
  scoreTrend: ScoreTrendDto[];
  recentAttendance: AttendanceSummaryDto[];
  positiveCount: number;
  negativeCount: number;
}

export interface TopEmployeeDto {
  id: string;
  fullName: string;
  department?: string;
  score: number;
  ranking?: number;
}

export interface ChartDataDto {
  label: string;
  value: number;
}

export interface AttendanceRecordDto {
  id: string;
  employeeId: string;
  personnelCode: string;
  fullName: string;
  attendanceDate: string;
  entryTime?: string;
  exitTime?: string;
  workingHours?: number;
  delayMinutes?: number;
  isOnLeave: boolean;
  leaveType?: string;
  source: string;
}

export interface ManagerDashboardDto {
  employeeCount: number;
  todayPresent: number;
  todayDelays: number;
  todayAbsent: number;
  averageScore: number;
  topEmployees: TopEmployeeDto[];
  weakEmployees: TopEmployeeDto[];
  monthlyTrend: ChartDataDto[];
  teamIndicators: ChartDataDto[];
}

export interface DepartmentRankDto {
  id: string;
  name: string;
  averageScore: number;
  employeeCount: number;
}

export interface AdminDashboardDto {
  totalEmployees: number;
  totalManagers: number;
  totalDepartments: number;
  todayPresent: number;
  todayAbsent: number;
  averageScore: number;
  departmentRankings: DepartmentRankDto[];
  performanceDistribution: ChartDataDto[];
}

export interface EvaluationCategoryDto {
  id: string;
  name: string;
  description?: string;
  color?: string;
  icon?: string;
  weight: number;
  isActive: boolean;
  itemCount: number;
}

export interface EvaluationItemDto {
  id: string;
  categoryId: string;
  title: string;
  description?: string;
  scoreType: ScoreType;
  defaultScore: number;
  maxScore: number;
  minScore: number;
  weight: number;
  isActive: boolean;
}

export interface CreateEvaluationRequest {
  employeeId: string;
  categoryId?: string;
  itemId?: string;
  score: number;
  scoreType: ScoreType;
  notes?: string;
  evaluationDate: string;
}

export interface EmployeeEvaluationDto {
  id: string;
  employeeId: string;
  employeeName: string;
  score: number;
  scoreType: ScoreType;
  notes?: string;
  evaluationDate: string;
  workflowStatus: WorkflowStatus;
}

export interface EmployeeIndicatorDto {
  categoryId: string;
  categoryName: string;
  defaultWeight: number;
  weight: number;
  isActive: boolean;
}

export interface AppealDto {
  id: string;
  employeeId: string;
  employeeName: string;
  reason: string;
  status: AppealStatus;
  createdAt: string;
  reviewComments?: string;
}

export interface CreateAppealRequest {
  scoreId?: string;
  evaluationId?: string;
  reason: string;
}

export interface ReviewAppealRequest {
  appealId: string;
  status: AppealStatus;
  reviewComments?: string;
}

export interface SettingDto {
  id: string;
  key: string;
  value: string;
  category: string;
  description?: string;
  dataType: string;
}

export interface UpdateSettingRequest {
  key: string;
  value: string;
}

export interface HolidayDto {
  id: string;
  title: string;
  holidayDate: string;
  isRecurring: boolean;
  description?: string;
}

export interface MisSyncDateRangeRequest {
  shamsiFromYear: number;
  shamsiFromMonth: number;
  shamsiFromDay: number;
  shamsiToYear: number;
  shamsiToMonth: number;
  shamsiToDay: number;
  employeeLimit?: number;
}

export interface ReportSummaryDto {
  title: string;
  generatedAt: string;
  data: unknown;
}

export const EMPLOYEE_STATUS_LABELS: Record<EmployeeStatus, string> = {
  [EmployeeStatus.Active]: 'فعال',
  [EmployeeStatus.Inactive]: 'غیرفعال',
  [EmployeeStatus.Suspended]: 'تعلیق',
  [EmployeeStatus.Terminated]: 'خاتمه همکاری',
};

export const APPEAL_STATUS_LABELS: Record<AppealStatus, string> = {
  [AppealStatus.Pending]: 'در انتظار بررسی',
  [AppealStatus.Approved]: 'تأیید شده',
  [AppealStatus.Rejected]: 'رد شده',
};

export const SCORE_TYPE_LABELS: Record<ScoreType, string> = {
  [ScoreType.Positive]: 'مثبت',
  [ScoreType.Negative]: 'منفی',
};
