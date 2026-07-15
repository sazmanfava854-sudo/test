import { useEffect, useState } from 'react';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import MenuItem from '@mui/material/MenuItem';
import Grid from '@mui/material/Grid';
import Alert from '@mui/material/Alert';
import Autocomplete from '@mui/material/Autocomplete';
import SaveIcon from '@mui/icons-material/Save';
import api from '../../services/api';
import { employeeService } from '../../services/employeeService';
import LoadingOverlay from '../../components/common/LoadingOverlay';
import { glassCardSx } from '../../theme/theme';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import { useTheme } from '@mui/material/styles';
import {
  type EmployeeDto,
  type EvaluationCategoryDto,
  type CreateEvaluationRequest,
  ScoreType,
  SCORE_TYPE_LABELS,
} from '../../types';

export default function EvaluationPage() {
  const theme = useTheme();
  const [employees, setEmployees] = useState<EmployeeDto[]>([]);
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

  useEffect(() => {
    const load = async () => {
      try {
        const [empRes, catRes] = await Promise.all([
          employeeService.getAll({ pageSize: 100 }),
          api.get('/evaluations/categories'),
        ]);
        if (empRes.success && empRes.data) {
          setEmployees(empRes.data.items);
        }
        const catData = catRes.data?.data ?? catRes.data;
        if (Array.isArray(catData)) {
          setCategories(catData);
        }
      } catch {
        /* use empty lists */
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

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

  const selectedEmployee = employees.find((e) => e.id === form.employeeId);

  return (
    <Box>
      <LoadingOverlay open={loading} />
      <Typography variant="h5" sx={{ fontWeight: 700 }} gutterBottom>
        ثبت ارزیابی دستی
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        ثبت امتیاز عملکرد برای کارمندان
      </Typography>

      <Card sx={{ ...glassCardSx(theme), maxWidth: 720 }}>
        <CardContent sx={{ p: 3 }}>
          {success && (
            <Alert severity="success" sx={{ mb: 2 }}>
              ارزیابی با موفقیت ثبت شد
            </Alert>
          )}
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          <Box component="form" onSubmit={handleSubmit}>
            <Grid container spacing={2.5}>
              <Grid size={{ xs: 12 }}>
                <Autocomplete
                  options={employees}
                  getOptionLabel={(opt) => `${opt.fullName} (${opt.personnelCode})`}
                  value={selectedEmployee ?? null}
                  onChange={(_, val) =>
                    setForm((prev) => ({ ...prev, employeeId: val?.id ?? '' }))
                  }
                  renderInput={(params) => (
                    <TextField {...params} label="کارمند" required />
                  )}
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  select
                  fullWidth
                  label="دسته‌بندی"
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
                  label="تاریخ ارزیابی"
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
    </Box>
  );
}
