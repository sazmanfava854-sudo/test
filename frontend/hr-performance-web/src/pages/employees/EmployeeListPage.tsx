import { useEffect, useState, useCallback } from 'react';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import TextField from '@mui/material/TextField';
import InputAdornment from '@mui/material/InputAdornment';
import Chip from '@mui/material/Chip';
import { DataGrid, type GridColDef } from '@mui/x-data-grid';
import Alert from '@mui/material/Alert';
import SearchIcon from '@mui/icons-material/Search';
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
  const [search, setSearch] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [paginationModel, setPaginationModel] = useState({
    page: 0,
    pageSize: 20,
  });

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
        setError(
          response.data.totalCount === 0
            ? 'کارمندی ثبت نشده — از تنظیمات → MIS دکمه «دریافت فهرست پرسنل» را بزنید.'
            : null,
        );
      } else {
        setRows([]);
        setTotalCount(0);
        setError(response.message ?? 'خطا در دریافت فهرست کارمندان');
      }
    } catch {
      setRows([]);
      setTotalCount(0);
      setError('خطا در اتصال به API. اگر تازه به‌روزرسانی کرده‌اید، Ctrl+F5 بزنید و دوباره وارد شوید.');
    } finally {
      setLoading(false);
    }
  }, [search, paginationModel]);

  useEffect(() => {
    const timer = setTimeout(fetchEmployees, 300);
    return () => clearTimeout(timer);
  }, [fetchEmployees]);

  return (
    <Box>
      <LoadingOverlay open={loading && rows.length === 0} />
      {error && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 700 }}>
            فهرست کارمندان
          </Typography>
          <Typography variant="body2" color="text.secondary">
            مدیریت و جستجوی اطلاعات پرسنل
          </Typography>
        </Box>
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
            noRowsLabel: 'کارمندی یافت نشد',
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
