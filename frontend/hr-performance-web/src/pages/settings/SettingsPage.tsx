import { useEffect, useMemo, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
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
import Stack from '@mui/material/Stack';
import SaveIcon from '@mui/icons-material/Save';
import SyncIcon from '@mui/icons-material/Sync';
import CodeIcon from '@mui/icons-material/Code';
import PeopleIcon from '@mui/icons-material/People';
import api from '../../services/api';
import { employeeService } from '../../services/employeeService';
import LoadingOverlay from '../../components/common/LoadingOverlay';
import ShamsiDatePicker from '../../components/common/ShamsiDatePicker';
import type { SettingDto, HolidayDto, AttendanceRecordDto } from '../../types';
import {
  formatGregorianDate,
  formatShamsiDate,
  formatShamsiParts,
  getDefaultMisSyncRange,
  isShamsiRangeValid,
  toMisSyncRequestPayload,
  type ShamsiDateParts,
} from '../../utils/misDate';

const PERSONNEL_GROUP_CODE = '147';
const APP_VERSION = '2.9.5-dev';

interface MisConnectionStatus {
  isConnectionConfigured?: boolean;
  missingFields?: string[];
  server?: string;
  database?: string;
  userId?: string;
  passwordIsPlaceholder?: boolean;
}

function parseMisConnection(raw: unknown): MisConnectionStatus | null {
  if (!raw || typeof raw !== 'object') return null;
  const data = raw as Record<string, unknown>;
  return {
    isConnectionConfigured: Boolean(data.isConnectionConfigured ?? data.IsConnectionConfigured),
    missingFields: (data.missingFields ?? data.MissingFields) as string[] | undefined,
    server: String(data.server ?? data.Server ?? ''),
    database: String(data.database ?? data.Database ?? 'MIS'),
    userId: String(data.userId ?? data.UserId ?? ''),
    passwordIsPlaceholder: Boolean(data.passwordIsPlaceholder ?? data.PasswordIsPlaceholder),
  };
}

export default function SettingsPage() {
  const [searchParams] = useSearchParams();
  const initialTab = searchParams.get('tab') === 'mis' ? 1 : 0;
  const [tab, setTab] = useState(initialTab);
  const [settings, setSettings] = useState<SettingDto[]>([]);
  const [holidays, setHolidays] = useState<HolidayDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [syncing, setSyncing] = useState(false);
  const [rosterSyncing, setRosterSyncing] = useState(false);
  const [success, setSuccess] = useState('');
  const [error, setError] = useState('');
  const [editedValues, setEditedValues] = useState<Record<string, string>>({});
  const [connectionOk, setConnectionOk] = useState(false);

  const defaultMisRange = useMemo(() => getDefaultMisSyncRange(), []);
  const [fromShamsi, setFromShamsi] = useState<ShamsiDateParts>(defaultMisRange.from);
  const [toShamsi, setToShamsi] = useState<ShamsiDateParts>(defaultMisRange.to);
  const [attendanceRecords, setAttendanceRecords] = useState<AttendanceRecordDto[]>([]);
  const [queryPreview, setQueryPreview] = useState('');
  const [queryPreviewLoading, setQueryPreviewLoading] = useState(false);
  const [shamsiFilterLabel, setShamsiFilterLabel] = useState('');
  const [misLiveHint, setMisLiveHint] = useState('');
  const [misLiveRowCount, setMisLiveRowCount] = useState<number | null>(null);

  const syncPayload = useMemo(
    () => toMisSyncRequestPayload(fromShamsi, toShamsi),
    [fromShamsi, toShamsi],
  );

  const rangeIsValid = useMemo(
    () => isShamsiRangeValid(fromShamsi, toShamsi),
    [fromShamsi, toShamsi],
  );

  useEffect(() => {
    if (searchParams.get('tab') === 'mis') setTab(1);
  }, [searchParams]);

  useEffect(() => {
    const load = async () => {
      try {
        const [settingsRes, holidaysRes, misHealthRes, statusRes] = await Promise.all([
          api.get('/settings'),
          api.get('/settings/holidays'),
          api.get('/health/mis'),
          api.get('/attendancesync/status').catch(() => ({ data: null })),
        ]);
        const settingsData = settingsRes.data?.data ?? settingsRes.data;
        const holidaysData = holidaysRes.data?.data ?? holidaysRes.data;

        if (Array.isArray(settingsData)) {
          setSettings(settingsData);
          const initial: Record<string, string> = {};
          settingsData.forEach((s: SettingDto) => {
            initial[s.key] = s.value;
          });
          setEditedValues(initial);
        }
        if (Array.isArray(holidaysData)) setHolidays(holidaysData);

        const conn =
          parseMisConnection(misHealthRes.data) ??
          parseMisConnection((statusRes.data as { connection?: unknown } | null)?.connection);
        setConnectionOk(conn?.isConnectionConfigured ?? false);
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

  const loadMisLiveTest = async () => {
    try {
      const res = await api.get('/health/mis-live-test', { params: syncPayload });
      const data = res.data;
      const rowCount = typeof data?.rowCount === 'number' ? data.rowCount : null;
      const hint = String(data?.hint ?? data?.error ?? '');
      setMisLiveRowCount(rowCount);
      setMisLiveHint(hint);
      if (data?.isConnectionConfigured === false) setConnectionOk(false);
      else if (data?.canConnect) setConnectionOk(true);
      return { rowCount, hint };
    } catch {
      const hint = 'تست اتصال MIS ناموفق — سرور API در حال اجرا است؟';
      setMisLiveHint(hint);
      setMisLiveRowCount(null);
      return { rowCount: null, hint };
    }
  };

  const extractApiError = (err: unknown): string => {
    const ax = err as {
      response?: { status?: number; data?: { message?: string; title?: string } };
      message?: string;
      code?: string;
    };
    const status = ax.response?.status;
    const msg =
      ax.response?.data?.message ??
      ax.response?.data?.title ??
      ax.message ??
      'خطای نامشخص';
    if (status) return `[HTTP ${status}] ${msg}`;
    if (ax.code === 'ERR_NETWORK') {
      return 'اتصال به API برقرار نشد — start-local.bat را اجرا کنید یا پورت API را بررسی کنید';
    }
    return msg;
  };

  const loadQueryPreview = async () => {
    setQueryPreviewLoading(true);
    setError('');
    try {
      let res;
      let lastError = '';
      try {
        res = await api.get('/health/mis-preview-query', { params: syncPayload });
      } catch (errHealth) {
        lastError = extractApiError(errHealth);
        try {
          res = await api.get('/attendancesync/preview-query', { params: syncPayload });
        } catch (errAuth) {
          throw new Error(`${lastError} | fallback: ${extractApiError(errAuth)}`);
        }
      }

      const data = res.data;
      if (data?.success === false) {
        throw new Error(String(data.message ?? 'پیش‌نمایش کوئری ناموفق بود'));
      }

      const sql = String(data?.sql ?? data?.sqlWithLiteralValues ?? '').trim();
      if (!sql) {
        const msg = 'کوئری خالی برگشت — پارامترهای تاریخ را بررسی کنید';
        setQueryPreview(`-- خطا: ${msg}`);
        setError(msg);
        return;
      }
      setQueryPreview(sql);
      if (data?.shamsiRange) {
        setShamsiFilterLabel(`فیلتر ShamsiDate: ${data.shamsiRange}`);
      } else if (data?.shamsiFromKey && data?.shamsiToKey) {
        setShamsiFilterLabel(`فیلتر ShamsiDate: ${data.shamsiFromKey} تا ${data.shamsiToKey}`);
      }
      setSuccess(`کوئری ساخته شد (API ${data?.apiVersion ?? '?'})`);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : extractApiError(err);
      setQueryPreview(`-- خطا: ${message}`);
      setError(message);
      setShamsiFilterLabel('');
    } finally {
      setQueryPreviewLoading(false);
    }
  };

  useEffect(() => {
    if (tab === 1) {
      void loadQueryPreview();
      void loadMisLiveTest();
    }
  }, [tab, syncPayload]);

  const loadAttendanceRecords = async () => {
    const res = await api.get('/attendancesync/records', {
      params: syncPayload,
    });
    const body = res.data;
    if (body?.success === false) {
      setError(body.message ?? 'خطا در بارگذاری رکوردها — شناسه سازمان را بررسی کنید');
      setAttendanceRecords([]);
      return;
    }
    const data = body?.data ?? body;
    if (Array.isArray(data)) setAttendanceRecords(data);
    else setAttendanceRecords([]);
  };

  const handleSyncRoster = async () => {
    setRosterSyncing(true);
    setSuccess('');
    setError('');
    try {
      const res = await employeeService.syncRosterFromMis();
      if (res.success === false) {
        setError(res.message ?? 'خطا در دریافت فهرست پرسنل');
        return;
      }
      setSuccess(
        res.message ??
          `فهرست پرسنل: ${res.inserted ?? 0} جدید، ${res.updated ?? 0} به‌روز، ${res.total ?? 0} از MIS (ProvinceCode ${PERSONNEL_GROUP_CODE})`,
      );
    } catch (err: unknown) {
      const message =
        (err as { response?: { data?: { message?: string } } })?.response?.data
          ?.message ?? 'خطا در دریافت فهرست پرسنل از MIS';
      setError(message);
    } finally {
      setRosterSyncing(false);
    }
  };

  const handleFetchMisData = async () => {
    if (!rangeIsValid) {
      setError(
        `تاریخ پایان (${formatShamsiParts(toShamsi)}) نمی‌تواند قبل از تاریخ شروع (${formatShamsiParts(fromShamsi)}) باشد.`,
      );
      return;
    }

    setSyncing(true);
    setSuccess('');
    setError('');
    try {
      const res = await api.post('/attendancesync/run-range', syncPayload);
      const result = res.data?.result;
      const processed = result?.recordsProcessed ?? 0;
      const misFetched = res.data?.misRowsFetched ?? result?.misRowsFetched ?? 0;
      const employeesUpserted = res.data?.employeesUpserted ?? result?.employeesUpserted ?? 0;
      const distinctEmployees = res.data?.distinctEmployeesInMis ?? result?.distinctEmployeesInMis ?? 0;
      const preview = res.data?.queryPreview;
      if (preview?.sqlWithLiteralValues) setQueryPreview(preview.sqlWithLiteralValues);
      const sr = res.data?.shamsiRange;
      if (sr?.from && sr?.to) {
        setShamsiFilterLabel(`فیلتر ShamsiDate: ${sr.from} تا ${sr.to}`);
      }
      const live = await loadMisLiveTest();
      if (processed === 0) {
        let hintText = '';
        if (misFetched > 0) {
          hintText = ` MIS ${misFetched} رکورد برگرداند ولی ذخیره نشد — لاگ سرور را ببینید.`;
        } else {
          try {
            const diagRes = await api.get('/attendancesync/diagnostic', { params: syncPayload });
            const hints = diagRes.data?.hints;
            if (Array.isArray(hints) && hints.length > 0) {
              hintText = ` ${hints.join(' ')}`;
            }
          } catch {
            /* ignore */
          }
          if (live.hint) hintText += ` ${live.hint}`;
        }
        setError(
          `هیچ رکوردی دریافت نشد.${hintText} کوئری را در پایین بررسی کنید یا بازه تاریخ را عوض کنید.`,
        );
      } else {
        setSuccess(
          res.data?.message ??
            `دریافت انجام شد: ${processed} رکورد، ${distinctEmployees} پرسنل MIS، ${employeesUpserted} کارمند جدید، ${result?.recordsFailed ?? 0} خطا`,
        );
      }
      await loadAttendanceRecords();
    } catch (err: unknown) {
      const message =
        (err as { response?: { data?: { message?: string } } })?.response?.data
          ?.message ?? 'خطا در دریافت داده از MIS';
      setError(message);
      setAttendanceRecords([]);
    } finally {
      setSyncing(false);
    }
  };

  const categories = [...new Set(settings.map((s) => s.category))];

  return (
    <Box>
      <LoadingOverlay open={loading || syncing || rosterSyncing} />
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
          <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 1 }}>
            نسخه UI: {APP_VERSION}
          </Typography>

          <Alert severity="info" sx={{ mb: 2 }}>
            ابتدا «دریافت فهرست پرسنل» را بزنید تا dropdown کارمندان در فرم ارزیابی پر شود (فیلتر
            ProvinceCode={PERSONNEL_GROUP_CODE}). سپس برای حضور/مرخصی «دریافت داده از MIS» را با
            بازه تاریخ اجرا کنید.
          </Alert>

          <Box sx={{ mb: 2, display: 'flex', gap: 2, flexWrap: 'wrap' }}>
            <Button
              variant="contained"
              color="primary"
              size="large"
              startIcon={<PeopleIcon />}
              onClick={() => void handleSyncRoster()}
              disabled={rosterSyncing || syncing || !connectionOk}
            >
              {rosterSyncing ? 'در حال دریافت فهرست...' : 'دریافت فهرست پرسنل'}
            </Button>
            <Button
              variant="outlined"
              color="secondary"
              size="large"
              startIcon={<CodeIcon />}
              onClick={() => void loadQueryPreview()}
              disabled={syncing || rosterSyncing || queryPreviewLoading || !rangeIsValid}
            >
              {queryPreviewLoading ? 'در حال ساخت کوئری...' : 'نمایش کوئری SQL'}
            </Button>
            <Button
              variant="contained"
              size="large"
              startIcon={<SyncIcon />}
              onClick={handleFetchMisData}
              disabled={syncing || rosterSyncing || !rangeIsValid}
            >
              دریافت داده از MIS
            </Button>
          </Box>

          <TextField
            fullWidth
            multiline
            minRows={12}
            label="کوئری SQL ساخته‌شده"
            value={queryPreview || '— دکمه «نمایش کوئری SQL» را بزنید —'}
            slotProps={{
              input: {
                readOnly: true,
                sx: { fontFamily: 'monospace', fontSize: 12, direction: 'ltr', textAlign: 'left' },
              },
            }}
            sx={{ mb: 2 }}
          />

          {!rangeIsValid && (
            <Alert severity="error" sx={{ mb: 2 }}>
              تاریخ پایان قبل از تاریخ شروع است.
            </Alert>
          )}

          {misLiveHint && (
            <Alert severity={misLiveRowCount && misLiveRowCount > 0 ? 'success' : connectionOk ? 'warning' : 'error'} sx={{ mb: 2 }}>
              {misLiveHint}
              {misLiveRowCount !== null && ` (تعداد در MIS: ${misLiveRowCount})`}
            </Alert>
          )}

          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            تاریخ شمسی را انتخاب کنید — فیلتر مستقیم روی ستون ShamsiDate در MIS (مثل 1404/04/10).
            گروه پرسنلی: {PERSONNEL_GROUP_CODE}
            {!connectionOk && ' — Password MIS را در appsettings.Development.json بررسی کنید.'}
          </Typography>

          <Stack spacing={2}>
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
              <ShamsiDatePicker
                label="از تاریخ (شمسی)"
                value={fromShamsi}
                onChange={setFromShamsi}
                disabled={syncing}
              />
              <ShamsiDatePicker
                label="تا تاریخ (شمسی)"
                value={toShamsi}
                onChange={setToShamsi}
                disabled={syncing}
              />
            </Stack>

            <TextField
              fullWidth
              label="بازه انتخاب‌شده"
              value={`${formatShamsiParts(fromShamsi)} تا ${formatShamsiParts(toShamsi)}`}
              slotProps={{ input: { readOnly: true } }}
              helperText={
                shamsiFilterLabel ||
                'فیلتر MIS روی ستون ShamsiDate (مثل 1404/04/10) — بدون تبدیل میلادی'
              }
            />

            <TextField
              fullWidth
              label="گروه پرسنلی"
              value={PERSONNEL_GROUP_CODE}
              slotProps={{ input: { readOnly: true } }}
              helperText="همیشه گروه 147 — ثابت سازمانی"
            />
          </Stack>

          {attendanceRecords.length > 0 && (
            <TableContainer component={Paper} elevation={0} sx={{ mt: 3 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 600, p: 2, pb: 0 }}>
                رکوردهای دریافت‌شده ({attendanceRecords.length})
              </Typography>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>کد پرسنلی</TableCell>
                    <TableCell>نام</TableCell>
                    <TableCell>تاریخ (شمسی)</TableCell>
                    <TableCell>ورود</TableCell>
                    <TableCell>خروج</TableCell>
                    <TableCell>تاخیر (دقیقه)</TableCell>
                    <TableCell>نوع</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {attendanceRecords.map((r) => (
                    <TableRow key={r.id}>
                      <TableCell>{r.personnelCode}</TableCell>
                      <TableCell>{r.fullName}</TableCell>
                      <TableCell>{formatShamsiDate(r.attendanceDate)}</TableCell>
                      <TableCell>{r.entryTime ?? '—'}</TableCell>
                      <TableCell>{r.exitTime ?? '—'}</TableCell>
                      <TableCell>
                        {(r.delayMinutes ?? 0) > 0 ? (
                          <Typography component="span" color="warning.main" sx={{ fontWeight: 600 }}>
                            {r.delayMinutes}
                          </Typography>
                        ) : (
                          '—'
                        )}
                      </TableCell>
                      <TableCell>{r.leaveType ?? r.source}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}

          {!syncing && attendanceRecords.length === 0 && success && (
            <Alert severity="info" sx={{ mt: 2 }}>
              دریافت انجام شد اما رکوردی برای نمایش در این بازه وجود ندارد.
            </Alert>
          )}
        </Paper>
      )}

      {tab === 2 && (
        <TableContainer component={Paper} elevation={0}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>عنوان</TableCell>
                <TableCell>تاریخ (میلادی)</TableCell>
                <TableCell>تکرار سالانه</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {holidays.map((holiday) => (
                <TableRow key={holiday.id}>
                  <TableCell>{holiday.title}</TableCell>
                  <TableCell>{formatGregorianDate(holiday.holidayDate)}</TableCell>
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
