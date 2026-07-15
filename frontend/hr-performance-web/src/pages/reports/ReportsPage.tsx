import { useState } from 'react';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Grid from '@mui/material/Grid';
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import MenuItem from '@mui/material/MenuItem';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Alert from '@mui/material/Alert';
import DownloadIcon from '@mui/icons-material/Download';
import AssessmentIcon from '@mui/icons-material/Assessment';
import { useTheme } from '@mui/material/styles';
import { glassCardSx } from '../../theme/theme';

const reportTypes = [
  { value: 'employee', label: 'گزارش عملکرد کارمند' },
  { value: 'department', label: 'گزارش عملکرد واحد' },
  { value: 'attendance', label: 'گزارش حضور و غیاب' },
  { value: 'evaluation', label: 'گزارش ارزیابی‌ها' },
];

export default function ReportsPage() {
  const theme = useTheme();
  const [reportType, setReportType] = useState('employee');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [generated, setGenerated] = useState(false);

  const handleGenerate = () => {
    setGenerated(true);
  };

  return (
    <Box>
      <Typography variant="h5" sx={{ fontWeight: 700 }} gutterBottom>
        گزارش‌ها
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        تولید و دانلود گزارش‌های عملکرد
      </Typography>

      <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: 5 }}>
          <Card sx={{ ...glassCardSx(theme) }}>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="h6" sx={{ fontWeight: 600 }} gutterBottom>
                پارامترهای گزارش
              </Typography>

              <TextField
                select
                fullWidth
                label="نوع گزارش"
                value={reportType}
                onChange={(e) => setReportType(e.target.value)}
                sx={{ mb: 2.5 }}
              >
                {reportTypes.map((rt) => (
                  <MenuItem key={rt.value} value={rt.value}>
                    {rt.label}
                  </MenuItem>
                ))}
              </TextField>

              <TextField
                fullWidth
                type="date"
                label="از تاریخ"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                slotProps={{ inputLabel: { shrink: true } }}
                sx={{ mb: 2.5 }}
              />

              <TextField
                fullWidth
                type="date"
                label="تا تاریخ"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                slotProps={{ inputLabel: { shrink: true } }}
                sx={{ mb: 3 }}
              />

              <Button
                variant="contained"
                fullWidth
                startIcon={<AssessmentIcon />}
                onClick={handleGenerate}
                disabled={!startDate || !endDate}
              >
                تولید گزارش
              </Button>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 7 }}>
          <Card sx={{ ...glassCardSx(theme), minHeight: 300 }}>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="h6" sx={{ fontWeight: 600 }} gutterBottom>
                پیش‌نمایش گزارش
              </Typography>

              {generated ? (
                <Box>
                  <Alert severity="success" sx={{ mb: 2 }}>
                    گزارش با موفقیت تولید شد
                  </Alert>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                    نوع: {reportTypes.find((r) => r.value === reportType)?.label}
                    <br />
                    بازه: {startDate} تا {endDate}
                  </Typography>
                  <Button variant="outlined" startIcon={<DownloadIcon />}>
                    دانلود PDF
                  </Button>
                  <Button variant="outlined" startIcon={<DownloadIcon />} sx={{ mr: 1 }}>
                    دانلود Excel
                  </Button>
                </Box>
              ) : (
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    height: 200,
                    color: 'text.secondary',
                  }}
                >
                  <Typography>
                    پارامترها را انتخاب کرده و گزارش را تولید کنید
                  </Typography>
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
