import { useEffect, useState, useCallback } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import TextField from '@mui/material/TextField';
import InputAdornment from '@mui/material/InputAdornment';
import Button from '@mui/material/Button';
import Chip from '@mui/material/Chip';
import Link from '@mui/material/Link';
import { DataGrid, type GridColDef } from '@mui/x-data-grid';
import Alert from '@mui/material/Alert';
import SearchIcon from '@mui/icons-material/Search';
import PeopleIcon from '@mui/icons-material/People';
import SyncIcon from '@mui/icons-material/Sync';
import { employeeService } from '../../services/employeeService';
import LoadingOverlay from '../../components/common/LoadingOverlay';
import {
  type EmployeeDto,
  EmployeeStatus,
  EMPLOYEE_STATUS_LABELS,
} from '../../types';

const statusColor: Record<EmployeeStatus, 'success' | 'default' | 'warning' | 'error'> = {
  [EmployeeStatus.Active]: 'success',
  [EmployeeStatus.Inactive]: 'default',
  [EmployeeStatus.Suspended]: 'warning',
  [EmployeeStatus.Terminated]: 'error',
};

const columns: GridColDef<EmployeeDto>[] = [
  { field: 'personnelCode', headerName: 'کد پرسنلی', width: 120 },
  { field: 'fullName', headerName: 'نام کامل', flex: 1, minWidth: 180 },
  { field: 'position', headerName: 'سمت', width: 140 },
  { field: 'departmentName', headerName: 'واحد', width: 140 },
  {
    field: 'currentScore',
    headerName: 'امتیاز',
    width: 90,
    type: 'number',
    valueFormatter: (value: number) => value?.toFixed(1),
  },
  {
    field: 'ranking',
    headerName: 'رتبه',
    width: 80,
    type: 'number',
  },
  {
    field: 'status',
    headerName: 'وضعیت',
    width: 130,
    renderCell: (params) => (
      <Chip
        label={EMPLOYEE_STATUS_LABELS[params.value as EmployeeStatus]}
        color={statusColor[params.value as EmployeeStatus]}
        size="small"
      />
    ),
  },
];

export default function EmployeeListPage() {
  const [rows, setRows] = useState<EmployeeDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [syncing, setSyncing] = useState(false);
  const [search, setSearch] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);
  const [paginationModel, setPaginationModel] = useState({
    page: 0,
    pageSize: 20,
  });

  const loadSummary = useCallback(async () => {
    try {
      const summary = await employeeService.getSummary();
      if (summary.employeesInDatabase != null && summary.lastEmployeeRosterSyncAt) {
        setInfo(
          `${summary.employeesInDatabase} کارمند در پایگاه — آخرین دریافت فهرست: ${new Date(summary.lastEmployeeRosterSyncAt).toLocaleString('fa-IR')}`,
        );
      } else if (summary.employeesInDatabase != null) {
        setInfo(`${summary.employeesInDatabase} کارمند در پایگاه`);
      }
    } catch {
      /* optional */
    }
  }, []);

  const fetchEmployees = useCallback(async () => {
    setLoading(true);
    try {
      const response = await employeeService.getAll({
        searchTerm: search || undefined,
        pageNumber: paginationModel.page + 1,
        pageSize: paginationModel.pageSize,
      });
      if (response.success && response.data) {
        setRows(response.data.items);
        setTotalCount(response.data.totalCount);
        if (response.data.totalCount === 0) {
          setError(
            'کارمندی ثبت نشده. دکمه «دریافت فهرست پرسنل از MIS» را بزنید (ProvinceCode=147).',
          );
        } else {
          setError(null);
        }
      } else {
        setRows([]);
        setTotalCount(0);
        setError(response.message ?? 'خطا در دریافت فهرست کارمندان');
      }
    } catch {
      setRows([]);
      setTotalCount(0);
      setError('خطا در اتصال به API — Ctrl+F5 و دوباره login کنید.');
    } finally {
      setLoading(false);
    }
  }, [search, paginationModel]);

  const handleSyncRoster = async () => {
    setSyncing(true);
    setError(null);
    try {
      const res = await employeeService.syncRosterFromMis();
      if (res.success === false) {
        setError(res.message ?? 'خطا در دریافت فهرست پرسنل');
        return;
      }
      setInfo(
        res.message ??
          `دریافت شد: ${res.inserted ?? 0} جدید، ${res.updated ?? 0} به‌روز، ${res.total ?? 0} از MIS`,
      );
      await fetchEmployees();
      await loadSummary();
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ??
        'خطا در دریافت فهرست پرسنل — API را restart کنید (migration خودکار در startup)';
      setError(msg);
    } finally {
      setSyncing(false);
    }
  };

  useEffect(() => {
    void loadSummary();
  }, [loadSummary]);

  useEffect(() => {
    const timer = setTimeout(fetchEmployees, 300);
    return () => clearTimeout(timer);
  }, [fetchEmployees]);

  return (
    <Box>
      <LoadingOverlay open={(loading && rows.length === 0) || syncing} />
      {error && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          {error}{' '}
          <Link component={RouterLink} to="/settings?tab=mis">
            تنظیمات MIS
          </Link>
        </Alert>
      )}
      {info && !error && (
        <Alert severity="info" sx={{ mb: 2 }}>
          {info}
        </Alert>
      )}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3, flexWrap: 'wrap', gap: 2 }}>
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 700 }}>
            فهرست کارمندان
          </Typography>
          <Typography variant="body2" color="text.secondary">
            مدیریت و جستجوی اطلاعات پرسنل
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
          <Button
            variant="contained"
            startIcon={syncing ? <SyncIcon /> : <PeopleIcon />}
            onClick={() => void handleSyncRoster()}
            disabled={syncing}
          >
            {syncing ? 'در حال دریافت...' : 'دریافت فهرست پرسنل از MIS'}
          </Button>
          <TextField
            size="small"
            placeholder="جستجو..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            sx={{ width: 280 }}
            slotProps={{
              input: {
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon color="action" />
                  </InputAdornment>
                ),
              },
            }}
          />
        </Box>
      </Box>

      <Box sx={{ height: 600, width: '100%' }}>
        <DataGrid
          rows={rows}
          columns={columns}
          getRowId={(row) => row.id}
          rowCount={totalCount}
          loading={loading}
          paginationMode="server"
          paginationModel={paginationModel}
          onPaginationModelChange={setPaginationModel}
          pageSizeOptions={[10, 20, 50]}
          disableRowSelectionOnClick
          localeText={{
            noRowsLabel: 'کارمندی یافت نشد — «دریافت فهرست پرسنل از MIS» را بزنید',
            footerRowSelected: (count) => `${count} ردیف انتخاب شده`,
          }}
          sx={{
            border: 'none',
            '& .MuiDataGrid-cell:focus': { outline: 'none' },
          }}
        />
      </Box>
    </Box>
  );
}
