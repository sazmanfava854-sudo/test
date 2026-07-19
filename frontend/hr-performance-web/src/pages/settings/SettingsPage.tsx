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
import api from '../../services/api';
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
  const [success, setSuccess] = useState('');
  const [error, setError] = useState('');
  const [editedValues, setEditedValues] = useState<Record<string, string>>({});
  const [connectionOk, setConnectionOk] = useState(false);

  const defaultMisRange = useMemo(() => getDefaultMisSyncRange(), []);
  const [fromShamsi, setFromShamsi] = useState<ShamsiDateParts>(defaultMisRange.from);
  const [toShamsi, setToShamsi] = useState<ShamsiDateParts>(defaultMisRange.to);
  const [attendanceRecords, setAttendanceRecords] = useState<AttendanceRecordDto[]>([]);
  const [queryPreview, setQueryPreview] = useState('');
  const [gregorianRangeLabel, setGregorianRangeLabel] = useState('');

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

  const loadQueryPreview = async () => {
    try {
      const res = await api.get('/attendancesync/preview-query', { params: syncPayload });
      const data = res.data;
      setQueryPreview(data?.sql ?? data?.sqlWithLiteralValues ?? '');
      const gr = data?.gregorianRange;
      if (gr?.from && gr?.to) {
        setGregorianRangeLabel(`میلادی: ${gr.from} تا ${gr.to}`);
      }
    } catch (err: unknown) {
      const message =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ??
        'خطا در ساخت کوئری';
      setQueryPreview(`-- خطا: ${message}`);
      setGregorianRangeLabel('');
    }
  };

  useEffect(() => {
    if (tab === 1) {
      void loadQueryPreview();
    }
  }, [tab, syncPayload]);

  const loadAttendanceRecords = async () => {
    const res = await api.get('/attendancesync/records', {
      params: syncPayload,
    });
    const data = res.data?.data ?? res.data;
    if (Array.isArray(data)) setAttendanceRecords(data);
    else setAttendanceRecords([]);
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
      const preview = res.data?.queryPreview;
      if (preview?.sqlWithLiteralValues) setQueryPreview(preview.sqlWithLiteralValues);
      const gr = res.data?.gregorianRange;
      if (gr?.from && gr?.to) {
        setGregorianRangeLabel(`میلادی: ${gr.from} تا ${gr.to}`);
      }
      if (processed === 0) {
        let hintText = '';
        try {
          const diagRes = await api.get('/attendancesync/diagnostic', { params: syncPayload });
          const hints = diagRes.data?.hints;
          if (Array.isArray(hints) && hints.length > 0) {
            hintText = ` ${hints.join(' ')}`;
          }
        } catch {
          /* ignore diagnostic errors */
        }
        setError(
          `هیچ رکوردی دریافت نشد.${hintText} کوئری ساخته‌شده را در پایین بررسی کنید.`,
        );
      } else {
        setSuccess(
          res.data?.message ??
            `دریافت انجام شد: ${processed} رکورد، ${result?.recordsFailed ?? 0} خطا`,
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
          {!rangeIsValid && (
            <Alert severity="error" sx={{ mb: 2 }}>
              تاریخ پایان قبل از تاریخ شروع است.
            </Alert>
          )}

          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            تاریخ شمسی را انتخاب کنید → به میلادی تبدیل می‌شود → در MIS جستجو می‌شود.
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
                gregorianRangeLabel ||
                'برای جستجو در MIS به تاریخ میلادی تبدیل می‌شود — StartDate با ساعت هم پوشش داده می‌شود'
              }
            />

            <TextField
              fullWidth
              multiline
              minRows={10}
              label="کوئری SQL ساخته‌شده"
              value={queryPreview || '— دکمه «نمایش کوئری» را بزنید —'}
              slotProps={{ input: { readOnly: true, sx: { fontFamily: 'monospace', fontSize: 12 } } }}
            />

            <TextField
              fullWidth
              label="گروه پرسنلی"
              value={PERSONNEL_GROUP_CODE}
              slotProps={{ input: { readOnly: true } }}
              helperText="همیشه گروه 147 — ثابت سازمانی"
            />
          </Stack>

          <Box sx={{ mt: 3, display: 'flex', gap: 2, flexWrap: 'wrap' }}>
            <Button
              variant="outlined"
              size="large"
              startIcon={<CodeIcon />}
              onClick={() => void loadQueryPreview()}
              disabled={syncing || !rangeIsValid}
            >
              نمایش کوئری
            </Button>
            <Button
              variant="contained"
              size="large"
              startIcon={<SyncIcon />}
              onClick={handleFetchMisData}
              disabled={syncing || !rangeIsValid}
            >
              دریافت داده از MIS
            </Button>
          </Box>

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
