import { useCallback, useEffect, useState } from 'react';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import MenuItem from '@mui/material/MenuItem';
import Grid from '@mui/material/Grid';
import Alert from '@mui/material/Alert';
import Autocomplete from '@mui/material/Autocomplete';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import Switch from '@mui/material/Switch';
import FormControlLabel from '@mui/material/FormControlLabel';
import SaveIcon from '@mui/icons-material/Save';
import TuneIcon from '@mui/icons-material/Tune';
import { Link as RouterLink } from 'react-router-dom';
import Link from '@mui/material/Link';
import api from '../../services/api';
import { employeeService } from '../../services/employeeService';
import LoadingOverlay from '../../components/common/LoadingOverlay';
import { glassCardSx } from '../../theme/theme';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import { useTheme } from '@mui/material/styles';
import {
  type EmployeeLookupDto,
  type EvaluationCategoryDto,
  type CreateEvaluationRequest,
  type EmployeeIndicatorDto,
  ScoreType,
  SCORE_TYPE_LABELS,
} from '../../types';
import { unwrapListItems, readApiMessage } from '../../utils/apiResponse';

export default function EvaluationPage() {
  const theme = useTheme();
  const [tab, setTab] = useState(0);
  const [employeeOptions, setEmployeeOptions] = useState<EmployeeLookupDto[]>([]);
  const [employeeTotal, setEmployeeTotal] = useState(0);
  const [employeeSearch, setEmployeeSearch] = useState('');
  const [employeeSearchLoading, setEmployeeSearchLoading] = useState(false);
  const [categories, setCategories] = useState<EvaluationCategoryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [form, setForm] = useState<CreateEvaluationRequest>({
    employeeId: '',
    categoryId: undefined,
    score: 0,
    scoreType: ScoreType.Positive,
    notes: '',
    evaluationDate: new Date().toISOString().split('T')[0],
  });

  const [selectedEmployee, setSelectedEmployee] = useState<EmployeeLookupDto | null>(null);
  const [selectedIndicatorEmployee, setSelectedIndicatorEmployee] = useState<EmployeeLookupDto | null>(null);
  const [indicatorEmployeeId, setIndicatorEmployeeId] = useState('');
  const [indicators, setIndicators] = useState<EmployeeIndicatorDto[]>([]);
  const [indicatorLoading, setIndicatorLoading] = useState(false);

  const searchEmployees = useCallback(async (query: string) => {
    setEmployeeSearchLoading(true);
    try {
      const res = await employeeService.lookup({ query: query || undefined, pageSize: 20 });
      const items = res.data?.items ?? [];
      setEmployeeOptions(items);
      setEmployeeTotal(res.data?.totalCount ?? items.length);
      return items;
    } catch {
      setEmployeeOptions([]);
      setEmployeeTotal(0);
      return [];
    } finally {
      setEmployeeSearchLoading(false);
    }
  }, []);

  useEffect(() => {
    const load = async () => {
      try {
        const [empItems, catRes] = await Promise.all([
          searchEmployees(''),
          api.get('/evaluations/categories'),
        ]);
        const categories = unwrapListItems<EvaluationCategoryDto>(catRes.data);
        setCategories(categories);

        if (empItems.length === 0) {
          setError(
            'لیست کارمندان خالی است. از تنظیمات → MIS دکمه «دریافت فهرست پرسنل» را بزنید.',
          );
        } else if (categories.length === 0) {
          setError('دسته‌بندی شاخص‌ها خالی است — دوباره وارد شوید یا database/14 را اجرا کنید.');
        } else {
          setError(null);
        }
      } catch (err: unknown) {
        const ax = err as { response?: { data?: { message?: string } } };
        setError(ax.response?.data?.message ?? readApiMessage(ax.response?.data) ?? 'خطا در بارگذاری داده‌های ارزیابی');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [searchEmployees]);

  useEffect(() => {
    const timer = setTimeout(() => {
      void searchEmployees(employeeSearch);
    }, 300);
    return () => clearTimeout(timer);
  }, [employeeSearch, searchEmployees]);

  useEffect(() => {
    if (!indicatorEmployeeId) {
      setIndicators([]);
      return;
    }

    const loadIndicators = async () => {
      setIndicatorLoading(true);
      setError(null);
      try {
        const res = await api.get(`/evaluations/employees/${indicatorEmployeeId}/indicators`);
        setIndicators(unwrapListItems<EmployeeIndicatorDto>(res.data));
      } catch {
        setError('خطا در بارگذاری شاخص‌های کارمند');
      } finally {
        setIndicatorLoading(false);
      }
    };
    loadIndicators();
  }, [indicatorEmployeeId]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.employeeId) {
      setError('لطفاً کارمند را انتخاب کنید');
      return;
    }
    setSaving(true);
    setError(null);
    setSuccess(false);
    try {
      await api.post('/evaluations', form);
      setSuccess(true);
      setForm((prev) => ({ ...prev, score: 0, notes: '' }));
    } catch {
      setError('خطا در ثبت ارزیابی');
    } finally {
      setSaving(false);
    }
  };

  const handleSaveIndicators = async () => {
    if (!indicatorEmployeeId) {
      setError('لطفاً کارمند را انتخاب کنید');
      return;
    }
    setSaving(true);
    setError(null);
    setSuccess(false);
    try {
      await api.put(`/evaluations/employees/${indicatorEmployeeId}/indicators`, {
        indicators: indicators.map((i) => ({
          categoryId: i.categoryId,
          weight: i.weight,
          isActive: i.isActive,
        })),
      });
      setSuccess(true);
    } catch {
      setError('خطا در ذخیره شاخص‌ها');
    } finally {
      setSaving(false);
    }
  };

  const employeeAutocomplete = (
    value: EmployeeLookupDto | null,
    onSelect: (emp: EmployeeLookupDto | null) => void,
  ) => (
    <Autocomplete
      options={employeeOptions}
      loading={employeeSearchLoading}
      getOptionLabel={(opt) => `${opt.fullName} (${opt.personnelCode})`}
      isOptionEqualToValue={(a, b) => a.id === b.id}
      value={value}
      onChange={(_, val) => onSelect(val)}
      onInputChange={(_, val, reason) => {
        if (reason === 'input') setEmployeeSearch(val);
      }}
      noOptionsText={
        employeeTotal === 0
          ? 'کارمندی یافت نشد — فهرست پرسنل را از MIS دریافت کنید'
          : 'موردی یافت نشد'
      }
      renderInput={(params) => (
        <TextField
          {...params}
          label="کارمند"
          required
          helperText={
            employeeTotal > 0
              ? `${employeeTotal} کارمند فعال — برای جستجو تایپ کنید`
              : undefined
          }
        />
      )}
    />
  );

  return (
    <Box>
      <LoadingOverlay open={loading || indicatorLoading} />
      <Typography variant="h5" sx={{ fontWeight: 700 }} gutterBottom>
        ارزیابی و شاخص‌های عملکرد
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        ثبت امتیاز و تنظیم وزن شاخص‌ها برای هر کارمند
      </Typography>

      <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 3 }}>
        <Tab label="ثبت امتیاز" />
        <Tab label="تنظیم شاخص‌های کارمند" icon={<TuneIcon />} iconPosition="start" />
      </Tabs>

      {success && (
        <Alert severity="success" sx={{ mb: 2 }}>
          عملیات با موفقیت انجام شد
        </Alert>
      )}
      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
          {employeeTotal === 0 && (
            <>
              {' '}
              <Link component={RouterLink} to="/settings?tab=mis">
                تنظیمات MIS
              </Link>
            </>
          )}
        </Alert>
      )}

      {tab === 0 && (
        <Card sx={{ ...glassCardSx(theme), maxWidth: 720 }}>
          <CardContent sx={{ p: 3 }}>
            <Box component="form" onSubmit={handleSubmit}>
              <Grid container spacing={2.5}>
                <Grid size={{ xs: 12 }}>
                  {employeeAutocomplete(selectedEmployee, (val) => {
                    setSelectedEmployee(val);
                    setForm((prev) => ({ ...prev, employeeId: val?.id ?? '' }));
                  })}
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    select
                    fullWidth
                    label="شاخص / دسته‌بندی"
                    value={form.categoryId ?? ''}
                    onChange={(e) =>
                      setForm((prev) => ({
                        ...prev,
                        categoryId: e.target.value || undefined,
                      }))
                    }
                  >
                    <MenuItem value="">بدون دسته</MenuItem>
                    {categories.map((cat) => (
                      <MenuItem key={cat.id} value={cat.id}>
                        {cat.name}
                      </MenuItem>
                    ))}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    select
                    fullWidth
                    label="نوع امتیاز"
                    value={form.scoreType}
                    onChange={(e) =>
                      setForm((prev) => ({
                        ...prev,
                        scoreType: Number(e.target.value) as ScoreType,
                      }))
                    }
                  >
                    {Object.entries(SCORE_TYPE_LABELS).map(([key, label]) => (
                      <MenuItem key={key} value={key}>
                        {label}
                      </MenuItem>
                    ))}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    type="number"
                    label="امتیاز"
                    value={form.score}
                    onChange={(e) =>
                      setForm((prev) => ({
                        ...prev,
                        score: Number(e.target.value),
                      }))
                    }
                    slotProps={{ htmlInput: { min: 0, max: 100, step: 0.5 } }}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    type="date"
                    label="تاریخ ارزیابی (میلادی)"
                    value={form.evaluationDate}
                    onChange={(e) =>
                      setForm((prev) => ({
                        ...prev,
                        evaluationDate: e.target.value,
                      }))
                    }
                    slotProps={{ inputLabel: { shrink: true } }}
                  />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField
                    fullWidth
                    multiline
                    rows={3}
                    label="توضیحات"
                    value={form.notes}
                    onChange={(e) =>
                      setForm((prev) => ({ ...prev, notes: e.target.value }))
                    }
                  />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <Button
                    type="submit"
                    variant="contained"
                    startIcon={<SaveIcon />}
                    disabled={saving}
                  >
                    ثبت ارزیابی
                  </Button>
                </Grid>
              </Grid>
            </Box>
          </CardContent>
        </Card>
      )}

      {tab === 1 && (
        <Card sx={{ ...glassCardSx(theme), maxWidth: 720 }}>
          <CardContent sx={{ p: 3 }}>
            <Grid container spacing={2.5}>
              <Grid size={{ xs: 12 }}>
                {employeeAutocomplete(selectedIndicatorEmployee, (val) => {
                  setSelectedIndicatorEmployee(val);
                  setIndicatorEmployeeId(val?.id ?? '');
                })}
              </Grid>
              {indicators.map((indicator, index) => (
                <Grid size={{ xs: 12 }} key={indicator.categoryId}>
                  <Box
                    sx={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: 2,
                      flexWrap: 'wrap',
                      p: 2,
                      borderRadius: 2,
                      bgcolor: 'action.hover',
                    }}
                  >
                    <Box sx={{ flex: 1, minWidth: 160 }}>
                      <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
                        {indicator.categoryName}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        وزن پیش‌فرض سازمان: {indicator.defaultWeight}
                      </Typography>
                    </Box>
                    <TextField
                      type="number"
                      label="وزن"
                      size="small"
                      value={indicator.weight}
                      onChange={(e) => {
                        const weight = Number(e.target.value);
                        setIndicators((prev) =>
                          prev.map((item, i) =>
                            i === index ? { ...item, weight } : item,
                          ),
                        );
                      }}
                      slotProps={{ htmlInput: { min: 0, max: 100, step: 1 } }}
                      sx={{ width: 120 }}
                    />
                    <FormControlLabel
                      control={
                        <Switch
                          checked={indicator.isActive}
                          onChange={(e) => {
                            const isActive = e.target.checked;
                            setIndicators((prev) =>
                              prev.map((item, i) =>
                                i === index ? { ...item, isActive } : item,
                              ),
                            );
                          }}
                        />
                      }
                      label="فعال"
                    />
                  </Box>
                </Grid>
              ))}
              {indicatorEmployeeId && indicators.length === 0 && !indicatorLoading && (
                <Grid size={{ xs: 12 }}>
                  <Alert severity="info">
                    شاخصی تعریف نشده — پس از دریافت فهرست پرسنل از MIS، دسته‌بندی‌ها خودکار ساخته می‌شوند.
                  </Alert>
                </Grid>
              )}
              <Grid size={{ xs: 12 }}>
                <Button
                  variant="contained"
                  startIcon={<SaveIcon />}
                  disabled={saving || !indicatorEmployeeId}
                  onClick={handleSaveIndicators}
                >
                  ذخیره شاخص‌های کارمند
                </Button>
              </Grid>
            </Grid>
          </CardContent>
        </Card>
      )}
    </Box>
  );
}
