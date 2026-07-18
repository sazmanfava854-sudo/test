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
import MenuItem from '@mui/material/MenuItem';
import Stack from '@mui/material/Stack';
import SaveIcon from '@mui/icons-material/Save';
import SyncIcon from '@mui/icons-material/Sync';
import api from '../../services/api';
import LoadingOverlay from '../../components/common/LoadingOverlay';
import type {
  SettingDto,
  HolidayDto,
  HrIntegrationSettingsDto,
  UpdateHrIntegrationSettingsRequest,
} from '../../types';

const defaultHrSettings: UpdateHrIntegrationSettingsRequest = {
  syncMode: 'Monthly',
  shamsiYearPrefix: '1404',
  provinceCode: '147',
  applyProvinceFilter: true,
  applyShamsiYearFilter: true,
  initialSyncMonthsBack: 12,
  monthsPerSyncRun: 1,
  syncDaysBack: 30,
  employeeLimit: 10,
  backgroundSyncEnabled: false,
  syncIntervalMinutes: 5,
};

export default function SettingsPage() {
  const [tab, setTab] = useState(0);
  const [settings, setSettings] = useState<SettingDto[]>([]);
  const [holidays, setHolidays] = useState<HolidayDto[]>([]);
  const [hrSettings, setHrSettings] = useState<UpdateHrIntegrationSettingsRequest>(defaultHrSettings);
  const [hrMeta, setHrMeta] = useState<Pick<HrIntegrationSettingsDto, 'isConnectionConfigured' | 'lastSyncAt'>>({
    isConnectionConfigured: false,
  });
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [syncing, setSyncing] = useState(false);
  const [success, setSuccess] = useState('');
  const [editedValues, setEditedValues] = useState<Record<string, string>>({});

  useEffect(() => {
    const load = async () => {
      try {
        const [settingsRes, holidaysRes, hrRes] = await Promise.all([
          api.get('/settings'),
          api.get('/settings/holidays'),
          api.get('/hrintegration/settings'),
        ]);
        const settingsData = settingsRes.data?.data ?? settingsRes.data;
        const holidaysData = holidaysRes.data?.data ?? holidaysRes.data;
        const hrData: HrIntegrationSettingsDto | undefined = hrRes.data?.data ?? hrRes.data;

        if (Array.isArray(settingsData)) {
          setSettings(settingsData);
          const initial: Record<string, string> = {};
          settingsData.forEach((s: SettingDto) => {
            initial[s.key] = s.value;
          });
          setEditedValues(initial);
        }
        if (Array.isArray(holidaysData)) setHolidays(holidaysData);
        if (hrData) {
          setHrSettings({
            syncMode: hrData.syncMode,
            shamsiYearPrefix: hrData.shamsiYearPrefix,
            provinceCode: hrData.provinceCode,
            applyProvinceFilter: hrData.applyProvinceFilter,
            applyShamsiYearFilter: hrData.applyShamsiYearFilter,
            initialSyncMonthsBack: hrData.initialSyncMonthsBack,
            monthsPerSyncRun: hrData.monthsPerSyncRun,
            syncDaysBack: hrData.syncDaysBack,
            employeeLimit: hrData.employeeLimit,
            backgroundSyncEnabled: hrData.backgroundSyncEnabled,
            syncIntervalMinutes: hrData.syncIntervalMinutes,
          });
          setHrMeta({
            isConnectionConfigured: hrData.isConnectionConfigured,
            lastSyncAt: hrData.lastSyncAt,
          });
        }
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
    try {
      await api.put('/settings', { key, value: editedValues[key] });
      setSuccess('تنظیمات با موفقیت ذخیره شد');
    } catch {
      /* error */
    } finally {
      setSaving(false);
    }
  };

  const handleSaveHrSettings = async () => {
    setSaving(true);
    setSuccess('');
    try {
      const res = await api.put('/hrintegration/settings', hrSettings);
      const saved: HrIntegrationSettingsDto | undefined = res.data?.data ?? res.data;
      if (saved) {
        setHrMeta({
          isConnectionConfigured: saved.isConnectionConfigured,
          lastSyncAt: saved.lastSyncAt,
        });
      }
      setSuccess('تنظیمات سینک MIS ذخیره شد');
    } catch {
      setSuccess('');
    } finally {
      setSaving(false);
    }
  };

  const handleManualSync = async () => {
    setSyncing(true);
    setSuccess('');
    try {
      await api.post('/attendancesync/run');
      setSuccess('سینک دستی انجام شد');
    } catch {
      setSuccess('');
    } finally {
      setSyncing(false);
    }
  };

  const categories = [...new Set(settings.map((s) => s.category))];

  return (
    <Box>
      <LoadingOverlay open={loading} />
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

      <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 3 }}>
        <Tab label="تنظیمات عمومی" />
        <Tab label="سینک MIS" />
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
          {!hrMeta.isConnectionConfigured && (
            <Alert severity="warning" sx={{ mb: 2 }}>
              اتصال MIS در appsettings سرور پیکربندی نشده است. فقط مدیر فنی می‌تواند Server و Password را تنظیم کند.
            </Alert>
          )}

          <Alert severity="info" sx={{ mb: 3 }}>
            سینک خودکار هنگام اجرای برنامه انجام نمی‌شود مگر اینکه «سینک خودکار» را فعال کنید.
            برای تست، تعداد پرسنل را ۱۰ بگذارید و با دکمه «سینک دستی» داده بگیرید.
          </Alert>

          <Stack spacing={2}>
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
              <TextField
                select
                fullWidth
                label="حالت سینک"
                value={hrSettings.syncMode}
                onChange={(e) => setHrSettings((prev) => ({ ...prev, syncMode: e.target.value }))}
              >
                <MenuItem value="Monthly">ماهانه</MenuItem>
                <MenuItem value="DaysBack">بر اساس روز</MenuItem>
              </TextField>
              <TextField
                fullWidth
                label="سال شمسی"
                value={hrSettings.shamsiYearPrefix}
                onChange={(e) => setHrSettings((prev) => ({ ...prev, shamsiYearPrefix: e.target.value }))}
              />
              <TextField
                fullWidth
                label="کد استان"
                value={hrSettings.provinceCode}
                onChange={(e) => setHrSettings((prev) => ({ ...prev, provinceCode: e.target.value }))}
              />
            </Stack>
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
              <TextField
                fullWidth
                type="number"
                label="تعداد ماه بک‌فیل"
                value={hrSettings.initialSyncMonthsBack}
                onChange={(e) =>
                  setHrSettings((prev) => ({
                    ...prev,
                    initialSyncMonthsBack: Number(e.target.value),
                  }))
                }
              />
              <TextField
                fullWidth
                type="number"
                label="تعداد ماه در هر اجرا"
                value={hrSettings.monthsPerSyncRun}
                onChange={(e) =>
                  setHrSettings((prev) => ({
                    ...prev,
                    monthsPerSyncRun: Number(e.target.value),
                  }))
                }
              />
              <TextField
                fullWidth
                type="number"
                label="محدودیت تعداد پرسنل (۰ = بدون محدودیت)"
                value={hrSettings.employeeLimit}
                onChange={(e) =>
                  setHrSettings((prev) => ({
                    ...prev,
                    employeeLimit: Number(e.target.value),
                  }))
                }
              />
            </Stack>
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
              <TextField
                fullWidth
                type="number"
                label="روزهای گذشته (حالت DaysBack)"
                value={hrSettings.syncDaysBack}
                onChange={(e) =>
                  setHrSettings((prev) => ({
                    ...prev,
                    syncDaysBack: Number(e.target.value),
                  }))
                }
              />
              <TextField
                fullWidth
                type="number"
                label="فاصله سینک خودکار (دقیقه)"
                value={hrSettings.syncIntervalMinutes}
                onChange={(e) =>
                  setHrSettings((prev) => ({
                    ...prev,
                    syncIntervalMinutes: Number(e.target.value),
                  }))
                }
              />
            </Stack>
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
              <FormControlLabel
                control={
                  <Switch
                    checked={hrSettings.applyProvinceFilter}
                    onChange={(e) =>
                      setHrSettings((prev) => ({
                        ...prev,
                        applyProvinceFilter: e.target.checked,
                      }))
                    }
                  />
                }
                label="فیلتر استان"
              />
              <FormControlLabel
                control={
                  <Switch
                    checked={hrSettings.applyShamsiYearFilter}
                    onChange={(e) =>
                      setHrSettings((prev) => ({
                        ...prev,
                        applyShamsiYearFilter: e.target.checked,
                      }))
                    }
                  />
                }
                label="فیلتر سال شمسی"
              />
              <FormControlLabel
                control={
                  <Switch
                    checked={hrSettings.backgroundSyncEnabled}
                    onChange={(e) =>
                      setHrSettings((prev) => ({
                        ...prev,
                        backgroundSyncEnabled: e.target.checked,
                      }))
                    }
                  />
                }
                label="سینک خودکار پس‌زمینه"
              />
            </Stack>
          </Stack>

          {hrMeta.lastSyncAt && (
            <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
              آخرین سینک: {new Date(hrMeta.lastSyncAt).toLocaleString('fa-IR')}
            </Typography>
          )}

          <Box sx={{ mt: 3, display: 'flex', gap: 2 }}>
            <Button
              variant="contained"
              startIcon={<SaveIcon />}
              onClick={handleSaveHrSettings}
              disabled={saving}
            >
              ذخیره تنظیمات سینک
            </Button>
            <Button
              variant="outlined"
              startIcon={<SyncIcon />}
              onClick={handleManualSync}
              disabled={syncing}
            >
              سینک دستی (یک بازه)
            </Button>
          </Box>
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
