import { useEffect, useState } from 'react';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import Alert from '@mui/material/Alert';
import FormControlLabel from '@mui/material/FormControlLabel';
import Switch from '@mui/material/Switch';
import Stack from '@mui/material/Stack';
import SaveIcon from '@mui/icons-material/Save';
import SyncIcon from '@mui/icons-material/Sync';
import api from '../../services/api';
import LoadingOverlay from '../../components/common/LoadingOverlay';
import type { SettingDto, HolidayDto, AttendanceRecordDto } from '../../types';

function formatDateInput(date: Date) {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

export default function SettingsPage() {
  const [tab, setTab] = useState(0);
  const [settings, setSettings] = useState<SettingDto[]>([]);
  const [holidays, setHolidays] = useState<HolidayDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [syncing, setSyncing] = useState(false);
  const [success, setSuccess] = useState('');
  const [error, setError] = useState('');
  const [editedValues, setEditedValues] = useState<Record<string, string>>({});
  const [connectionOk, setConnectionOk] = useState(false);

  const today = new Date();
  const monthAgo = new Date();
  monthAgo.setDate(today.getDate() - 30);

  const [fromDate, setFromDate] = useState(formatDateInput(monthAgo));
  const [toDate, setToDate] = useState(formatDateInput(today));
  const [provinceCode, setProvinceCode] = useState('147');
  const [shamsiYearPrefix, setShamsiYearPrefix] = useState('1405');
  const [employeeLimit, setEmployeeLimit] = useState(0);
  const [applyProvinceFilter, setApplyProvinceFilter] = useState(true);
  const [applyShamsiYearFilter, setApplyShamsiYearFilter] = useState(false);
  const [diagnosticHints, setDiagnosticHints] = useState<string[]>([]);
  const [attendanceRecords, setAttendanceRecords] = useState<AttendanceRecordDto[]>([]);

  useEffect(() => {
    const load = async () => {
      try {
        const [settingsRes, holidaysRes, statusRes] = await Promise.all([
          api.get('/settings'),
          api.get('/settings/holidays'),
          api.get('/attendancesync/status'),
        ]);
        const settingsData = settingsRes.data?.data ?? settingsRes.data;
        const holidaysData = holidaysRes.data?.data ?? holidaysRes.data;
        const status = statusRes.data;

        if (Array.isArray(settingsData)) {
          setSettings(settingsData);
          const initial: Record<string, string> = {};
          settingsData.forEach((s: SettingDto) => {
            initial[s.key] = s.value;
          });
          setEditedValues(initial);
        }
        if (Array.isArray(holidaysData)) setHolidays(holidaysData);
        setConnectionOk(status?.connection?.isConnectionConfigured ?? false);
      } catch {
        /* empty */
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const handleSave = async (key: string) => {
    setSaving(true);
    setSuccess('');
    setError('');
    try {
      await api.put('/settings', { key, value: editedValues[key] });
      setSuccess('تنظیمات با موفقیت ذخیره شد');
    } catch {
      setError('خطا در ذخیره تنظیمات');
    } finally {
      setSaving(false);
    }
  };

  const loadAttendanceRecords = async () => {
    try {
      const res = await api.get('/attendancesync/records', {
        params: { fromDate, toDate },
      });
      const data = res.data?.data ?? res.data;
      if (Array.isArray(data)) setAttendanceRecords(data);
    } catch {
      setAttendanceRecords([]);
    }
  };

  const handlePreviewMis = async () => {
    setError('');
    setDiagnosticHints([]);
    try {
      const res = await api.get('/attendancesync/diagnostic', {
        params: {
          fromDate,
          toDate,
          provinceCode,
          shamsiYearPrefix,
          applyProvinceFilter,
          applyShamsiYearFilter,
          employeeLimit,
        },
      });
      const hints = res.data?.hints ?? [];
      setDiagnosticHints(Array.isArray(hints) ? hints : []);
      const count = res.data?.diagnostic?.countWithActiveFilters ?? 0;
      if (count === 0) {
        setError('با این فیلترها داده‌ای یافت نشد. راهنمای بالا را ببینید.');
      } else {
        setSuccess(`پیش‌نمایش: ${count} رکورد آماده دریافت است`);
      }
    } catch {
      setError('خطا در پیش‌نمایش MIS');
    }
  };

  const handleFetchMisData = async () => {
    setSyncing(true);
    setSuccess('');
    setError('');
    setDiagnosticHints([]);
    try {
      const res = await api.post('/attendancesync/run-range', {
        fromDate,
        toDate,
        provinceCode,
        shamsiYearPrefix,
        applyProvinceFilter,
        applyShamsiYearFilter,
        employeeLimit,
      });
      const result = res.data?.result;
      const processed = result?.recordsProcessed ?? 0;
      if (processed === 0) {
        setError('هیچ رکوردی دریافت نشد. فیلتر سال شمسی یا بازه تاریخ را بررسی کنید.');
      } else {
        setSuccess(
          res.data?.message ??
            `دریافت انجام شد: ${processed} رکورد، ${result?.recordsFailed ?? 0} خطا`
        );
      }
      await loadAttendanceRecords();
    } catch (err: unknown) {
      const message =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ??
        'خطا در دریافت داده از MIS';
      setError(message);
    } finally {
      setSyncing(false);
    }
  };

  const categories = [...new Set(settings.map((s) => s.category))];

  return (
    <Box>
      <LoadingOverlay open={loading || syncing} />
      <Typography variant="h5" sx={{ fontWeight: 700 }} gutterBottom>
        تنظیمات سیستم
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        پیکربندی پارامترهای سازمانی
      </Typography>

      {success && (
        <Alert severity="success" sx={{ mb: 2 }}>
          {success}
        </Alert>
      )}
      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 3 }}>
        <Tab label="تنظیمات عمومی" />
        <Tab label="دریافت از MIS" />
        <Tab label="تعطیلات رسمی" />
      </Tabs>

      {tab === 0 && (
        <Box>
          {categories.map((category) => (
            <Box key={category} sx={{ mb: 4 }}>
              <Typography variant="h6" sx={{ fontWeight: 600, mb: 2 }}>
                {category}
              </Typography>
              <TableContainer component={Paper} elevation={0}>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>عنوان</TableCell>
                      <TableCell>مقدار</TableCell>
                      <TableCell width={120}>عملیات</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {settings
                      .filter((s) => s.category === category)
                      .map((setting) => (
                        <TableRow key={setting.id}>
                          <TableCell>
                            <Typography variant="body2" sx={{ fontWeight: 500 }}>
                              {setting.description ?? setting.key}
                            </Typography>
                          </TableCell>
                          <TableCell>
                            <TextField
                              size="small"
                              fullWidth
                              value={editedValues[setting.key] ?? ''}
                              onChange={(e) =>
                                setEditedValues((prev) => ({
                                  ...prev,
                                  [setting.key]: e.target.value,
                                }))
                              }
                            />
                          </TableCell>
                          <TableCell>
                            <Button
                              size="small"
                              startIcon={<SaveIcon />}
                              onClick={() => handleSave(setting.key)}
                              disabled={saving}
                            >
                              ذخیره
                            </Button>
                          </TableCell>
                        </TableRow>
                      ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </Box>
          ))}
          {settings.length === 0 && !loading && (
            <Alert severity="info">تنظیماتی یافت نشد</Alert>
          )}
        </Box>
      )}

      {tab === 1 && (
        <Paper elevation={0} sx={{ p: 3 }}>
          {!connectionOk && (
            <Alert severity="warning" sx={{ mb: 2 }}>
              اتصال MIS در سرور پیکربندی نشده است. فقط مدیر فنی می‌تواند Server و Password را در appsettings تنظیم کند.
            </Alert>
          )}

          <Alert severity="info" sx={{ mb: 3 }}>
            بازه تاریخ را انتخاب کنید و سپس «دریافت داده» را بزنید. هیچ داده‌ای به‌صورت خودکار لود نمی‌شود.
          </Alert>

          <Stack spacing={2}>
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
              <TextField
                fullWidth
                type="date"
                label="از تاریخ"
                value={fromDate}
                onChange={(e) => setFromDate(e.target.value)}
                slotProps={{ inputLabel: { shrink: true } }}
              />
              <TextField
                fullWidth
                type="date"
                label="تا تاریخ"
                value={toDate}
                onChange={(e) => setToDate(e.target.value)}
                slotProps={{ inputLabel: { shrink: true } }}
              />
            </Stack>
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
              <TextField
                fullWidth
                label="سال شمسی"
                value={shamsiYearPrefix}
                onChange={(e) => setShamsiYearPrefix(e.target.value)}
              />
              <TextField
                fullWidth
                label="کد استان"
                value={provinceCode}
                onChange={(e) => setProvinceCode(e.target.value)}
              />
              <TextField
                fullWidth
                type="number"
                label="حداکثر تعداد پرسنل (۰ = همه)"
                value={employeeLimit}
                onChange={(e) => setEmployeeLimit(Number(e.target.value))}
              />
            </Stack>
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
              <FormControlLabel
                control={
                  <Switch
                    checked={applyProvinceFilter}
                    onChange={(e) => setApplyProvinceFilter(e.target.checked)}
                  />
                }
                label="فیلتر استان"
              />
              <FormControlLabel
                control={
                  <Switch
                    checked={applyShamsiYearFilter}
                    onChange={(e) => setApplyShamsiYearFilter(e.target.checked)}
                  />
                }
                label="فیلتر سال شمسی"
              />
            </Stack>
          </Stack>

          <Box sx={{ mt: 3, display: 'flex', gap: 2, flexWrap: 'wrap' }}>
            <Button variant="outlined" onClick={handlePreviewMis} disabled={syncing}>
              پیش‌نمایش فیلتر
            </Button>
            <Button
              variant="contained"
              size="large"
              startIcon={<SyncIcon />}
              onClick={handleFetchMisData}
              disabled={syncing || !fromDate || !toDate}
            >
              دریافت داده از MIS
            </Button>
            <Button variant="text" onClick={loadAttendanceRecords} disabled={syncing}>
              نمایش رکوردهای دریافت‌شده
            </Button>
          </Box>

          {diagnosticHints.length > 0 && (
            <Alert severity="info" sx={{ mt: 2 }}>
              {diagnosticHints.map((h) => (
                <div key={h}>{h}</div>
              ))}
            </Alert>
          )}

          {attendanceRecords.length > 0 && (
            <TableContainer component={Paper} elevation={0} sx={{ mt: 3 }}>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>کد پرسنلی</TableCell>
                    <TableCell>نام</TableCell>
                    <TableCell>تاریخ</TableCell>
                    <TableCell>ورود</TableCell>
                    <TableCell>خروج</TableCell>
                    <TableCell>نوع</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {attendanceRecords.map((r) => (
                    <TableRow key={r.id}>
                      <TableCell>{r.personnelCode}</TableCell>
                      <TableCell>{r.fullName}</TableCell>
                      <TableCell>{new Date(r.attendanceDate).toLocaleDateString('fa-IR')}</TableCell>
                      <TableCell>{r.entryTime ?? '—'}</TableCell>
                      <TableCell>{r.exitTime ?? '—'}</TableCell>
                      <TableCell>{r.leaveType ?? r.source}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </Paper>
      )}

      {tab === 2 && (
        <TableContainer component={Paper} elevation={0}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>عنوان</TableCell>
                <TableCell>تاریخ</TableCell>
                <TableCell>تکرار سالانه</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {holidays.map((holiday) => (
                <TableRow key={holiday.id}>
                  <TableCell>{holiday.title}</TableCell>
                  <TableCell>
                    {new Date(holiday.holidayDate).toLocaleDateString('fa-IR')}
                  </TableCell>
                  <TableCell>{holiday.isRecurring ? 'بله' : 'خیر'}</TableCell>
                </TableRow>
              ))}
              {holidays.length === 0 && !loading && (
                <TableRow>
                  <TableCell colSpan={3} align="center">
                    تعطیلی ثبت نشده است
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  );
}
