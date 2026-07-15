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
import SaveIcon from '@mui/icons-material/Save';
import api from '../../services/api';
import LoadingOverlay from '../../components/common/LoadingOverlay';
import type { SettingDto, HolidayDto } from '../../types';

export default function SettingsPage() {
  const [tab, setTab] = useState(0);
  const [settings, setSettings] = useState<SettingDto[]>([]);
  const [holidays, setHolidays] = useState<HolidayDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [success, setSuccess] = useState(false);
  const [editedValues, setEditedValues] = useState<Record<string, string>>({});

  useEffect(() => {
    const load = async () => {
      try {
        const [settingsRes, holidaysRes] = await Promise.all([
          api.get('/settings'),
          api.get('/settings/holidays'),
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
    setSuccess(false);
    try {
      await api.put('/settings', { key, value: editedValues[key] });
      setSuccess(true);
    } catch {
      /* error */
    } finally {
      setSaving(false);
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
          تنظیمات با موفقیت ذخیره شد
        </Alert>
      )}

      <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 3 }}>
        <Tab label="تنظیمات عمومی" />
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
